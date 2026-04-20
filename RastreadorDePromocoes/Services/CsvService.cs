using CsvHelper;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;

namespace MercadoLivre.Bot
{
    internal class CsvService
    {
        public void SaveToCsv(List<Product> itens, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(itens);
        }

       
    }
}
