using LibraryManagementSystem.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LibraryManagementSystem.DataAccess
{
    public class DataRepositoryFactory
    {
        private readonly IConfiguration _configuration;

        public DataRepositoryFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IDataRepository> CreateRepositoryAsync()
        {
            var dataSource = _configuration["DataSource"];

            switch (dataSource?.ToLower())
            {
                case "postgresql":
                    return await CreatePostgreSqlRepositoryAsync();
                case "file":
                default:
                    return CreateFileRepository();
            }
        }

        public async Task<IDataRepository> CreateRepositoryAsync(string dataSource)
        {
            switch (dataSource?.ToLower())
            {
                case "postgresql":
                    return await CreatePostgreSqlRepositoryAsync();
                case "file":
                default:
                    return CreateFileRepository();
            }
        }

        private async Task<IDataRepository> CreatePostgreSqlRepositoryAsync()
        {
            var connectionString = _configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("PostgreSQL connection string is not configured.");
            }

            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseNpgsql(connectionString)
                .Options;

            var context = new LibraryContext(options);
            var repository = new PostgreSqlDataRepository(context);

            // Test connection and initialize database
            if (!await repository.TestConnectionAsync())
            {
                throw new InvalidOperationException("Cannot connect to PostgreSQL database.");
            }

            await repository.InitializeAsync();
            return repository;
        }

        private IDataRepository CreateFileRepository()
        {
            var dataDirectory = _configuration["FileDataSettings:DataDirectory"] ?? "Data";
            return new FileDataRepository(dataDirectory);
        }

        public static DataRepositoryFactory Create()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return new DataRepositoryFactory(configuration);
        }
    }
}