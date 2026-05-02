using Npgsql;
using System;
using System.Collections.Generic;

namespace MercadoLivre.Bot.Database
{
    public class ProductRepository
    {
        private readonly DbContext _context;

        public ProductRepository(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Insere o produto somente se a URL ainda não existir no banco.
        /// Retorna true se inseriu, false se já existia (duplicata ignorada).
        /// </summary>
        public bool InsertIfNotExists(Product product)
        {
            using var conn = _context.GetConnection();
            conn.Open();

            // ON CONFLICT DO NOTHING → se a URL já existe, não lança erro, só ignora
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO products (title, price, original_price, url)
                VALUES (@title, @price, @original_price, @url)
                ON CONFLICT (title) DO NOTHING;
            ", conn);

            cmd.Parameters.AddWithValue("title", product.Title);
            cmd.Parameters.AddWithValue("price", product.Price);
            cmd.Parameters.AddWithValue("original_price", (object?)product.OriginalPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("url", product.Url);

            // ExecuteNonQuery retorna 0 se o ON CONFLICT ignorou (duplicata)
            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        /// <summary>
        /// Insere uma lista inteira, logando o resultado de cada item.
        /// </summary>
        public void InsertMany(List<Product> products)
        {
            int inseridos = 0;
            int duplicatas = 0;

            foreach (var product in products)
            {
                bool inserido = InsertIfNotExists(product);

                if (inserido)
                {
                    inseridos++;
                    Console.WriteLine($"[DB] ✅ Salvo: {product.Title}");
                }
                else
                {
                    duplicatas++;
                    Console.WriteLine($"[DB] ⚠️  Duplicata ignorada: {product.Title}");
                }
            }

            Console.WriteLine($"\n[DB] Resultado: {inseridos} inseridos, {duplicatas} duplicatas ignoradas.");
        }
    


    public List<Product> GetAll()
        {
            var products = new List<Product>();

            using var conn = _context.GetConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT title, price, original_price, url FROM products ORDER BY scraped_at DESC;", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Title = reader.GetString(0),
                    Price = reader.GetString(1),
                    OriginalPrice = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Url = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return products;
        }
    }
}