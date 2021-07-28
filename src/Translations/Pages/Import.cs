namespace albumica.Translations
{
    public interface IImport : IStandard
    {
        string PageTitle { get; }
    }
    public class Import_en : Standard_en, IImport
    {
        public string PageTitle => "Import";
    }
    public class Import_hr : Standard_hr, IImport
    {
        public string PageTitle => "Uvoz";
    }
}