using HelloWorld.EFModels;
using Microsoft.EntityFrameworkCore;

namespace HelloWorld;

public partial class DataAccess
{
    private string _connectionString;
    private HelloWorldContext data;

    public DataAccess(string ConnectionString = "")
    {
        _connectionString = ConnectionString;
        if (!String.IsNullOrWhiteSpace(_connectionString))
        {
            _connectionString = ConnectionString;
            GlobalSettings.DatabaseConnection = _connectionString;
        }
        else
        {
            _connectionString = GlobalSettings.DatabaseConnection;
        }
        if (!String.IsNullOrEmpty(_connectionString))
        {
            var optionsBuilder = new DbContextOptionsBuilder<HelloWorldContext>();
            //optionsBuilder.UseInMemoryDatabase("InMemory");
            optionsBuilder.UseSqlServer(_connectionString, options => options.EnableRetryOnFailure()); data = new HelloWorldContext(optionsBuilder.Options);
            data = new HelloWorldContext(optionsBuilder.Options);
        }
        else
        {
            throw new Exception("No sql connection string found");
        }
    }

    public void EnsureCreated()
    {
        data.Database.EnsureCreated();
    }

    public void EnsureDeleted()
    {
        data.Database.EnsureDeleted();
    }
}


