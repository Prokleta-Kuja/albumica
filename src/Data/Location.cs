namespace albumica.Data
{
    public class Location
    {
        public int LocationId { get; set; }
        public int CityId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public City? City { get; set; }
        public Image? Image { get; set; }
    }
}