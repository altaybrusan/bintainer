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
            Order order = new Order();


            // Return a success response
            return new OkResult();
        }

    }
}
