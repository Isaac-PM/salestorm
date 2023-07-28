using Inventory.Logic;
using System.Data.SqlClient;

namespace Inventory.Data
{
    public class ItemDao
    {
        private RelDatabase _database = RelDatabase.Instance;

        public ItemDao()
        {
        }

        public Item ParseItem(SqlDataReader reader)
        {
            Item item = new Item();
            item.Id = Convert.ToInt32(reader["id"]);
            item.ItemDescription = reader["item_description"]?.ToString() ?? "";
            item.CurrentQuantity = Convert.ToInt32(reader["current_quantity"]);
            item.MinimumQuantity = Convert.ToInt32(reader["minimum_quantity"]);
            item.ReorderQuantity = Convert.ToInt32(reader["reorder_quantity"]);
            item.PurchasePrice = Convert.ToSingle(reader["purchase_price"]);
            item.ProfitMargin = Convert.ToSingle(reader["profit_margin"]);
            item.SalePrice = Convert.ToSingle(reader["sale_price"]);
            return item;
        }

        public List<Item> GetItems()
        {
            List<Item> items = new List<Item>();
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return items;
            }
            try
            {
                string query = "SELECT * FROM Item";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(ParseItem(reader));
                        }
                        _database.CloseConnection(connection);
                        return items;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}\n");
            }
            return items;
        }

        public Item? GetItem(int itemId)
        {
            SqlConnection? connection = _database.OpenConnection();
            if (connection == null)
            {
                return null;
            }
            try
            {
                string query = "SELECT * FROM Item WHERE Item.id = @itemId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@itemId", itemId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Item? item = null;
                        if (reader.Read())
                        {
                            item = ParseItem(reader);
                        }
                        _database.CloseConnection(connection);
                        return item;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error executing query: {e.Message}\n");
            }
            return null;
        }
    }
}
