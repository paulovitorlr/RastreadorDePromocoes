using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;

namespace MercadoLivre.Bot
{
    public class ReportService
    {
        public void SaveBatchToCsv(List<Product> batch, int batchNumber, string outputDir = "output")
        {
            Directory.CreateDirectory(outputDir);
            var path = Path.Combine(outputDir, $"lote_{batchNumber:D2}.csv");

            using var write = new StreamWriter(path);
            using var csv = new CsvWriter(write, CultureInfo.InvariantCulture);
            csv.WriteRecords(batch);

            Console.WriteLine($"[Lote {batchNumber}] Salvo em {path}");
        }

        public void SaveFullRankingToCsv(List<Product> all, string outputDir = "output")
        {
            Directory.CreateDirectory(outputDir);
            var path = Path.Combine(outputDir, "ranking_completo.csv");

            using var writer = new StreamWriter(path);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(all);
        }

    }
}
