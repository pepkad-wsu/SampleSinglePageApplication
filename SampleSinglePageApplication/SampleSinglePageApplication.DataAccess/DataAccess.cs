namespace SampleSinglePageApplication;
public partial class DataAccess : IDisposable, IDataAccess
{
    private int _accountLockoutMaxAttempts = 5;
    private int _accountLockoutMinutes = 10;
    private string _appName = "SampleSinglePageApplication";
    private string _connectionString;
    private string _copyright = "Company Name";
    private EFDataModel data;
    private string _databaseType;
    private Guid _guid1 = new Guid("00000000-0000-0000-0000-000000000001");
    private Guid _guid2 = new Guid("00000000-0000-0000-0000-000000000002");
    private Microsoft.AspNetCore.Http.HttpContext? _httpContext;
    private bool _inMemoryDatabase = false;
    private string _ldapOptionalLocationAttribute = String.Empty;
    private bool _localDb = false;
    private bool _open;
    private string _version = "1.0.0";
    private DateTime _released = Convert.ToDateTime("2/27/2023 12:37 PM");

    public DataAccess(string ConnectionString = "", string DatabaseType = "")
    {
        _connectionString = ConnectionString;
        _databaseType = DatabaseType;

        if (!String.IsNullOrWhiteSpace(_connectionString)) {
            _connectionString = ConnectionString;
        }

        var optionsBuilder = new DbContextOptionsBuilder<EFDataModel>();

        // Both the Connection String and Database Type parameters are required.
        // Otherwise the app will redirect to the page to configure the database connection.
        if (!String.IsNullOrEmpty(_connectionString) && !String.IsNullOrWhiteSpace(_databaseType)) {
            switch (_databaseType.ToLower()) {
                case "inmemory":
                    optionsBuilder.UseInMemoryDatabase("InMemory");
                    _inMemoryDatabase = true;
                    break;

                case "mysql":
                    optionsBuilder.UseMySQL(_connectionString, options => options.EnableRetryOnFailure());
                    break;

                case "postgresql":
                    optionsBuilder.UseNpgsql(_connectionString, options => options.EnableRetryOnFailure());
                    break;

                case "sqlite":
                    optionsBuilder.UseSqlite(_connectionString);
                    break;

                case "sqlserver":
                    optionsBuilder.UseSqlServer(_connectionString, options => options.EnableRetryOnFailure());
                    break;
            }

            data = new EFDataModel(optionsBuilder.Options);

            if (_inMemoryDatabase) {
                // For the In-Memory database this creates the schema in memory.
                data.Database.EnsureCreated();
            }

            if (!GlobalSettings.StartupRun) {
                if (data.Database.CanConnect()) {
                    _open = true;

                    // See if any migrations need to be applied.
                    if (!_inMemoryDatabase) {
                        DatabaseApplyLatestMigrations();
                    }
                } else {
                    // Try and create the database using the built-in EF command
                    data.Database.EnsureCreated();

                    // See if any migrations need to be applied.
                    if (data.Database.CanConnect()) {
                        DatabaseApplyLatestMigrations();
                        _open = true;
                    } else {
                        throw new Exception("Unable to connect to the database. Please check your connection string.");
                    }
                }

                // Make sure the default data exists and is up to date.
                SeedTestData();

                GlobalSettings.StartupRun = true;
                GlobalSettings.RunningSince = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            } else {
                _open = true;
            }
        } else {
            // To prevent errors just use an InMemory copy
            optionsBuilder.UseInMemoryDatabase("InMemory");
            data = new EFDataModel(optionsBuilder.Options);

            if (!GlobalSettings.StartupRun) {
                GlobalSettings.StartupError = true;
                GlobalSettings.StartupErrorCode = "MissingConnectionString";
            } else {
                throw new NullReferenceException("Missing Connection String and/or Database Type");
            }
        }
    }
}