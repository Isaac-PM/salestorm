using Microsoft.AspNetCore.SignalR;
using Inventory.Logic;

#pragma warning disable CS8600
namespace Inventory.Hubs
{
    public class InventoryHub : Hub
    {
        private static Service _service = Service.Instance;
        private readonly string _groupName = "Inventory";

        public async Task UserLogin(string userName, string userPassword)
        {
            Console.WriteLine($"UserLogin({userName}, {userPassword})");
            await Groups.AddToGroupAsync(Context.ConnectionId, _groupName);
            _service.Refresh(); // TODO Not sure of the efficiency of this.
            Worker worker = _service.WorkerDaoInstance.GetWorker(userName, userPassword);
            await Clients.Caller.SendAsync("UserLogin", worker);
        }

        public async Task GetInventory()
        {
            Console.WriteLine("GetInventory()");
            _service.Refresh();
            List<Item> inventory = _service.ItemDaoInstance.GetItems();
            await Clients.Caller.SendAsync("GetInventory", inventory);
        }

        public async Task GenerateReceipt(Receipt receipt)
        {
            List<ReceiptLine> lines = receipt.Lines;
            int workerId = receipt.WorkerId;
            Console.WriteLine($"GenerateReceipt({receipt.Id})");
            _service.Refresh();
            bool receiptInserted = _service.ReceiptDaoInstance.InsertReceipt(receipt);
            if (receiptInserted)
            {
                bool linesInsertedAll = lines.All(line => _service.ReceiptLineDaoInstance.InsertReceiptLine(line));
                if (linesInsertedAll)
                {
                    bool linesApplied = _service.ReceiptLineDaoInstance.ApplyReceiptLines(workerId);
                    if (linesApplied)
                    {
                        _service.Refresh();
                        List<Item> inventory = _service.ItemDaoInstance.GetItems();
                        await Clients.Group(_groupName).SendAsync("GetInventory", inventory);
                        await Clients.Caller.SendAsync("GenerateReceipt", "success");
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("GenerateReceipt", "failure");
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("GenerateReceipt", "failure");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("GenerateReceipt", "failure");
            }
        }
    }
}
#pragma warning restore CS8600
