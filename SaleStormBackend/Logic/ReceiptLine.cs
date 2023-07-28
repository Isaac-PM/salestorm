using System.Text;

namespace Inventory.Logic
{
    public class ReceiptLine
    {
        public long ReceiptId { get; set; }
        public int Quantity { get; set; }
        public Item? Item { get; set; }
        public string SaleStatus { get; set; }
        public float LineTotal { get; set; }

        public ReceiptLine(
            long ReceiptId = -1,
            int Quantity = 0,
            Item? Item = null
            )
        {
            this.ReceiptId = ReceiptId;
            this.Quantity = Quantity;
            this.Item = Item;
            this.SaleStatus = "open";
            ComputeLineTotal();
        }

        public void ComputeLineTotal()
        {
            LineTotal = Quantity * (Item?.SalePrice ?? 0);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ReceiptLine:\n");
            sb.Append($"ReceiptId: {ReceiptId}\n");
            sb.Append($"Quantity: {Quantity}\n");
            sb.Append($"Item: {(Item?.Id.ToString() ?? "No item set")}\n");
            sb.Append($"SaleStatus: {SaleStatus}\n");
            sb.Append($"LineTotal: {LineTotal}\n");
            return sb.ToString();
        }
    }
}
