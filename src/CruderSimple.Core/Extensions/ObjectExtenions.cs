namespace CruderSimple.Core.Extensions
{
    public static class ObjectExtenions
    {
        public static double GetValueByPropertyName(this object value, string propertyName)
        {
            var itemProperties = value.GetType().GetProperty(propertyName);
            if (itemProperties is null)
                return 0;
            return (double) value;
        }
        public static T GetValueByPropertyName<T>(this object value, string propertyName)
        {
            var itemProperties = value.GetType().GetProperty(propertyName);
            if (itemProperties is null)
                return default;
            return (T)value;
        }
    }
}
