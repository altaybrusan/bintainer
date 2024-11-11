using Azure.Core;
using Bintainer.Model;
using Bintainer.Model.DTO;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bintainer.Repository.Service
{
    public class OrderRepository:IOrderRepository
    {
        BintainerDbContext _dbContext;
        public OrderRepository(BintainerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Order? GetOrderByOrderNumber(string orderNumber, string userId)
        {
            Order? registeredOrder = _dbContext.Orders.Include(o => o.OrderPartAssociations)
                                                      .FirstOrDefault(o => o.OrderNumber == orderNumber && o.UserId == userId);
            return registeredOrder;
        }
        public Order? AddAndSaveOrder(Order? order) 
        {
            if(order is null)
                return order;

            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges(true);
            return order;
        }
        public Order? UpdateOrder(Order? order) 
        {
            if(order is null)
                return order;

            _dbContext.Orders.Update(order);
            _dbContext.SaveChanges(true);
            return order;
        }
        public List<OrderInfo>? FilterOrder(FilterOrderRequestModel request)
        {
            // Build the query with optional search parameters
            var ordersQuery = _dbContext.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                ordersQuery = ordersQuery.Where(o => o.OrderNumber.Contains(request.OrderNumber));
            }

            if (!string.IsNullOrWhiteSpace(request.Supplier))
            {
                ordersQuery = ordersQuery.Where(o => o.Supplier.Contains(request.Supplier));
            }

            if (request.FromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate <= request.ToDate.Value);
            }

            // Fetch the orders and associated parts (join with OrderPartAssociations)
            var ordersView = ordersQuery
                .Select(o => new OrderInfo
                {
                    OrderNumber = o.OrderNumber,
                    Supplier = o.Supplier,
                    OrderDate = o.OrderDate,
                    HandOverDate = o.HandOverDate,
                    Parts = _dbContext.OrderPartAssociations
                                .Where(opa => opa.OrderId == o.Id)
                                .Select(opa => new PartSummary
                                {
                                    PartName = opa.Part.Number,
                                    Quantity = opa.Quantity
                                }).ToList()
                })
                .ToList();
            
            return ordersView;     
        }
    }
}
