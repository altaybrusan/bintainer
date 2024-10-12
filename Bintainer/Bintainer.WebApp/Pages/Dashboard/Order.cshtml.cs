using Bintainer.WebApp.Data.DTOs;
using Bintainer.WebApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
                        order.Parts.Add(part);
                }
                _dbcontext.Orders.Add(order);
                _dbcontext.SaveChanges(true);                
            }


            // Return a success response
            return new OkResult();
        }

    }
}
