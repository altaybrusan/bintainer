using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Model.Template
{
    public class Response<T>
    {
        public bool IsSuccess { get; set; }
        public T? Result { get; set; }
        public string? Message { get; set; }
    }
}
