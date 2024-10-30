using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Interface
{
    public interface IOrderRepository
    {
        public Order? GetOrderByOrderNumber(string orderNumber,string userId);
        public Order? AddAndSaveOrder(Order? order);
        public Order? UpdateOrder(Order? order);
        public List<OrderInfo>? FilterOrder(FilterOrderRequestModel request);
    }
}
