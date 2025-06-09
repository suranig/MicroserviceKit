using Testcontainers.PostgreSql;
using Testcontainers.MsSql;

namespace ECommerce.OrderService.Integration.Tests.Fixtures;

public class TestContainersSetup : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private MsSqlContainer? _sqlServerContainer;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Using in-memory database for tests
ConnectionString = "Data Source=:memory:";
    }

    public async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }

        if (_sqlServerContainer != null)
        {
            await _sqlServerContainer.DisposeAsync();
        }
    }

    private async Task SetupPostgreSqlContainer()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();
        ConnectionString = _postgresContainer.GetConnectionString();
    }

    private async Task SetupSqlServerContainer()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithPassword("TestPass123!")
            .WithCleanUp(true)
            .Build();

        await _sqlServerContainer.StartAsync();
        ConnectionString = _sqlServerContainer.GetConnectionString();
    }
}