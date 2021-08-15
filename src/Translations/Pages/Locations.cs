namespace albumica.Translations
{
    public interface ILocations : IStandard
    {
        string PageTitle { get; }
        string CountryTab { get; }
        string CountryAddTitle { get; }
        string CountryEditTitle { get; }
        string CountryName { get; }
        string CountryDisplayName { get; }
        string CountryCode { get; }
        string CountryTableName { get; }
        string CountryTableCode { get; }
        string CountryTableCities { get; }
        string CountryTableSuburbs { get; }
        string CityTab { get; }
        string CityAddTitle { get; }
        string CityEditTitle { get; }
        string CityCountry { get; }
        string CityName { get; }
        string CityDisplayName { get; }
        string CityTableName { get; }
        string CityTableCountry { get; }
        string CityTableSuburbs { get; }
        string SuburbTab { get; }
        string SuburbAddTitle { get; }
        string SuburbEditTitle { get; }
        string SuburbCity { get; }
        string SuburbName { get; }
        string SuburbDisplayName { get; }
        string SuburbTableName { get; }
        string SuburbTableCountry { get; }
        string SuburbTableCity { get; }
        string Country { get; }
        string City { get; }
        string Suburb { get; }
    }
    public class Locations_en : Standard_en, ILocations
    {
        public string PageTitle => "Locations";
        public string CountryTab => "Countries";
        public string CountryAddTitle => "Add Country";
        public string CountryEditTitle => "Edit Country";
        public string CountryName => "Country Name";
        public string CountryDisplayName => "Country Display Name";
        public string CountryCode => "Country Code";
        public string CountryTableName => "Country";
        public string CountryTableCode => "Code";
        public string CountryTableCities => "Cities";
        public string CountryTableSuburbs => "Suburbs";
        public string CityTab => "Cities";
        public string CityAddTitle => "Add City";
        public string CityEditTitle => "Edit City";
        public string CityCountry => "City Country";
        public string CityName => "City Name";
        public string CityDisplayName => "City Display Name";
        public string CityTableName => "City";
        public string CityTableCountry => "Country";
        public string CityTableSuburbs => "Suburbs";
        public string SuburbTab => "Suburbs";
        public string SuburbAddTitle => "Add Suburb";
        public string SuburbEditTitle => "Edit Suburb";
        public string SuburbCity => "Suburb City";
        public string SuburbName => "Suburb Name";
        public string SuburbDisplayName => "Suburb Display Name";
        public string SuburbTableName => "Suburb";
        public string SuburbTableCountry => "Country";
        public string SuburbTableCity => "City";
        public string Country => "Country";
        public string City => "City";
        public string Suburb => "Suburb";
    }
    public class Locations_hr : Standard_hr, ILocations
    {
        public string PageTitle => "Lokacije";
        public string CountryTab => "Države";
        public string CountryAddTitle => "Dodaj državu";
        public string CountryEditTitle => "Izmijeni državu";
        public string CountryName => "Naziv države";
        public string CountryDisplayName => "Naziv države za prikaz";
        public string CountryCode => "Kod države";
        public string CountryTableName => "Država";
        public string CountryTableCode => "Kod";
        public string CountryTableCities => "Gradova";
        public string CountryTableSuburbs => "Četvrti";
        public string CityTab => "Gradovi";
        public string CityAddTitle => "Dodaj grad";
        public string CityEditTitle => "Izmijeni grad";
        public string CityCountry => "Država grada";
        public string CityName => "Naziv grada";
        public string CityDisplayName => "Naziv grada za prikaz";
        public string CityTableName => "Grad";
        public string CityTableCountry => "Država";
        public string CityTableSuburbs => "Četvrti";
        public string SuburbTab => "Četvrti";
        public string SuburbAddTitle => "Dodaj četvrt";
        public string SuburbEditTitle => "Izmijeni četvrt";
        public string SuburbCity => "Grad";
        public string SuburbName => "Naziv četvrti";
        public string SuburbDisplayName => "Naziv četvrti";
        public string SuburbTableName => "Četvrt";
        public string SuburbTableCountry => "Država";
        public string SuburbTableCity => "Grad";
        public string Country => "Država";
        public string City => "Grad";
        public string Suburb => "Četvrt";
    }
}