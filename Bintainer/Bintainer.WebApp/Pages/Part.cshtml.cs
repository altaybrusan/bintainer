using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bintainer.WebApp.Pages
{
    public class DropdownViewModel
    {
        public List<string> Items { get; set; }
    }
    public class PartModel : PageModel
    {
        public DropdownViewModel DropdownModel { get; set; }
        public void OnGet()
        {
            DropdownModel = new DropdownViewModel
            {
                Items = new List<string> { "Resistor", "Capacitor", "Transistor", "Diode" }
            };
        }
    }
}
