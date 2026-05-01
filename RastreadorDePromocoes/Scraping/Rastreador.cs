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
                    // TÍTULO — tag é h3 > a com classe poly-component__title
                    var titleElement = card.FindElements(
                        By.XPath(".//a[contains(@class,'poly-component__title')]")
                    ).FirstOrDefault();

                    if (titleElement == null) continue;
                    var title = titleElement.Text;

                    // URL — mesmo elemento do título já tem o href
                    string url = titleElement.GetAttribute("href");

                    // PREÇO ATUAL — dentro de div.poly-price__current
                    var currentPriceElement = card.FindElements(
                        By.XPath(".//div[contains(@class,'poly-price__current')]" +
                                 "//span[contains(@class,'andes-money-amount__fraction')]")
                    ).FirstOrDefault();

                    if (currentPriceElement == null) continue; // sem preço → ignora

                    string price = currentPriceElement.Text;

                    // PREÇO ORIGINAL (riscado) — span com classe andes-money-amount--previous
                    string? originalPrice = null;
                    var oldPriceElement = card.FindElements(
                        By.XPath(".//s[contains(@class,'andes-money-amount--previous')]" +
                                 "//span[contains(@class,'andes-money-amount__fraction')]")
                    ).FirstOrDefault();

                    if (oldPriceElement != null)
                        originalPrice = oldPriceElement.Text;

                    produtos.Add(new Product()
                    {
                        Title = title,
                        Price = price,
                        OriginalPrice = originalPrice,
                        Url = url
                    });

                    Console.WriteLine("-----");
                    Console.WriteLine($"Produto: {title}");
                    Console.WriteLine($"Preço atual: {price}");
                    Console.WriteLine($"Preço original: {originalPrice ?? "sem desconto"}");
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