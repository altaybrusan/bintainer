using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service.Interface
{
    public interface IOrderService
    {
        public Response<Order?> RegisterOrder(RegisterOrderRequest request, string userId);
        public Response<List<OrderViewModel>?> FilterOrder(FilterOrderRequest request);

    }
}
