namespace SampleSinglePageApplication;

public partial class DataAccess
{
    private DataObjects.BooleanResponse CreateDatabase()
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();

        var csConfig = GetConnectionStringConfig();
        string databaseName = csConfig.Database;
        bool alreadyExists = false;

        string cs = Utilities.ReplaceInString(csConfig.ConnectionString, "data source=" + csConfig.Server, "");
        cs = Utilities.ReplaceInString(cs, "initial catalog=" + csConfig.Database, "");

        using (Sql2LINQ data = new Sql2LINQ(cs)) {
            System.Data.DataTable dt = data.RunQuery("SELECT DBI.name, DBI.crdate AS CreateDate, DBI.filename " +
                "FROM master.dbo.sysdatabases DBI WHERE DBI.Name NOT IN ('master','model','msdb','tempdb') ORDER BY Name");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (System.Data.DataRow r in dt.Rows) {
                    string strThisDatabase = r["Name"] + String.Empty;
                    if (strThisDatabase.ToLower() == databaseName.ToLower()) { alreadyExists = true; }
                }
            }

            if (alreadyExists) {
                output.Messages.Add("The database '" + databaseName + "' already exists on the server.");
                output.Result = true;
            } else {
                // OK, attempt to create this database
                System.Data.SqlClient.SqlParameter[] qPar = new System.Data.SqlClient.SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter("@DBName", databaseName)
                };

