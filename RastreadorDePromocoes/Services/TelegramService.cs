using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MercadoLivre.Bot
{
    public class TelegramService
    {
        private readonly TelegramBotClient _bot;
        private readonly long _chatId;

        public TelegramService()
        {
            var token = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN")
                ?? throw new Exception("[TELEGRAM] TELEGRAM_TOKEN não encontrado no .env");

            var chatIdStr = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID")
                ?? throw new Exception("[TELEGRAM] TELEGRAM_CHAT_ID não encontrado no .env");

            _chatId = long.Parse(chatIdStr);
            _bot = new TelegramBotClient(token, new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            });
        }

        public async Task EnviarPromocao(Product p)
        {
            if (p.DiscountPercent <= 0) return;

            var preco = $"R$ {p.PriceDecimal:N2}";
            var precoOriginal = p.OriginalPriceDecimal > 0
                ? $"<s>R$ {p.OriginalPriceDecimal:N2}</s>"
                : "";

            var mensagem = $"""
        🔥 <b>PROMOÇÃO MERCADO LIVRE</b>

        📦 {p.Title}
        💰 Preço: <b>{preco}</b>
        {(precoOriginal != "" ? $"📉 De: {precoOriginal}" : "")}
        🏷️ Desconto: <b>{p.DiscountPercent}% OFF</b>
        🔗 <a href="{p.Url}">Ver oferta</a>
        """;

            await _bot.SendMessage(
                chatId: _chatId,
                text: mensagem,
                parseMode: ParseMode.Html
            );

            Console.WriteLine($"[TELEGRAM] ✅ Enviado: {p.Title}");
        }

        public async Task EnviarLote(List<Product> batch, int numero)
        {
            Console.WriteLine($"\n[TELEGRAM] Enviando lote {numero} ({batch.Count} produtos)...");

            foreach (var p in batch)
            {
                await EnviarPromocao(p);
                await Task.Delay(1500); // Evita flood limit do Telegram
            }
        }

        // MarkdownV2 exige escape de caracteres especiais
        private static string EscapeMarkdown(string text)
        {
            var chars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
            foreach (var c in chars)
                text = text.Replace(c, "\\" + c);
            return text;
        }
    }
}