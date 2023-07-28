using Inventory.Logic;
using System.Data.SqlClient;

namespace Inventory.Data
{
    public class WorkerDao
    {
        private RelDatabase _database = RelDatabase.Instance;
        private List<Item> _items;
        private object _locker = new object();
        // private List<Receipt> _receipts;

        public WorkerDao(List<Item> _items)
        {
            this._items = _items;
        }

        public void SetItems(List<Item> items)
        {
            _items = items;
        }

        public Worker ParseWorker(SqlDataReader reader)
        {
            lock (_locker)
            {
                Worker worker = new Worker();
                worker.Id = Convert.ToInt32(reader["id"]);
                worker.UserName = reader["user_name"]?.ToString() ?? "";
                worker.UserPassword = reader["user_password"]?.ToString() ?? "";
                worker.UserType = reader["user_type"]?.ToString() ?? "seller";
                worker.Receipts = new ReceiptDao(_items).GetReceiptsFromWorker(worker.Id);
                // worker.Items = _items;
                return worker;
            }
        }

        public Worker? GetWorker(string userName, string userPassword)
        { // I know this is atrocious, but this project is a PoC, not a production-ready application.
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return null;
            }
            try
            {
                string query = "SELECT * FROM Worker WHERE user_name = @userName AND user_password = @userPassword";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userName", userName);
                    command.Parameters.AddWithValue("@userPassword", userPassword);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Worker? worker = null;
                        if (reader.Read())
                        {
                            worker = ParseWorker(reader);
                        }
                        _database.CloseConnection(connection);
                        return worker;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}\n");
            }
            return null;
        }

        public List<Worker> GetWorkers()
        {
            List<Worker> workers = new List<Worker>();
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return workers;
            }
            try
            {
                string query = "SELECT * FROM Worker";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            workers.Add(ParseWorker(reader));
                        }
                        _database.CloseConnection(connection);
                        return workers;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}\n");
            }
            return workers;
        }
    }
}
