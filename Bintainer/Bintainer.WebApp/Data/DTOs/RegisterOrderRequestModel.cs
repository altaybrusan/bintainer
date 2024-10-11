namespace Bintainer.WebApp.Data.DTOs
{
    public record OrderItem(string PartName, int Quantity, int PartId);
    public class RegisterOrderRequestModel
    {
        public string? OrderNumber { get; set; }
        public string? Supplier { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime HandoverDate { get; set; }
        public List<OrderItem> Parts { get; set; }
    }
}
