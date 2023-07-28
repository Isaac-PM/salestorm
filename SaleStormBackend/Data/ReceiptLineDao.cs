using Inventory.Logic;
using System.Data.SqlClient;

namespace Inventory.Data
{
    public class ReceiptLineDao
    {
        private RelDatabase _database = RelDatabase.Instance;
        private List<Item> _items;
        private object _locker = new object();

        public ReceiptLineDao(List<Item> _items)
        {
            this._items = _items;
        }

        public void SetItems(List<Item> items)
        {
            _items = items;
        }

        public ReceiptLine ParseReceiptLine(SqlDataReader reader)
        {
            lock (_locker)
            {
                ReceiptLine receiptLine = new ReceiptLine();
                receiptLine.ReceiptId = Convert.ToInt64(reader["receipt_id"]);
                int itemId = Convert.ToInt32(reader["item_id"]);
                receiptLine.Item = _items.Find(item => item.Id == itemId);
                receiptLine.Quantity = Convert.ToInt32(reader["quantity"]);
                receiptLine.SaleStatus = reader["sale_status"]?.ToString() ?? "open";
                receiptLine.LineTotal = Convert.ToSingle(reader["line_total"]);
                return receiptLine;
            }
        }

        public List<ReceiptLine> GetLinesFromReceipt(long receiptId)
        {
            List<ReceiptLine> lines = new List<ReceiptLine>();
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return lines;
            }
            try
            {
                string query = "SELECT * FROM ReceiptLine WHERE ReceiptLine.receipt_id = @receiptId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@receiptId", receiptId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lines.Add(ParseReceiptLine(reader));
                        }
                        _database.CloseConnection(connection);
                        return lines;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}\n");
            }
            return lines;
        }

        public bool InsertReceiptLine(ReceiptLine line)
        {
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return false;
            }
            try
            {
                string query = "INSERT INTO ReceiptLine (receipt_id, item_id, quantity) VALUES (@receiptId, @itemId, @quantity)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@receiptId", line.ReceiptId);
                    command.Parameters.AddWithValue("@itemId", line.Item.Id);
                    command.Parameters.AddWithValue("@quantity", line.Quantity);
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

        public bool ApplyReceiptLines(int workerId)
        {
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return false;
            }
            try
            {
                string query = "EXEC INVENTORY.dbo.ApplyReceipts @worker_id = @workerId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@workerId", workerId);
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