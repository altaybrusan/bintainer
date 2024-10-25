using System.Text.Json.Serialization;

namespace Bintainer.Model.DTO
{
    public class DigikeySettings
    {
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public string? GrantType { get; set; }
	}
}
