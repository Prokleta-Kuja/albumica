using System;

namespace albumica.Translations
{
    public static class LocalizationFactory
    {
        public static Formats Formats() => Formats(C.Env.Locale, C.Env.TimeZone);
        public static Formats Formats(string locale, string timeZone) => new(locale, timeZone);
        public static IStandard Standard() => Standard(C.Env.Locale);
        public static IStandard Standard(string locale)
        {
            if (locale.StartsWith("hr"))
                return new Standard_hr();

            return new Standard_en();
        }
        public static INavigation Navigation() => Navigation(C.Env.Locale);
        public static INavigation Navigation(string locale)
        {
            if (locale.StartsWith("hr"))
                return new Navigation_hr();

            return new Navigation_en();
        }
        public static ILocations Locations() => Locations(C.Env.Locale);
        public static ILocations Locations(string locale)
        {
            if (locale.StartsWith("hr"))
                return new Locations_hr();

            return new Locations_en();
        }
        public static IImport Import() => Import(C.Env.Locale);
        public static IImport Import(string locale)
        {
            if (locale.StartsWith("hr"))
                return new Import_hr();

            return new Import_en();
        }
    }
}