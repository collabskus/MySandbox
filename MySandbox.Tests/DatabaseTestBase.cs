using Microsoft.EntityFrameworkCore;
using MySandbox.ClassLibrary;

namespace MySandbox.Tests;

public class DatabaseTestBase : IDisposable
{
    protected StuffDbContext DbContext { get; }
    private readonly string _databaseName;

    public DatabaseTestBase()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<StuffDbContext>()
            .UseSqlite($"Data Source={_databaseName};Mode=Memory;Cache=Shared")
            .Options;

        DbContext = new StuffDbContext(options);
        DbContext.Database.OpenConnection();
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.CloseConnection();
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
