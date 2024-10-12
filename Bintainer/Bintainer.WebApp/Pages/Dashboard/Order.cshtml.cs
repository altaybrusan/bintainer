using Bintainer.WebApp.Data.DTOs;
using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class SearchOrderRequestModel
    {
        public string? OrderNumber { get; set; }
        public string? Supplier { get; set; }
        public DateTime? FromDate { get; set; } = null;
        public DateTime? ToDate { get; set; } = null;
    }
    public class OrderModel : PageModel
    {
        BintainerDbContext _dbcontext;
        public List<Part> Part { get; set; } = new List<Part>();

        public OrderModel(BintainerDbContext dbContext)
        {
            _dbcontext = dbContext;
            Part= _dbcontext.Parts.ToList();
        }

        public void OnGet()
        {

        }

        public IActionResult OnPostRegisterNewOrder([FromBody]RegisterOrderRequestModel request)
        {
            if (ModelState.IsValid) 
            {
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
                Order? registeredOrder = _dbcontext.Orders.Include(o => o.OrderPartAssociations).FirstOrDefault(o => o.OrderNumber == request.OrderNumber);
                
                if(registeredOrder is null) 
                {
                    Order order = new Order();
                    order.OrderNumber = request.OrderNumber;
                    order.OrderDate = request.OrderDate;
                    order.HandOverDate = request.HandoverDate;
                    order.Supplier = request.Supplier;
                    order.UserId = UserId;
                    foreach (var item in request.Parts)
                    {
                        Part? part = _dbcontext.Parts.FirstOrDefault(c => c.Id == item.PartId);
                        if (part is not null) 
                        {
                            OrderPartAssociation association = new OrderPartAssociation();
                            association.PartId = part.Id;
                            association.Qunatity= item.Quantity;
                            order.OrderPartAssociations.Add(association);
                        }
                    }
                    _dbcontext.Orders.Add(order);
                    _dbcontext.SaveChanges(true);
                }
                else 
                {
                    registeredOrder.OrderDate = request.OrderDate;
                    registeredOrder.HandOverDate= request.HandoverDate;
                    registeredOrder.Supplier = request.Supplier;
                    foreach (var item  in registeredOrder.OrderPartAssociations) 
                    {
                        item.Qunatity = request.Parts.FirstOrDefault(p => p.PartId == item.PartId)?.Quantity;
                    }
                    _dbcontext.Orders.Update(registeredOrder);
                    _dbcontext.SaveChanges(true);

                }

            }


            // Return a success response
            return new OkResult();
        }

        public IActionResult OnPostSearchOrder([FromBody] SearchOrderRequestModel request)
        {
            if (ModelState.IsValid)
            {
                // Build the query with optional search parameters
                var ordersQuery = _dbcontext.Orders.AsQueryable();

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
                var orders = ordersQuery
                    .Select(o => new
                    {
                        o.OrderNumber,
                        o.Supplier,
                        o.OrderDate,
                        o.HandOverDate,
                        Parts = _dbcontext.OrderPartAssociations
                                    .Where(opa => opa.OrderId == o.Id)
                                    .Select(opa => new
                                    {
                                        opa.Part.Name,
                                        opa.Qunatity
                                    }).ToList()
                    })
                    .ToList();

                if (orders.Any())
                {
                    return new JsonResult(new { success = true, orders });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No results found." });
                }
            }

            return BadRequest("Invalid search parameters.");
        }
    }
}
