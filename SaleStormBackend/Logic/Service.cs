using Inventory.Data;

namespace Inventory.Logic
{
    public class Service
    {
        public static Service Instance { get; } = new Service();

        public List<Item> Items = new List<Item>();
        public ItemDao ItemDaoInstance;
        public ReceiptLineDao ReceiptLineDaoInstance;
        public ReceiptDao ReceiptDaoInstance;
        public WorkerDao WorkerDaoInstance;

        private Service()
        {
            ItemDaoInstance = new ItemDao();
            Items = ItemDaoInstance.GetItems() ?? new List<Item>();
            ReceiptLineDaoInstance = new ReceiptLineDao(Items);
            ReceiptDaoInstance = new ReceiptDao(Items);
            WorkerDaoInstance = new WorkerDao(Items);
        }
        public void Refresh()
        {
            Items = ItemDaoInstance.GetItems() ?? new List<Item>();
            ReceiptLineDaoInstance.SetItems(Items);
            ReceiptDaoInstance.SetItems(Items);
            WorkerDaoInstance.SetItems(Items);
        }
    }
}
