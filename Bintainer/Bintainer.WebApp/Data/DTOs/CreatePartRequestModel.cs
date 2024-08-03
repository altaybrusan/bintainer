namespace Bintainer.WebApp.Data.DTOs
{
    public class CreatePartRequestModel
    {
        public string? PartName { get; set; }
        public string? Description { get; set; }
        public string? Label { get; set; }
        public string? Package { get; set; }
        public string? Category { get; set; }
        public string? Group { get; set; }
        public string? OrderNumber { get; set; }
        public string? OrderQuantity { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? SelectedAttributeTableIndex { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
    }
}
