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
        string Delete { get; }
        string Skip { get; }
        string ImportImage { get; }
        string SinglePersonInImage { get; }
        string LoadingGps { get; }
        string GpsFound { get; }
        string GpsNotFound { get; }
        string NothingToImport { get; }
        string Duplicate { get; }
        string Importing { get; }
        string Date { get; }
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
        public string Delete => "Delete";
        public string Skip => "Skip";
        public string ImportImage => "Import";
        public string SinglePersonInImage => "Single person in image - use for image recognition";
        public string LoadingGps => "Loading Coordinates";
        public string GpsFound => "Coordinates found";
        public string GpsNotFound => "Coordinates not found";
        public string NothingToImport => "Nothing to import";
        public string Duplicate => "Duplicate";
        public string Importing => "Importing";
        public string Date => "Date";
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
        public string Delete => "Obriši";
        public string Skip => "Preskoči";
        public string ImportImage => "Uvezi";
        public string SinglePersonInImage => "Jedna osoba na slici - koristi za prepoznavanje lica";
        public string LoadingGps => "Učitavam koordinate";
        public string GpsFound => "Koordinate pronađene";
        public string GpsNotFound => "Koordinate nisu pronađene";
        public string NothingToImport => "Nema više ništa za uvoz";
        public string Duplicate => "Duplikat";
        public string Importing => "Uvozim";
        public string Date => "Datum";
    }
}