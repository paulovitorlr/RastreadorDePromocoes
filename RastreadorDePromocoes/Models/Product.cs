using System;
using System.Collections.Generic;
using System.Text;

namespace MercadoLivre.Bot
{
    public class Product
    {
        public string? Title {  get; set; }
        public string? Price { get; set; } //Pega o preço Atual
        public string? OriginalPrice { get; set; } //Pega o preço "de"
        public string? Url { get; set; }
        public decimal PriceDecimal { get; set; }
        public decimal OriginalPriceDecimal { get; set; }
        public decimal DiscountPercent { get; set; }
        public int BatchGroup {  get; set; }
       

    }
}
