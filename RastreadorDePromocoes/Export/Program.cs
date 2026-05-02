using MercadoLivre.Bot;
using MercadoLivre.Bot.Database;

namespace MercadoLivre.Bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            bool apenasEnviar = args.Contains("--enviar");

            if (!apenasEnviar)
            {
                // ─── ETAPA 1: Scraping (sem VPN) ───
                Console.WriteLine("[BOT] Iniciando scraping...");

                var rastreador = new Rastreador();
                var raw = rastreador.TestWeb();
                Console.WriteLine($"[BOT] {raw.Count} produtos coletados.");

                var dbContext = new DbContext();
                dbContext.EnsureCreated();
                var repository = new ProductRepository(dbContext);
                repository.InsertMany(raw);

                var report = new ReportService();
                report.SaveFullRankingToCsv(raw);

                Console.WriteLine("\n[BOT] Scraping concluído! Ligue a VPN e rode com --enviar");
                return;
            }

            // ─── ETAPA 2: Envio pro Telegram (com VPN) ───
            Console.WriteLine("[BOT] Iniciando envio para o Telegram...");

            var db = new DbContext();
            var repo = new ProductRepository(db);
            var produtos = repo.GetAll();

            if (produtos.Count == 0)
            {
                Console.WriteLine("[BOT] Nenhum produto no banco. Rode sem --enviar primeiro.");
                return;
            }

            Console.WriteLine($"[BOT] {produtos.Count} produtos carregados do banco.");

            var analyzer = new DiscountAnalyzer();
            var ranked = analyzer.EnrichWithDiscount(produtos);

            var scheduler = new BatchScheduler(batchSize: 3, intervalSeconds: 30);
            var batches = scheduler.CreateBatches(ranked);

            var telegram = new TelegramService();

            await scheduler.ProcessBatchesAsync(batches, async (batch, number) =>
            {
                foreach (var p in batch)
                    Console.WriteLine($"[{p.DiscountPercent}% off] {p.Title} - R$ {p.Price}");

                await telegram.EnviarLote(batch, number);
            });

            Console.WriteLine("\n[BOT] Envio finalizado!");
        }
    }
}