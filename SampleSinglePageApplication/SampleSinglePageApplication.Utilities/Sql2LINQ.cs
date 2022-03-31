using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SampleSinglePageApplication;

/// <summary>
/// A collection of data tools for working with SQL database and returning standard C# objects.
/// </summary>
public class Sql2LINQ : IDisposable
{
    private bool _dbOpen = false;
    private string _connectionString;
    private string _connectionType;
    private SqlConnection _sqlConnection;

    /// <summary>
    /// creates a new instance of a data connection using a full connection string or a named connection string from a .Config file.
    /// </summary>
    /// <param name="ConnectionString"></param>
    public Sql2LINQ(string ConnectionString)
    {
        _connectionString = ConnectionString;

        if (String.IsNullOrWhiteSpace(_connectionString)) {
            throw new NullReferenceException("Invalid Connection String");
        }

        _connectionType = "sql";
        _sqlConnection = new SqlConnection(_connectionString);
        if (!_dbOpen) {
            _sqlConnection.Open();
            _dbOpen = true;
        }
    }

    /// <summary>
    /// Closes any open database connection and disposes the object
    /// </summary>
    public void Dispose()
    {
        CloseConnection();
    }

    /// <summary>
    /// closes the connection to the database
    /// </summary>
    public void CloseConnection()
    {
        if (_dbOpen) {
            try {
                _sqlConnection.Close();
            } catch { }

            _sqlConnection = null;
            _connectionString = String.Empty;
            _dbOpen = false;
        }
    }

    /// <summary>
    /// Disposes of the class.
    /// </summary>
    ~Sql2LINQ()
    {
        CloseConnection();
    }

