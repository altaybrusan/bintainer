using System.Text.Json.Serialization;

namespace Bintainer.WebApp.Services
{
    public class DigikeySettings
    {
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public string? GrantType { get; set; }
	}
}
