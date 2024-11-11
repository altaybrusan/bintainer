using AutoMapper;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Model.Template;
using Bintainer.Model.View;
using Bintainer.Repository.Interface;
using Bintainer.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPartRepository _partRepository;
        private readonly IMapper _mapper;
        public OrderService(IOrderRepository orderRepository,
                            IPartRepository partRepository,
                            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _partRepository = partRepository;
            _mapper = mapper;
        }

        public Response<Order?> RegisterOrder(RegisterOrderRequest request, string userId)
        {
            Order? registeredOrder = _orderRepository.GetOrderByOrderNumber(request.OrderNumber, userId);
            if (registeredOrder is null)
            {
                Order order = new Order();
                order.OrderNumber = request.OrderNumber;
                order.OrderDate = request.OrderDate;
                order.HandOverDate = request.HandoverDate;
                order.Supplier = request.Supplier;
                order.UserId = userId;
               
                //TODO: update this
                //foreach (var item in request.Parts)
                //{
                //    Part? part = _partRepository.GetPart(item);
                //    if (part is not null)
                //    {
                //        OrderPartAssociation association = new OrderPartAssociation();
                //        association.PartId = part.Id;
                //        association.Quantity = item.Quantity;
                //        order.OrderPartAssociations.Add(association);
                //    }
                //}
                
                _orderRepository.AddAndSaveOrder(order);
                
                return new Response<Order?>() 
                {
                    IsSuccess = true,
                    Result = order 
                };
            }
            else
            {
                registeredOrder.OrderDate = request.OrderDate;
                registeredOrder.HandOverDate = request.HandoverDate;
                registeredOrder.Supplier = request.Supplier;
                foreach (var item in registeredOrder.OrderPartAssociations)
                {
                    item.Quantity = request.Parts.FirstOrDefault(p => p.PartId == item.PartId)?.Quantity;
                }
                _orderRepository.UpdateOrder(registeredOrder);                
            }
            return new Response<Order?>()
            {
                IsSuccess = true,
                Result = registeredOrder
            };
        }

        public Response<List<OrderViewModel>?> FilterOrder(FilterOrderRequest request) 
        {
            var requestModel = _mapper.Map<FilterOrderRequestModel>(request);

            var order =  _orderRepository.FilterOrder(requestModel);
            
            if (order == null)
            {
                return new Response<List<OrderViewModel>?>() 
                { 
                    IsSuccess = true, 
                    Result = null 
                };
            }

            var ordersViewModel = order.Select(o => new OrderViewModel
            {
                OrderNumber = o.OrderNumber,
                Supplier = o.Supplier,
                OrderDate = o.OrderDate,
                HandOverDate = o.HandOverDate,
                Parts = o.Parts.Select(p => new PartSummary
                {
                    PartName = p.PartName,
                    Quantity = p.Quantity
                }).ToList()
            }).ToList();

            return new Response<List<OrderViewModel>?>()
            {
                IsSuccess = true,
                Result = ordersViewModel
            };
        }
    }
}
