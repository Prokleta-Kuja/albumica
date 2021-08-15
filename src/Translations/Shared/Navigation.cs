namespace albumica.Translations
{
    public interface INavigation : IStandard
    {
        string App { get; }
        string Import { get; }
        string Locations { get; }
    }
    public class Navigation_en : Standard_en, INavigation
    {
        public string App => "albumica";
        public string Import => "Import";
        public string Locations => "Locations";
    }
    public class Navigation_hr : Standard_hr, INavigation
    {
        public string App => "albumica";
        public string Import => "Uvoz";
        public string Locations => "Lokacije";
    }
}