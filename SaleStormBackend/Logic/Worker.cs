using System.Text;

namespace Inventory.Logic
{
    public class Worker
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string UserType { get; set; }
        public List<Receipt>? Receipts { get; set; }
        public List<Item>? Items { get; set; }

        public Worker(
            int Id = -1,
            string UserName = "",
            string UserPassword = "",
            string UserType = "",
            List<Receipt>? Receipts = null,
            List<Item>? Items = null
            )
        {
            this.Id = Id;
            this.UserName = UserName;
            this.UserPassword = UserPassword;
            this.UserType = UserType;
            if (Receipts != null)
            {
                this.Receipts = Receipts;
            }
            else
            {
                this.Receipts = new List<Receipt>();
            }
            if (Items != null)
            {
                this.Items = Items;
            }
            else
            {
                this.Items = new List<Item>();
            }
        }

        public void AddReceipt(Receipt receipt, string owner)
        {
            if (Receipts != null && receipt != null)
            {
                if (owner == "me") receipt.WorkerId = Id;
                Receipts.Add(receipt);
            }
        }

        public void AddItem(Item item)
        {
            if (Items != null && item != null)
            {
                Items.Add(item);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Worker:\n");
            sb.Append($"Id: {Id}\n");
            sb.Append($"UserName: {UserName}\n");
            sb.Append($"UserPassword: {UserPassword}\n");
            sb.Append($"UserType: {UserType}\n");
            sb.Append($"Receipts: {Receipts?.Count.ToString() ?? "0"}\n");
            sb.Append($"Items: {Items?.Count.ToString() ?? "0"}\n");
            return sb.ToString();
        }
    }
}
