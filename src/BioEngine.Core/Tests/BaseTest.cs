using System;
using BioEngine.Core.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace BioEngine.Core.Tests
{
    public abstract class BaseTest
    {
        protected readonly ILoggerFactory LoggerFactory;
        protected readonly ILogger Logger;

        protected BaseTest(ITestOutputHelper testOutputHelper)
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            Logger = LoggerFactory.CreateLogger<BaseTest>();
        }
    }

    public abstract class BaseTest<T> : BaseTest, IClassFixture<T>, IDisposable where T : BaseTestFixture
    {
        private readonly BaseTestFixture _testFixture;


        protected BaseTest(BaseTestFixture testFixture, ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _testFixture = testFixture;
        }

        protected BioContext GetDbContext(string databaseName, bool inMemory = true, bool init = true)
        {
            return _testFixture.GetDbContext(LoggerFactory, databaseName, inMemory, init);
        }

        public void Dispose()
        {
            LoggerFactory?.Dispose();
        }
    }

    public abstract class BaseTestFixture
    {
        public BaseTestFixture()
        {
            Configuration = new ConfigurationBuilder()
                .AddUserSecrets("bw")
                .AddEnvironmentVariables()
                .Build();
        }

        public BioContext GetDbContext(ILoggerFactory loggerFactory, string databaseName = "",
            bool inMemory = true, bool needInit = true)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BioContext>()
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(loggerFactory);
            DbContextOptions<BioContext> options;
            if (inMemory)
            {
                options =
                    optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                        .UseInMemoryDatabase(databaseName).Options;
            }
            else
            {
                options = optionsBuilder
                    .UseNpgsql(GetConnectionString(databaseName))
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options;
            }

            ModifyDbOptions(options, inMemory);
            var dbContext = new BioContext(options);
            if (dbContext == null)
            {
                throw new Exception("Can't create db context");
            }

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            if (needInit)
            {
                InitDbContext(dbContext);
            }

            return dbContext;
        }

        protected virtual string GetConnectionString(string databaseName)
        {
            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = Configuration["BIO_POSTGRES_HOST"] ?? "localhost",
                Port =
                    !string.IsNullOrEmpty(Configuration["BIO_POSTGRES_PORT"])
                        ? int.Parse(Configuration["BIO_POSTGRES_PORT"])
                        : 5432,
                Username = Configuration["BIO_POSTGRES_USERNAME"] ?? "postgres",
                Password = Configuration["BIO_POSTGRES_PASSWORD"] ?? "",
                Database = Configuration["BIO_POSTGRES_DATABASE"] ?? "postgres",
                Pooling = false
            };
            return connBuilder.ConnectionString;
        }

        public readonly IConfiguration Configuration;

        protected virtual void InitDbContext(BioContext dbContext)
        {
        }

        protected virtual void ModifyDbOptions(DbContextOptions<BioContext> options, bool inMemory)
        {
        }
    }
}