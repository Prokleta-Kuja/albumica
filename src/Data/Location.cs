namespace albumica.Data
{
    public class Location
    {
        public int LocationId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? SuburbId { get; set; }

        public Country? Country { get; set; }
        public City? City { get; set; }
        public Suburb? Suburb { get; set; }
        public Image? Image { get; set; }

        public bool HasGpsData => Latitude != 0 && Longitude != 0;
        public bool HasData => CountryId.HasValue || CityId.HasValue || SuburbId.HasValue;
    }
}