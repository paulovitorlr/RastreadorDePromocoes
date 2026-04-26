using MercadoLivre.Bot;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var ProdutosBody = new List<Product>();

            driver.Navigate().GoToUrl("https://lista.mercadolivre.com.br/celulares_Desde_0_OrderId_RECENT");

            //aguarda os cards carregarem
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            wait.Until(d => d.FindElements(
                By.XPath("//li[contains(@class,'ui-search-layout__item')]")
            ).Count > 0);
            
            //pega todos os cards da pagina
            var cards = driver.FindElements(
                By.XPath("//li[contains(@class,'ui-search-layout__item')]")
            );

            foreach (var card in cards)
            {
                try
                {
                    var title = card.FindElement(By.XPath(".//h3")).Text;

                    var price = card.FindElement(
                        By.XPath(".//span[contains(@class,'andes-money-amount__fraction')]")
                    ).Text;

                    //preco "de"(riscado) nem todos tem desconto, por isso um try separado.
                    string originalPrice = "";

                    try
                    {
                        originalPrice = card.FindElement(By.XPath(
                            ".//s[contains(@class,'andes-money-amount')]//span[contains(@class,'andes-money-amount__fraction')]")).Text;
                    }
                    catch
                    {

                    }

                    string url = "";
                    
                    try
                    {
                        url = card.FindElement(By.XPath(".//a")).GetAttribute("href");
                    }
                    catch
                    {

                    }

                    ProdutosBody.Add(new Product()
                    {
                        Title = title,
                        Price = price,
                        OriginalPrice = originalPrice,
                        Url = url
                  
                    });
                }
                catch
                {
                    continue;
                }
            }

            return ProdutosBody;
        }
        public void Fechar()
        {
            driver.Quit();
        }

    }
    
}
