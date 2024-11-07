using Bintainer.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.View
{
	public class InventorySectionViewModel
	{
		public string? SectionName { get; set; }

		public int? Width { get; set; }

		public int? Height { get; set; }

		public int? SubspaceCount { get; set; }
	}
}
