namespace CruderSimple.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToStartDate(this DateTime date) 
            => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        public static DateTime ToEndDate(this DateTime date)
            => new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

        public static string RelativeDate(this DateTime dateTime)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "um segundo atrás" : ts.Seconds + " segundoss atrás";

            if (delta < 2 * MINUTE)
                return "um minuto atrás";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutos atrás";

            if (delta < 90 * MINUTE)
                return "uma hora atrás";

            if (delta < 24 * HOUR)
                return ts.Hours + " horas atrás";

            if (delta < 48 * HOUR)
                return "ontem";

            if (delta < 30 * DAY)
                return ts.Days + " dias atrás";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "um mês atrás" : months + " mês atrás";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "um ano atrás" : years + " anos atrás";
            }

        }
    }
}
