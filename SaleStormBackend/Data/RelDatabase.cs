using Inventory.Logic;
using System.Data.SqlClient;

namespace Inventory.Data
{
    public class RelDatabase
    // TODO: Change explicit queries to stored procedures
    {
        /*
        Gather connection information from the server :
        1. Open SQL Server Management Studio
        2. Connect to the server
        3. Right click on the server name
        4. Select "Properties"
        5. Select the page "View Connection Properties"
        6. Exit the server properties dialog
        7. Right click on the database name
        8. Select "Properties"
        9. Select the page "View Connection Properties"

        If an error message pops up, saying "Server is no longer available",
        try making any SELECT in the database, and then try again; or:
        https://learn.microsoft.com/en-us/answers/questions/803631/unable-to-connect-to-web-server-iis-express-its-ba
        */
        private readonly string _connectionString = "";
        private readonly string _serverName = "ISAACPM"; // TODO : Change this to your server name
        private readonly string _databaseName = "INVENTORY";
        private readonly string _userName = "";
        private readonly string _password = "";

        public static RelDatabase Instance { get; } = new RelDatabase();

        private RelDatabase()
        {
            _connectionString += $"Data Source={_serverName};";
            _connectionString += $"Initial Catalog={_databaseName};";
            if (_userName != "")
            {
                _connectionString += $"User ID={_userName};";
                _connectionString += $"Password={_password};";
            }
            else
            {
                _connectionString += $"Integrated Security=True;"; // Using Windows Authentication
            }
        }

        public SqlConnection? OpenConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error connecting to the database: {e.Message}\n");
                return null;
            }
        }

        public void CloseConnection(SqlConnection connection)
        {
            try
            {
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing connection: {e.Message}\n");
            }
        }
    }
}
