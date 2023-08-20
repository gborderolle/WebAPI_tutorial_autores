using NodaTime;
using System.Globalization;

namespace WebAPI_tutorial_recursos.Utilities
{
    public static class GlobalServices
    {
        public static DateTime GetDatetimeUruguay()
        {
            var nowInUruguay = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb["America/Montevideo"]);
            return nowInUruguay.ToDateTimeUnspecified();
        }

        public static string GetDatetimeUruguayString()
        {
            CultureInfo culture = new CultureInfo("es-UY");
            var nowInUruguay = SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb["America/Montevideo"]);
            return nowInUruguay.ToDateTimeUnspecified().ToString("G", culture);
        }

    }
}
