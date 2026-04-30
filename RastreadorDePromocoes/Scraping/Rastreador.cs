using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MercadoLivre.Bot
{
    public class Rastreador
    {
        public IWebDriver driver;

        public Rastreador()
        {
            driver = new ChromeDriver();
        }

        public List<Product> TestWeb()
        {
            var produtos = new List<Product>();

            driver.Navigate().GoToUrl("https://lista.mercadolivre.com.br/celulares_Desde_0_OrderId_RECENT");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            wait.Until(d => d.FindElements(
                By.XPath("//li[contains(@class,'ui-search-layout__item')]")
            ).Count > 0);

            var cards = driver.FindElements(
                By.XPath("//li[contains(@class,'ui-search-layout__item')]")
            );

            foreach (var card in cards)
            {
                try
                {
                    var title = card.FindElement(By.XPath(".//h3")).Text;

                    string url = "";
                    var linkElement = card.FindElements(By.XPath(".//a")).FirstOrDefault();
                    if (linkElement != null)
                        url = linkElement.GetAttribute("href");

                    // PEGA TODOS OS PREÇOS DO CARD
                    var priceElements = card.FindElements(By.XPath(
                        ".//span[contains(@class,'andes-money-amount__fraction')]"
                    ));

                    var values = priceElements
                        .Select(e => e.Text)
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();

                    string price = "";
                    string? originalPrice = null;

                    if (values.Count == 1)
                    {
                        // sem desconto
                        price = values[0];
                    }
                    else if (values.Count >= 2)
                    {
                        // converte para decimal
                        var parsed = values
                        .Select(v => decimal.Parse(v.Replace(".", "").Replace(",", ".")))
                        .Where(v => v > 800) // ignora parcelas pequenas
                        .OrderBy(v => v)
                        .ToList();

                        if (parsed.Count == 1)
                        {
                            price = parsed[0].ToString();
                        }
                        else if (parsed.Count >= 2)
                        {
                            price = parsed[0].ToString();
                            originalPrice = parsed.Last().ToString();
                        } // maior = preço original
                    }
                    else
                    {
                        continue; // sem preço → ignora
                    }

                    produtos.Add(new Product()
                    {
                        Title = title,
                        Price = price,
                        OriginalPrice = originalPrice,
                        Url = url
                    });

                    // DEBUG
                    Console.WriteLine("-----");
                    Console.WriteLine($"Produto: {title}");
                    Console.WriteLine($"Preço atual: {price}");
                    Console.WriteLine($"Preço original: {originalPrice}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro no card: " + ex.Message);
                    continue;
                }
            }

            return produtos;
        }

        public void Fechar()
        {
            driver.Quit();
        }
    }
}