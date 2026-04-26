using System;
using System.Collections.Generic;
using System.Text;

namespace MercadoLivre.Bot
{
    public class DiscountAnalyzer
    {
       private decimal Parseprice(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0;
            var cleaned = raw.Replace(".", "").Replace(",", ".").Trim();
            return decimal.TryParse(cleaned, out var val) ? val : 0;
        }

        public List<Product> EnrichWithDiscount(List<Product> products)
        {
            foreach(var p in products)
            {
                p.PriceDecimal = Parseprice(p.Price);
                p.OriginalPriceDecimal = Parseprice(p.OriginalPrice);
                
                if(p.OriginalPriceDecimal > 0 && p.PriceDecimal > 0 
                    && p.OriginalPriceDecimal > p.PriceDecimal)
                {
                    p.DiscountPercent = Math.Round(
                        (1 - p.PriceDecimal / p.OriginalPriceDecimal) * 100, 2
                        );
                }
            }
            return products.OrderByDescending(p => p.DiscountPercent).ToList();
        }

        
    }
}
