using System.Text;

namespace Inventory.Logic
{
    public class Item
    {
        public int Id { get; set; }
        public string ItemDescription { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumQuantity { get; set; }
        public int ReorderQuantity { get; set; }
        public float PurchasePrice { get; set; }
        public float ProfitMargin { get; set; }
        public float SalePrice { get; set; }

        public Item(
            int Id = -1,
            string ItemDescription = "",
            int CurrentQuantity = 0,
            int MinimumQuantity = 0,
            int ReorderQuantity = 0,
            float PurchasePrice = 0,
            float ProfitMargin = 0,
            float SalePrice = 0
            )
        {
            this.Id = Id;
            this.ItemDescription = ItemDescription;
            this.CurrentQuantity = CurrentQuantity;
            this.MinimumQuantity = MinimumQuantity;
            this.ReorderQuantity = ReorderQuantity;
            this.PurchasePrice = PurchasePrice;
            this.ProfitMargin = ProfitMargin;
            if (SalePrice == 0)
            {
                this.SalePrice = this.PurchasePrice * (1 + this.ProfitMargin);
            }
            else
            {
                this.SalePrice = SalePrice;
            }
        }

        public bool ReduceQuantity(int quantity)
        {
            if (CurrentQuantity - quantity < 0)
            {
                return false;
            }
            else
            {
                CurrentQuantity -= quantity;
                return true;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Item:\n");
            sb.Append($"Id: {Id}\n");
            sb.Append($"ItemDescription: {ItemDescription}\n");
            sb.Append($"CurrentQuantity: {CurrentQuantity}\n");
            sb.Append($"MinimumQuantity: {MinimumQuantity}\n");
            sb.Append($"ReorderQuantity: {ReorderQuantity}\n");
            sb.Append($"PurchasePrice: {PurchasePrice}\n");
            sb.Append($"ProfitMargin: {ProfitMargin}\n");
            sb.Append($"SalePrice: {SalePrice}\n");
            return sb.ToString();
        }
    }
}