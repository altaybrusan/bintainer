using System.ComponentModel.DataAnnotations;

namespace Bintainer.WebApp.Data.DTOs
{
    public class CreatePartRequestModel
    {
        public string? PartName { get; set; }
        public string? Description { get; set; }
        public string? Package { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? Group { get; set; }
        public string? OrderNumber { get; set; }
        public string? OrderQuantity { get; set; }
        public string? FootprintUrl { get; set; }
        public string? PartUrl { get; set; }
        public int? AttributeTemplateId { get; set; }
        public DateTime? OrderDate { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
    }
}
