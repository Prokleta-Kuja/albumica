using albumica.Data;

namespace albumica.Models
{
    public class CountryModel
    {
        public CountryModel()
        {
            Name = null!;
            Code = null!;
            DisplayName = null!;
        }
        public CountryModel(Country c, int cityCount = 0, int suburbCount = 0)
        {
            CountryId = c.CountryId;
            Name = c.Name;
            Code = c.Code;
            DisplayName = c.DisplayName;
            CityCount = cityCount;
            SuburbCount = suburbCount;
        }
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public int CityCount { get; set; }
        public int SuburbCount { get; set; }
    }
}