namespace albumica.Models
{
    public class GeoCodeModel
    {
        public bool IsSuccess { get; set; }
        public string? CountryCode { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Suburb { get; set; }
    }
}