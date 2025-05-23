﻿using Bintainer.Model.DTO;

namespace Bintainer.Model.Request
{
    public class RegisterOrderRequest
    {
        public string? OrderNumber { get; set; }
        public string? Supplier { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? HandoverDate { get; set; }
        public List<OrderItem> Parts { get; set; }
    }
}
