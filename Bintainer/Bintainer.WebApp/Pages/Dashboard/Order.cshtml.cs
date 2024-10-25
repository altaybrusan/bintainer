using AutoMapper;
using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Resources;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using static NuGet.Packaging.PackagingConstants;

namespace Bintainer.WebApp.Pages.Dashboard
{
    public class OrderModel : PageModel
    {
        public List<Part> Part { get; set; } = new List<Part>();

        private readonly IOrderService _orderService;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        public OrderModel(IOrderService orderService, IStringLocalizer<ErrorMessages> localizer)
        {
            _orderService = orderService;
            _localizer = localizer;

            var currentCulture = CultureInfo.CurrentCulture;
            var currentUICulture = CultureInfo.CurrentUICulture;



            //TODO: warning check this out
            //Part= _dbcontext.Parts.ToList();
        }

        public void OnGet()
        {
            var test = _localizer["Test1"];
        }

        public IActionResult OnPostRegisterNewOrder([FromBody]RegisterOrderRequest request)
        {
            if (ModelState.IsValid) 
            {
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

                try
                {
                    _orderService.RegisterOrder(request, UserId);
                }
                catch (Exception ex)
                {
                    //TODO: make sure to throw correct message
                    throw new Exception(ex.Message);
                }
            }

            return new OkResult();
        }

        public IActionResult OnPostSearchOrder([FromBody] FilterOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    success = false,
                    message = _localizer["ErrorModelStateError"],
                });
            }
            try
            {
                var ordersViewModel = _orderService.FilterOrder(request);
                if (ordersViewModel is not null && ordersViewModel.Any())
                {
                    return new JsonResult(new { success = true, ordersViewModel });
                }
                else
                {
                    return new JsonResult(new { success = false, message = _localizer["WarningNotFound"] });
                }
            }
            catch (Exception)
            {
                throw;
            }
        
        }
    }
}
