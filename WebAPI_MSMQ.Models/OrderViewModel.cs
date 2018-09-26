namespace WebAPI_MSMQ.Models
{
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public string InformationMessage { get; set; }
        public int ProductQuantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}