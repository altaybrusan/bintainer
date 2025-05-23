﻿using System.ComponentModel.DataAnnotations;

namespace Bintainer.Model.Request
{
    public class CreatePartRequest
    {
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
        public string? Supplier { get; set; }
        public string? Package { get; set; } = string.Empty;
        public string? Group { get; set; }
        public string? OrderNumber { get; set; }
        public string? OrderQuantity { get; set; }
        public string? FootprintUrl { get; set; }
        public string? PartUrl { get; set; }
        public Guid? AttributeTemplateGuid { get; set; }
        public DateTime? OrderDate { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public List<string?>? PathToCategory { get; set; } // Add the Path property
    }
}
