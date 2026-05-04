using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace MercadoLivre.Bot
{
    public class Rastreador
    {
        public IWebDriver driver;

        public Rastreador()
        {
            var options = new ChromeOptions();
            // Removido --headless para você ver o que está acontecendo
            // Adicione de volta depois que funcionar:
            // options.AddArgument("--headless=new");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
        }

        public List<Product> TestWeb()
        {
            var produtos = new List<Product>();

            var urls = new List<string>
            {
                "https://www.mercadolivre.com.br/ofertas",
                "https://lista.mercadolivre.com.br/mais-vendidos"
            };

            foreach (var url in urls)
            {
                Console.WriteLine($"\n[BOT] Acessando: {url}");
                driver.Navigate().GoToUrl(url);

                // Aguarda carregamento inicial
                Thread.Sleep(3000);

                // Loga o título da página para confirmar que carregou
                Console.WriteLine($"[BOT] Título da página: {driver.Title}");

                // Tenta rolar a página para forçar carregamento lazy
                ((IJavaScriptExecutor)driver).ExecuteScript(
                    "window.scrollTo(0, document.body.scrollHeight / 2);"
                );
                Thread.Sleep(2000);

                // Loga o HTML parcial para diagnóstico
                var bodySnippet = driver.FindElement(By.TagName("body")).Text;
                Console.WriteLine($"[BOT] Primeiros 300 chars do body:\n{bodySnippet[..Math.Min(300, bodySnippet.Length)]}");

                // Tenta múltiplos seletores conhecidos do ML
                var seletores = new[]
                {
                    "//li[contains(@class,'promotion-item')]",
                    "//li[contains(@class,'ui-search-layout__item')]",
                    "//div[contains(@class,'promotion-item')]",
                    "//article[contains(@class,'ui-search-layout__item')]",
                    "//*[@data-testid='promotion-item']",
                };

                IReadOnlyCollection<IWebElement>? cards = null;

                foreach (var seletor in seletores)
                {
                    var found = driver.FindElements(By.XPath(seletor));
                    Console.WriteLine($"[BOT] Seletor '{seletor}': {found.Count} elementos");
                    if (found.Count > 0)
                    {
                        cards = found;
                        break;
                    }
                }

                if (cards == null || cards.Count == 0)
                {
                    Console.WriteLine($"[BOT] Nenhum card encontrado em {url}, pulando...");
                    continue;
                }

                Console.WriteLine($"[BOT] {cards.Count} cards encontrados!");

                foreach (var card in cards)
                {
                    try
                    {
                        // Loga o HTML do primeiro card para diagnóstico
                        if (produtos.Count == 0)
                        {
                            var html = card.GetAttribute("innerHTML");
                            Console.WriteLine($"[DEBUG] HTML do 1º card:\n{html[..Math.Min(500, html.Length)]}");
                        }

                        var titleElement =
                            card.FindElements(By.XPath(".//*[contains(@class,'poly-component__title')]")).FirstOrDefault()
                            ?? card.FindElements(By.XPath(".//*[contains(@class,'promotion-item__title')]")).FirstOrDefault()
                            ?? card.FindElements(By.XPath(".//h2")).FirstOrDefault()
                            ?? card.FindElements(By.XPath(".//h3")).FirstOrDefault();

                        if (titleElement == null) continue;
                        var title = titleElement.Text.Trim();
                        if (string.IsNullOrWhiteSpace(title)) continue;

                        var linkElement =
                            card.FindElements(By.XPath(".//a[contains(@class,'poly-component__title')]")).FirstOrDefault()
                            ?? card.FindElements(By.XPath(".//a[contains(@class,'promotion-item__title')]")).FirstOrDefault()
                            ?? card.FindElements(By.XPath(".//a")).FirstOrDefault();

                        string? cardUrl = linkElement?.GetAttribute("href");

                        var currentPriceElement =
                            card.FindElements(By.XPath(
                                ".//*[contains(@class,'poly-price__current')]//*[contains(@class,'andes-money-amount__fraction')]"
                            )).FirstOrDefault()
                            ?? card.FindElements(By.XPath(
                                ".//*[contains(@class,'andes-money-amount__fraction')]"
                            )).FirstOrDefault();

                        if (currentPriceElement == null) continue;
                        string price = currentPriceElement.Text;

                        string? originalPrice = card.FindElements(By.XPath(
                            ".//s[contains(@class,'andes-money-amount--previous')]//*[contains(@class,'andes-money-amount__fraction')]"
                        )).FirstOrDefault()?.Text;

                        if (produtos.Any(p => p.Url == cardUrl)) continue;

                        produtos.Add(new Product
                        {
                            Title = title,
                            Price = price,
                            OriginalPrice = originalPrice,
                            Url = cardUrl
                        });

                        Console.WriteLine($"✅ {title} | R$ {price} | De: {originalPrice ?? "sem desconto"}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERRO] Card: {ex.Message}");
                    }
                }
            }

            driver.Quit();
            return produtos;
        }

        public void Fechar() => driver.Quit();
    }
}