namespace SampleSinglePageApplication;
public partial class DataAccess : IDisposable, IDataAccess
{
    private int _accountLockoutMaxAttempts = 5;
    private int _accountLockoutMinutes = 10;
    private string _connectionString;
    private EFDataModel data;
    private bool _databaseExists;
    //private Encryption encryption = new Encryption(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x30, 0x31, 0x32 });
    private Guid _guid1 = new Guid("00000000-0000-0000-0000-000000000001");
    private Guid _guid2 = new Guid("00000000-0000-0000-0000-000000000002");
    private Microsoft.AspNetCore.Http.HttpContext? _httpContext;
    private bool _inMemoryDatabase = false;
    private string _ldapOptionalLocationAttribute = String.Empty;
    private bool _localDb = false;
    private bool _open;
    private string _version = "1.0.0";
    private string _released = "May 4, 2022";


    public DataAccess(string ConnectionString = "")
    {
        _connectionString = ConnectionString;
        if (!String.IsNullOrWhiteSpace(_connectionString)) {
            _connectionString = ConnectionString;
            GlobalSettings.DatabaseConnection = _connectionString;
        } else {
            _connectionString = GlobalSettings.DatabaseConnection;
        }

        if (!String.IsNullOrEmpty(_connectionString)) {
            var optionsBuilder = new DbContextOptionsBuilder<EFDataModel>();

            if (_connectionString.ToLower() == "inmemory") {
                optionsBuilder.UseInMemoryDatabase(_connectionString);
                _inMemoryDatabase = true;
            } else {
                optionsBuilder.UseSqlServer(_connectionString, options => options.EnableRetryOnFailure());
            }

            data = new EFDataModel(optionsBuilder.Options);

            //if(_connectionString.ToLower() == "inmemory") {
            //    var created = data.Database.EnsureCreated();
            //}

            if (!GlobalSettings.StartupRun) {
                if (data.Database.CanConnect()) {
                    _databaseExists = true;
                    _open = true;

                    // See if any migrations need to be applied.
                    if (!_inMemoryDatabase) {
                        DatabaseApplyLatestMigrations();
                    }
                } else {
                    // Try and create the database.
                    var created = CreateDatabase();
                    if (created.Result) {
                        _databaseExists = true;
                        // Try and apply the latest migrations
                        DatabaseApplyLatestMigrations();
                        _databaseExists = true;
                        _open = true;
                    }
                }

                // Make sure the default data exists and is up todate.
                SeedTestData();

                GlobalSettings.StartupRun = true;
                GlobalSettings.RunningSince = NowFromUnixEpoch();
            } else {
                _databaseExists = true;
                _open = true;
            }
        }
    }
}