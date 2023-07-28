using Inventory.Logic;
using System.Data.SqlClient;

namespace Inventory.Data
{
    public class ReceiptDao
    {
        private RelDatabase _database = RelDatabase.Instance;
        private List<Item> _items;
        private object _locker = new object();

        public ReceiptDao(List<Item> _items)
        {
            this._items = _items;
        }

        public void SetItems(List<Item> items)
        {
            _items = items;
        }

        public Receipt ParseReceipt(SqlDataReader reader)
        {
            lock (_locker)
            {
                Receipt receipt = new Receipt();
                receipt.Id = Convert.ToInt64(reader["id"]);
                receipt.WorkerId = Convert.ToInt32(reader["worker_id"]);
                receipt.GrandTotal = Convert.ToSingle(reader["grand_total"]);
                receipt.Lines = new ReceiptLineDao(_items).GetLinesFromReceipt(receipt.Id);
                return receipt;
            }
        }

        public List<Receipt> GetReceipts()
        {
            return GetReceiptsFromWorker(-1);
        }

        public List<Receipt> GetReceiptsFromWorker(int workerId)
        {
            List<Receipt> receipts = new List<Receipt>();
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return receipts;
            }
            try
            {
                string query = "SELECT * FROM Receipt";
                if (workerId != -1) query = "SELECT * FROM Receipt WHERE Receipt.worker_id = @workerId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (workerId != -1) command.Parameters.AddWithValue("@workerId", workerId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            receipts.Add(ParseReceipt(reader));
                        }
                        _database.CloseConnection(connection);
                        return receipts;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}\n");
            }
            return receipts;
        }

        public bool InsertReceipt(Receipt receipt)
        {
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return false;
            }
            try
            {
                string query = "INSERT INTO Receipt (id, worker_id) VALUES (@id, @workerId)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", receipt.Id);
                    command.Parameters.AddWithValue("@workerId", receipt.WorkerId);
                    command.ExecuteNonQuery();
                    _database.CloseConnection(connection);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}\n");
            }
            return false;
        }
    }
}
