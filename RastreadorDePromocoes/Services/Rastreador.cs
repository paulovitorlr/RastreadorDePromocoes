using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
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

            driver.Navigate().GoToUrl("https://lista.mercadolivre.com.br/_Container_mais-vendidos-de-smf?skipInApp=true&matt_ignore=true");

            var elementosBody = driver.FindElements(By.XPath("//li[contains(@class,'ui-search-layout__item')]"));

            foreach (var elemento in elementosBody)
            {
                var price = elemento.FindElement(By.XPath(".//span[contains(@class,'andes-money-amount__fraction')]")).Text;
                
                var title = elemento.FindElement(By.XPath(".//h3")).Text;


                var product = new Product() { Price = price, Title = title };

                ProdutosBody.Add(product);
            }
            return ProdutosBody;
        }
      
    }
    
}
