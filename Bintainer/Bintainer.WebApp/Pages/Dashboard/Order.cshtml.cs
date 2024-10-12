using Bintainer.WebApp.Data.DTOs;
using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bintainer.WebApp.Pages.Dashboard
{
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

    }
}
