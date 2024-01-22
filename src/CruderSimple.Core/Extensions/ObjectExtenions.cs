namespace CruderSimple.Core.Extensions
{
    public static class ObjectExtenions
    {
        public static double GetValueByPropertyName(this object value, string propertyName)
        {
            var itemProperties = value.GetType().GetProperty(propertyName);
            if (itemProperties is null)
                return 0;
            return (double)itemProperties.GetValue(value);
        }
        public static T GetValueByPropertyName<T>(this object value, string propertyName)
        {
            var itemProperties = value.GetType().GetProperty(propertyName);
            if (itemProperties is null)
                return default;
            if (itemProperties.GetValue(value) == null)
                return default;
            return (T) itemProperties.GetValue(value);
        }
        public static Type GetPropertyTypeByName(this object value, string propertyName)
        {
            var itemProperties = value.GetType().GetProperty(propertyName);
            if (itemProperties is null)
                return default;
            return itemProperties.PropertyType;
        }
    }
}
