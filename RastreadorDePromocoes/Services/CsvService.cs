using CsvHelper;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;

namespace MercadoLivre.Bot
{
    public class CsvService
    {
        public void SaveToCsv(List<Product> itens, string filePath)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Escreve o cabeçalho manualmente
            csv.WriteHeader<Product>();
            csv.NextRecord();

            // Escreve cada produto individualmente
            foreach (var item in itens)
            {
                csv.WriteRecord(item);
                csv.NextRecord();
            }
        }
    }
}
