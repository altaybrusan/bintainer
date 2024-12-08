using AutoMapper;
using Bintainer.Model;
using Bintainer.Model.Entity;
using Bintainer.Model.Request;
using Bintainer.Service;
using Bintainer.Service.Interface;
using Bintainer.SharedResources.Interface;
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
        public List<Part> Parts { get; set; } = new List<Part>();

        private readonly IOrderService _orderService;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
        private readonly IPartService _partService;
        private readonly IAppLogger _appLogger;
        public OrderModel(IOrderService orderService, 
                          IStringLocalizer<ErrorMessages> localizer,
                          IPartService partService,
                          IAppLogger appLogger)
        {
            _orderService = orderService;
            _localizer = localizer;
            _appLogger = appLogger;
            _partService = partService;

            //TODO: warning check this out
            //Part= _dbcontext.Parts.ToList();
        }

        public void OnGet()
        {
            var userId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            Parts = _partService.GetPartsByUserId(userId).Result;
        }

        public IActionResult OnPostRegisterNewOrder([FromBody]RegisterOrderRequest request)
        {
            if (ModelState.IsValid) 
            {
                var UserId = User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

                try
                {
                    var response = _orderService.RegisterOrder(request, UserId);
                    return new JsonResult(response.Message);
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
                _appLogger.LogModelError(nameof(OnPostSearchOrder), ModelState);
                
                return BadRequest(new
                {
                    success = false,
                    message = _localizer["ErrorModelStateError"],
                });
            }
            try
            {
                var ordersViewModel = _orderService.FilterOrder(request);
                if (ordersViewModel.Result is not null && ordersViewModel.Result.Any())
                {
                    return new JsonResult(new { success = true, ordersViewModel.Result });
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