                try {
                    var response = data.ExecuteQuery("CREATE DATABASE [" + databaseName + "]; ", qPar);
                    if (!response.Response) {
                        output.Messages.Add("Error attempting to create the new database '" + databaseName + "' on the SQL Server.");
                        return output;
                    }
                    output.Result = true;
                } catch (Exception ex) {
                    output.Messages.Add("Error attempting to create the new database '" + databaseName + "' on the SQL Server - " + ex.Message);
                }
            }
        }

        return output;
    }

    private DataObjects.BooleanResponse DatabaseApplyLatestMigrations()
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        output.Messages = new List<string>();

        bool upToDate = false;

        // First, see if this is a completely empty database. If so, run all the database packages.
        // If not, find out what the last package was that ran and run all newer packages.
        List<string> tables = DatabaseGetTables();
        DataMigrations m = new DataMigrations();
        List<DataObjects.DataMigration> migrations = m.GetMigrations();
        int iHighestInstalledMigration = 0;

        if (tables.Count > 0) {
            upToDate = DatabaseUpToDate;
        }

        if (!upToDate) {
            foreach (var mig in migrations) {
                int MigrationId = mig.MigrationId;
                if (MigrationId > iHighestInstalledMigration) {
                    DateTime MigrationDate = mig.MigrationDate;
                    List<String> Migration = mig.Migration != null ? mig.Migration : new List<string>();
                    if (Migration.Count > 0) {
                        // run each migration in this set
                        foreach (string migrationPart in Migration) {
                            var migrationApplied = DatabaseApplyMigration(MigrationId, migrationPart);
                            if (!migrationApplied.Result) {
                                output.Messages.Add("Failed to Execute SQL Statement: " + migrationPart);
                                return output;
                            }
                        }
                    }
                    // Pause for a few seconds to give the SQL Server time to save changes
                    System.Threading.Thread.Sleep(2000);

                    // After these migrations were applied update the DataMigrations table entry
                    DatabaseUpdateMigrationRecord(mig);

                    // Pause for a few seconds to give the SQL Server time to save changes
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }


        // Pause for a few seconds to give the SQL Server time to save changes
        System.Threading.Thread.Sleep(2000);

        output.Result = true;

        return output;
    }

    private DataObjects.BooleanResponse DatabaseApplyMigration(int MigrationId, string SQL)
    {
        DataObjects.BooleanResponse output = new DataObjects.BooleanResponse();
        output.Messages = new List<string>();

        using (Sql2LINQ da = new Sql2LINQ(_connectionString)) {
            System.Data.SqlClient.SqlParameter[] qpar = new System.Data.SqlClient.SqlParameter[] { new System.Data.SqlClient.SqlParameter("@par", "0") };
            var resp = da.ExecuteQuery(SQL, qpar);

            if (!resp.Response) {
                string message = String.Empty;
                if (resp.Message != null) {
                    message += resp.Message;
                }

                if (message.ToLower().Contains("there is already an object named")) {
                    // Skip these messages
                } else {
                    output.Messages.Add("Error processing migration " + MigrationId.ToString() + ": " + message);
                }
            }
        }

        output.Result = output.Messages.Count() == 0;

        return output;
    }

    public bool DatabaseExists {
        get {
            return _databaseExists;
        }
    }

    private List<string> DatabaseGetTables()
    {
        List<string> output = new List<string>();

        using (Sql2LINQ da = new Sql2LINQ(_connectionString)) {
            System.Data.DataTable dt = da.RunQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (System.Data.DataRow r in dt.Rows) {
                    output.Add(r["TABLE_NAME"] + String.Empty);
                }
            }
        }

        return output;
    }

    public bool DatabaseOpen {
        get {
            return _open;
        }
    }

    private void DatabaseUpdateMigrationRecord(DataObjects.DataMigration m)
    {
        string migration = String.Empty;

        if (m != null) {
            if (m.Migration != null && m.Migration.Count() > 0) {
                foreach (string mig in m.Migration) {
                    if (!String.IsNullOrWhiteSpace(migration)) { migration += Environment.NewLine; }
                    migration += mig;
                }
            }

            var rec = data.DataMigrations.FirstOrDefault(x => x.MigrationId == m.MigrationId);

            if (rec == null) {
                rec = new DataMigration {
                    MigrationId = m.MigrationId,
                    MigrationDate = m.MigrationDate,
                    MigrationApplied = DateTime.Now,
                    Migration = migration
                };
                data.DataMigrations.Add(rec);
            } else {
                rec.MigrationDate = m.MigrationDate;
                rec.MigrationApplied = DateTime.Now;
                rec.Migration = migration;
            }
            try {
                data.SaveChanges();
            } catch { }
        }
    }

    public bool DatabaseUpToDate {
        get {
            bool output = false;

            if (_open) {
                int highestMigrationLevel = HighestMigrationLevel;
                int highestMigrationApplied = 0;

                try {
                    var recs = data.DataMigrations.OrderBy(x => x.MigrationId);
                    if (recs != null && recs.Any()) {
                        foreach (var rec in recs) {
                            if (rec.MigrationId > highestMigrationApplied) {
                                highestMigrationApplied = rec.MigrationId;
                            }
                        }
                    }
                } catch { }

                if (highestMigrationApplied == highestMigrationLevel) {
                    output = true;
                }
            }

            return output;

        }
    }

    public DataObjects.ConnectionStringConfig GetConnectionStringConfig()
    {
        var output = new DataObjects.ConnectionStringConfig();
        output.ActionResponse = GetNewActionResponse();

        string connectionString = String.Empty;
        try {
            var appSettingsPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");
            var json = System.IO.File.ReadAllText(appSettingsPath);

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new ExpandoObjectConverter());
            jsonSettings.Converters.Add(new StringEnumConverter());

            dynamic? config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);
            if (config != null) {
                connectionString += config.ConnectionStrings.AppData;
            }
        } catch { }

        if (!String.IsNullOrEmpty(connectionString)) {
            output.ActionResponse.Result = true;
            output.ConnectionString = connectionString;
            List<string> parts = connectionString.Split(';').ToList();
            if (parts != null && parts.Any()) {
                foreach (var part in parts) {
                    string element = String.Empty;
                    string value = String.Empty;

                    var items = part.Split('=');
                    if (items.Length > 0) {
                        element += items[0];
                        if (items.Length > 1) {
                            value += items[1];
                        }
                    }

                    if (!String.IsNullOrEmpty(element)) {
                        switch (element.ToUpper()) {
                            case "DATA SOURCE":
                                output.Server = value;
                                break;

                            case "INITIAL CATALOG":
                                output.Database = value;
                                break;

                            case "USER ID":
                                output.UserId = value;
                                break;

                            case "PASSWORD":
                                output.Password = value;
                                break;
                        }
                    }
                }
            }
        }

        return output;
    }

    public int HighestMigrationLevel {
        get {
            int output = 0;

            var dataMigrations = new DataMigrations();
            var migrations = dataMigrations.GetMigrations();
            if (migrations != null && migrations.Any()) {
                foreach (var migration in migrations) {
                    if (migration.MigrationId > output) {
                        output = migration.MigrationId;
                    }
                }
            }

            return output;
        }
    }


}