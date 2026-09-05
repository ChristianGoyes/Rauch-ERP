using Testcontainers.PostgreSql;

namespace RauchERP.IntegrationTests;
public sealed class PostgresPersistenceTests
{
    [Fact(Skip = "Requires Docker. Run after installing Docker Desktop.")]
    public async Task PostgreSql_container_is_available_for_module_migrations()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17-alpine").Build(); await postgres.StartAsync(); Assert.NotEmpty(postgres.GetConnectionString());
    }
}
