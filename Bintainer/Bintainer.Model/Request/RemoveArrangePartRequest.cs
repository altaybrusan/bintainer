﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Request
{
    public class RemoveArrangePartRequest
    {
        public string? PartNumber { get; set; }
        public int SectionId { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public string? Label { get; set; }
        public string? Group { get; set; }
        public Dictionary<int, int>? SubspaceQuantities { get; set; }
        public bool IsFilled { get; set; }
        public int? FillAllQuantity { get; set; }
        public string? SectionName { get; set; }
    }
}
