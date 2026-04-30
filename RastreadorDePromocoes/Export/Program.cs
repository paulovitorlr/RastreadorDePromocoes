using MercadoLivre.Bot;

namespace MercadoLivre.Bot
{
    public class Program
    {
        public static async Task Main()
        {
            var rastreador = new Rastreador();
            var raw = rastreador.TestWeb();
            Console.WriteLine($"{raw.Count} produtos coletados.");

            var analyzer = new DiscountAnalyzer();
            var ranked = analyzer.EnrichWithDiscount( raw );


            var scheduler = new BatchScheduler(batchSize: 3, intervalSeconds: 30);
            var batches = scheduler.CreateBatches( ranked );

            var report = new ReportService();
            report.SaveFullRankingToCsv( raw );

            await scheduler.ProcessBatchesAsync(batches, async (batch, number) =>
            {
                foreach (var p in batch)
                    Console.WriteLine($"[{p.DiscountPercent}% off] {p.Title} - R$ {p.PriceDecimal}");
                report.SaveBatchToCsv(batch, number);
                await Task.CompletedTask;
            });

            Console.WriteLine("\nProcessamento finalizado!");
        }
    }
}





