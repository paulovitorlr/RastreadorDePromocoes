


namespace MercadoLivre.Bot
{
    public class Program
    {
        

        public static void Main()
        {
            var web = new Rastreador();
            var csvService = new CsvService();

            var itens = web.TestWeb();

            web.TestWeb();

            csvService.SaveToCsv(itens, "products.csv");

            Console.WriteLine("Scraping finalizado!");
        }
    }
}





