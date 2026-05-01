using Npgsql;
using DotNetEnv;
using System;

namespace MercadoLivre.Bot.Database
{
    public class DbContext
    {
        private readonly string _connectionString;

        public DbContext()
        {
            // Sobe pastas até encontrar o .env na raiz do projeto
            var basePath = Directory.GetCurrentDirectory();
            var envPath = Path.Combine(basePath, ".env");

            // Se não achar no diretório atual, sobe até 4 níveis
            for (int i = 0; i < 4; i++)
            {
                if (File.Exists(envPath))
                    break;

                basePath = Directory.GetParent(basePath)?.FullName ?? basePath;
                envPath = Path.Combine(basePath, ".env");
            }

            Env.Load(envPath);
            Console.WriteLine($"[ENV] Carregado de: {envPath}");

            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var database = Environment.GetEnvironmentVariable("DB_NAME");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (string.IsNullOrEmpty(host))
                throw new Exception($"[ENV] Variáveis não carregadas. Arquivo .env encontrado em: {envPath}");

            _connectionString =
                $"Host={host};Port={port};Database={database};Username={user};Password={password};";
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        /// <summary>
        /// Cria a tabela products caso ela ainda não exista.
        /// Chamado uma vez na inicialização da aplicação.
        /// </summary>
        public void EnsureCreated()
        {
            using var conn = GetConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
               CREATE TABLE IF NOT EXISTS products (
                    id              SERIAL PRIMARY KEY,
                    title           TEXT        NOT NULL UNIQUE,
                    price           VARCHAR(50) NOT NULL,
                    original_price  VARCHAR(50),
                    url             TEXT,
                    scraped_at      TIMESTAMP   NOT NULL DEFAULT NOW()
                );
                ", conn);

            cmd.ExecuteNonQuery();
            Console.WriteLine("[DB] Tabela 'products' verificada/criada com sucesso.");
        }
    }
}