    private List<T> DataTableToList<T>(DataTable table)
    {
        List<T> output = null;

        if (table != null && table.Rows.Count > 0) {
            output = new List<T>();

            var properties = ObjectProperties(Activator.CreateInstance(typeof(T)));

            foreach (DataRow row in table.Rows) {
                object item = Activator.CreateInstance(typeof(T));

                foreach (var prop in properties) {
                    // See if there is an item in the row for this property item that works with an SQL type
                    if (table.Columns.Contains(prop.Key)) {
                        string value = row[prop.Key].ToString();
                        if (!String.IsNullOrWhiteSpace(value)) {
                            if (prop.Value == typeof(string).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, value);
                            } else if (prop.Value == typeof(DateTime).GetTypeInfo() || prop.Value == typeof(DateTime?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, DateTime.Parse(value));
                            } else if (prop.Value == typeof(bool).GetTypeInfo() || prop.Value == typeof(bool?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, bool.Parse(value));
                            } else if (prop.Value == typeof(Guid).GetTypeInfo() || prop.Value == typeof(Guid?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, new Guid(value));
                            } else if (prop.Value == typeof(int).GetTypeInfo() || prop.Value == typeof(int?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, int.Parse(value));
                            } else if (prop.Value == typeof(byte).GetTypeInfo() || prop.Value == typeof(byte?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, byte.Parse(value));
                            } else if (prop.Value == typeof(char).GetTypeInfo() || prop.Value == typeof(char?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, char.Parse(value));
                            } else if (prop.Value == typeof(decimal).GetTypeInfo() || prop.Value == typeof(decimal?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, decimal.Parse(value));
                            } else if (prop.Value == typeof(double).GetTypeInfo() || prop.Value == typeof(double?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, double.Parse(value));
                            } else if (prop.Value == typeof(float).GetTypeInfo() || prop.Value == typeof(float?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, float.Parse(value));
                            } else if (prop.Value == typeof(uint).GetTypeInfo() || prop.Value == typeof(uint?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, uint.Parse(value));
                            } else if (prop.Value == typeof(long).GetTypeInfo() || prop.Value == typeof(long?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, long.Parse(value));
                            } else if (prop.Value == typeof(ulong).GetTypeInfo() || prop.Value == typeof(ulong?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, ulong.Parse(value));
                            } else if (prop.Value == typeof(short).GetTypeInfo() || prop.Value == typeof(short?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, short.Parse(value));
                            } else if (prop.Value == typeof(ushort).GetTypeInfo() || prop.Value == typeof(ushort?).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, ushort.Parse(value));
                            } else if (prop.Value == typeof(byte[]).GetTypeInfo() || prop.Value == typeof(byte?[]).GetTypeInfo()) {
                                item.GetType().GetProperty(prop.Key).SetValue(item, ((byte[])row[prop.Key]));
                            }
                        }
                    }
                }
                output.Add((T)item);
            }
        }

        return output;
    }

    /// <summary>
    /// Executes an SQL statement that does not return any values.
    /// </summary>
    /// <param name="query">The query to exectute.</param>
    /// <param name="sqlParameters">Optional collection of SqlParameters.</param>
    /// <returns>A BooleanResponse object.</returns>
    public BooleanResponse ExecuteQuery(string query, SqlParameter[] sqlParameters = null)
    {
        BooleanResponse output = new BooleanResponse();
        try {
            SqlCommand command = new SqlCommand(query, _sqlConnection);
            if (sqlParameters != null) {
                foreach (SqlParameter par in sqlParameters) {
                    if (par.Value == null) { par.Value = DBNull.Value; }
                    command.Parameters.Add(par);
                }
            }
            command.ExecuteNonQuery();
            output.Response = true;
        } catch (Exception ex) {
            output.Message = ex.Message;
        }
        return output;
    }

    /// <summary>
    /// Returns the table schema as a DataTable for a given query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <returns>The table schema as a DataTable.</returns>
    public DataTable GetTableSchema(string query)
    {
        DataTable output = null;

        SqlCommand command = new SqlCommand(query, _sqlConnection);
        SqlDataReader reader = command.ExecuteReader();
        output = reader.GetSchemaTable();

        return output;
    }

    /// <summary>
    /// Returns the table schema as a DataTable for a given query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="sqlParameters">An array of SqlParameter items.</param>
    /// <returns>The table schema as a DataTable.</returns>
    public DataTable GetTableSchema(string query, SqlParameter[] sqlParameters)
    {
        SqlCommand command = new SqlCommand(query, _sqlConnection);
        foreach (SqlParameter par in sqlParameters) {
            if (par.Value == null) { par.Value = DBNull.Value; }
            command.Parameters.Add(par);
        }
        SqlDataReader reader = command.ExecuteReader();
        DataTable output = reader.GetSchemaTable();
        return output;
    }

    private static Dictionary<string, System.Type> ObjectProperties(object atype)
    {
        if (atype == null) return new Dictionary<string, System.Type>();
        Type t = atype.GetType();
        PropertyInfo[] props = t.GetProperties();
        Dictionary<string, System.Type> dict = new Dictionary<string, System.Type>();
        foreach (PropertyInfo prp in props) {
            dict.Add(prp.Name, prp.PropertyType);
        }
        return dict;
    }

    private static Dictionary<string, object> ObjectPropertiesAndValues(object atype)
    {
        if (atype == null) return new Dictionary<string, object>();
        Type t = atype.GetType();
        PropertyInfo[] props = t.GetProperties();
        Dictionary<string, object> dict = new Dictionary<string, object>();
        foreach (PropertyInfo prp in props) {
            object value = prp.GetValue(atype, new object[] { });
            dict.Add(prp.Name, value);
        }
        return dict;
    }

    /// <summary>
    /// Returns the results of a query as a DataTable.
    /// </summary>
    /// <param name="query">The query to run.</param>
    /// <returns>A nullable DataTable object with the results of the query.</returns>
    public DataTable RunQuery(string query)
    {
        DataTable output = null;

        SqlCommand command = new SqlCommand(query, _sqlConnection);
        SqlDataReader reader = command.ExecuteReader();
        try {
            DataTable dt = new DataTable();
            dt.Load(reader);
            output = dt;
        } catch { }

        return output;
    }

    /// <summary>
    /// Returns the results of a query as a DataTable.
    /// </summary>
    /// <param name="query">The query to run.</param>
    /// <param name="sqlParameters">Optional collection of SqlParameters</param>
    /// <returns>A nullable DataTable object with the results of the query.</returns>
    public DataTable RunQuery(string query, SqlParameter[] sqlParameters)
    {
        DataTable output = null;
        SqlCommand command = new SqlCommand(query, _sqlConnection);
        foreach (SqlParameter par in sqlParameters) {
            if (par.Value == null) { par.Value = DBNull.Value; }
            command.Parameters.Add(par);
        }
        SqlDataReader reader = command.ExecuteReader();
        try {
            DataTable dt = new DataTable();
            dt.Load(reader);
            output = dt;
        } catch { }
        return output;
    }

    /// <summary>
    /// Executes a query and returns a collection of objects of type T. The object properties need to match the name of the fields returned in the query.
    /// </summary>
    /// <typeparam name="T">An object that will received the results of the query. Property names need to match the field names and the property type should match the SQL column type.</typeparam>
    /// <param name="query">The query to run.</param>
    /// <returns>A nullable List of T objects specified.</returns>
    public List<T> RunQuery<T>(string query)
    {
        List<T> output = null;
        if (!String.IsNullOrWhiteSpace(query)) {
            var table = RunQuery(query);
            output = DataTableToList<T>(table);
        }
        return output;
    }

    /// <summary>
    /// Executes a query with SqlParameters and returns a collection of objects of type T. The object properties need to match the name of the fields returned in the query.
    /// </summary>
    /// <typeparam name="T">An object that will received the results of the query. Property names need to match the field names and the property type should match the SQL column type.</typeparam>
    /// <param name="query">The query to run.</param>
    /// <param name="sqlParameters">Collection of SqlParameters.</param>
    /// <returns>A nullable List of T objects specified.</returns>
    public List<T> RunQuery<T>(string query, SqlParameter[] sqlParameters)
    {
        List<T> output = null;
        if (!String.IsNullOrWhiteSpace(query)) {
            var table = RunQuery(query, sqlParameters);
            output = DataTableToList<T>(table);

        }
        return output;
    }

    /// <summary>
    /// Saves a record to a table by passing a C# object with properties that match the field names in the table.
    /// </summary>
    /// <typeparam name="T">The object type being passed to the function.</typeparam>
    /// <param name="Record">An object with property names that match the field names and the property types that match the SQL column types.</param>
    /// <param name="TableName">The name of the table to insert the record into.</param>
    /// <param name="IdColumn">The name of the colum that will be used to match the record. The value from the object T with the same property name will be searched for an existing value to try and find the record.</param>
    /// <param name="CreateIfMissing">Option to create a new record if no match is found. Assuming the IdColumn creates a value on insert that value will be updated in the object T returned for the property with the name specified in the IdColumn property.</param>
    /// <returns>A Tuple containing a BooleanResponse and the updated object.</returns>
    public Tuple<BooleanResponse, T> SaveRecord<T>(T Record, string TableName, string IdColumn, bool CreateIfMissing = false)
    {
        BooleanResponse response = new BooleanResponse();
        var rec = Record;

        // Find the value of the IdColumn property in the <T>record object that was passed.
        var properties = ObjectPropertiesAndValues(Record);
        object idValue = null;
        if (properties != null && properties.Count() > 0) {
            var prop = properties.FirstOrDefault(p => p.Key.ToLower() == IdColumn.ToLower());
            if (prop.Key.ToLower() == IdColumn.ToLower() && prop.Value != null) {
                idValue = prop.Value;
            }
        }

        DataTable schema = null;

        // First, see if the record exists.
        DataTable exists = null;
        string query = "SELECT * FROM [" + TableName + "] WHERE [" + IdColumn + "]=@val";

        exists = RunQuery(query, new SqlParameter[] { new SqlParameter("@val", idValue) });
        schema = GetTableSchema(query, new SqlParameter[] { new SqlParameter("@val", idValue) });

        // Check the table schema to see if the IdColumn field has the IsIdentity flag set to true
        bool idColumnIsIdentity = false;
        if (schema != null && schema.Rows != null && schema.Rows.Count > 0) {
            foreach (DataRow row in schema.Rows) {
                string col = String.Empty;
                try {
                    col += row["ColumnName"].ToString();
                } catch { }

                if (!String.IsNullOrWhiteSpace(col) && col.ToLower() == IdColumn.ToLower()) {
                    try {
                        idColumnIsIdentity = (bool)row["IsIdentity"];
                    } catch { }

                    if (!idColumnIsIdentity) {
                        try {
                            idColumnIsIdentity = (bool)row["IsAutoIncrement"];
                        } catch { }
                    }
                }
            }
        }

        List<SqlParameter> sqlParameters = new List<SqlParameter> { new SqlParameter("@val", idValue) };
        string executeQuery = String.Empty;

        bool newRecord = false;
        if (exists != null && exists.Rows != null && exists.Rows.Count > 0) {
            if (exists.Rows.Count == 1) {
                // Build an update query
                executeQuery = "UPDATE [" + TableName + "] SET ";
                bool firstItem = true;
                foreach (var prop in properties) {
                    if (prop.Key.ToLower() != IdColumn.ToLower() || !idColumnIsIdentity) {
                        if (!firstItem) {
                            executeQuery += ", ";
                        }
                        executeQuery += "[" + prop.Key + "]=@" + prop.Key;

                        sqlParameters.Add(new SqlParameter("@" + prop.Key, prop.Value));
                        firstItem = false;
                    }
                }

                executeQuery += " WHERE [" + IdColumn + "]=@val;";
            } else {
                response.Message = "Unique Record Not Found, " + exists.Rows.Count + " Rows Found";
            }
        } else if (CreateIfMissing) {
            // Build an insert query
            executeQuery = "INSERT INTO [" + TableName + "] (";
            bool firstItem = true;
            foreach (var prop in properties) {
                if (prop.Key.ToLower() != IdColumn.ToLower() || !idColumnIsIdentity) {
                    if (!firstItem) {
                        executeQuery += ", ";
                    }
                    executeQuery += "[" + prop.Key + "]";
                    firstItem = false;

                    sqlParameters.Add(new SqlParameter("@" + prop.Key, prop.Value));
                }
            }
            executeQuery += " ) OUTPUT inserted.[" + IdColumn + "] VALUES (";
            firstItem = true;
            foreach (var prop in properties) {
                if (prop.Key.ToLower() != IdColumn.ToLower() || !idColumnIsIdentity) {
                    if (!firstItem) {
                        executeQuery += ", ";
                    }
                    executeQuery += "@" + prop.Key;
                    firstItem = false;
                }
            }
            executeQuery += ");";
            newRecord = true;
        } else {
            response.Message = "Record Not Found";
        }

        if (!String.IsNullOrWhiteSpace(executeQuery)) {
            object executeResponse = null;

            try {
                SqlCommand command = new SqlCommand(executeQuery, _sqlConnection);
                foreach (SqlParameter par in sqlParameters) {
                    if (par.Value == null) { par.Value = DBNull.Value; }
                    command.Parameters.Add(par);
                }
                executeResponse = command.ExecuteScalar();
                response.Response = true;
                response.Message = newRecord ? "Record Added" : "Record Updated";
            } catch (Exception ex) {
                response.Message = ex.Message;
            }

            if (newRecord && executeResponse != null && idColumnIsIdentity) {
                // Set the value of the returned record's idColum property to the value returned
                rec.GetType().GetProperty(IdColumn).SetValue(rec, executeResponse);
            }
        }

        return new Tuple<BooleanResponse, T>(response, rec);
    }

    /// <summary>
    /// Tests a query to see if the query is a valid SQL statement.
    /// </summary>
    /// <param name="query">The query to test.</param>
    /// <param name="sqlParameters">Optional collection of SqlParameters.</param>
    /// <returns>A BooleanResponse object.</returns>
    public BooleanResponse ValidateQuery(string query, SqlParameter[] sqlParameters = null)
    {
        BooleanResponse output = new BooleanResponse();
        try {
            SqlCommand command = new SqlCommand(query, _sqlConnection);
            if (sqlParameters != null) {
                foreach (SqlParameter par in sqlParameters) {
                    if (par.Value == null) { par.Value = DBNull.Value; }
                    command.Parameters.Add(par);
                }
            }
            SqlDataReader reader = command.ExecuteReader();
            output.Response = true;
        } catch (Exception ex) {
            output.Message = ex.Message;
        }
        return output;
    }

    /// <summary>
    /// Object that returns a response for functions that don't return data. The Response is either true or false and the Message will contain any error returned.
    /// </summary>
    public class BooleanResponse
    {
        /// <summary>
        /// Indicates if the result was true or false.
        /// </summary>
        public bool Response { get; set; }
        /// <summary>
        /// Any message returned.
        /// </summary>
        public string Message { get; set; }
    }

}