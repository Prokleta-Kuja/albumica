namespace albumica.Translations
{
    public interface IImport : IStandard
    {
        string PageTitle { get; }
        string ImageToBeImported { get; }
        string Location { get; }
        string Country { get; }
        string City { get; }
        string Suburb { get; }
        string Persons { get; }
    }
    public class Import_en : Standard_en, IImport
    {
        public string PageTitle => "Import";
        public string ImageToBeImported => "Image to be imported";
        public string Location => "Location";
        public string Country => "Country";
        public string City => "City";
        public string Suburb => "Suburb";
        public string Persons => "Persons";
    }
    public class Import_hr : Standard_hr, IImport
    {
        public string PageTitle => "Uvoz";
        public string ImageToBeImported => "Slika za uvoz";
        public string Location => "Lokacija";
        public string Country => "Država";
        public string City => "Grad";
        public string Suburb => "Četvrt";
        public string Persons => "Osobe";
    }
}