namespace CruderSimple.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToStartDate(this DateTime date) 
            => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        public static DateTime ToEndDate(this DateTime date)
            => new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
    }
}
