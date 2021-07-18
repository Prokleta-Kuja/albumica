using System;

namespace albumica.Localization
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
        // public static INavigation Navigation() => Navigation(C.Env.Locale);
        // public static INavigation Navigation(string locale)
        // {
        //     if (locale.StartsWith("hr"))
        //         return new Navigation_hr();

        //     return new Navigation_en();
        // }
    }
}