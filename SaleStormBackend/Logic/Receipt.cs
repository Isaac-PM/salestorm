using System.Text;

namespace Inventory.Logic
{
    public class Receipt
    {
        public long Id { get; set; }
        public int WorkerId { get; set; }
        public float GrandTotal { get; set; }
        public List<ReceiptLine>? Lines { get; set; }

        public Receipt(
            long Id = -1,
            int WorkerId = -1,
            float GrandTotal = 0,
            List<ReceiptLine>? Lines = null)
        {
            this.Id = Id;
            this.WorkerId = WorkerId;
            this.GrandTotal = GrandTotal;
            if (Lines == null)
            {
                this.Lines = new List<ReceiptLine>();
            }
            else
            {
                this.Lines = Lines;
                ComputeGrandTotal();
            }
        }

        public void ComputeGrandTotal()
        {
            if (Lines != null && Lines.Count > 0)
            {
                GrandTotal = 0;
                foreach (ReceiptLine line in Lines)
                {
                    GrandTotal += line.LineTotal;
                }
            }
            else
            {
                GrandTotal = 0;
            }
        }

        public void AddLine(ReceiptLine line)
        {
            if (Lines != null && line != null)
            {
                line.ReceiptId = Id;
                Lines.Add(line);
                GrandTotal += line.LineTotal;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Receipt:\n");
            sb.Append($"Id: {Id}\n");
            sb.Append($"WorkerId: {WorkerId}\n");
            sb.Append($"GrandTotal: {GrandTotal}\n");
            sb.Append($"Lines:\n");
            if (Lines != null)
            {
                foreach (ReceiptLine line in Lines)
                {
                    sb.Append(line.ToString());
                }
            }
            else
            {
                sb.Append("No lines\n");
            }
            return sb.ToString();
        }
    }
}
