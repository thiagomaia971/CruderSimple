using System.Reflection;

namespace CruderSimple.Core.Extensions
{
    public static class ObjectExtenions
    {
        public static object GetValueByPropertyName(this object value, string propertyName)
        {
            if (value == null)
                return default;
            var splited = propertyName.Split('.');
            var itemProperties = value.GetType().GetProperty(splited[0]);
            if (itemProperties is null)
                return default;

            var currentValue = itemProperties.GetValue(value);
            if (splited.Length == 1)
                return currentValue;

            return GetValueByPropertyName(currentValue, string.Join(".", splited.Skip(1)));
        }

        public static PropertyInfo GetPropertyByPropertyName(this object value, string propertyName)
        {
            var splited = propertyName.Split('.');
            var itemProperties = value.GetType().GetProperty(splited[0]);
            return itemProperties;
        }

        public static T GetValueByPropertyName<T>(this object value, string propertyName)
        {
            if (value == null)
                return default;
            var itemProperties = value.GetPropertyByPropertyName(propertyName);
            if (itemProperties == null)
                return default;
            
            var currentValue = itemProperties.GetValue(value);
            if (propertyName.Split('.').Length == 1)
                return currentValue == null ? (T) default : (T) currentValue;

            return GetValueByPropertyName<T>(currentValue, string.Join(".", propertyName.Split('.').Skip(1)));
        }

        public static void SetValueByPropertyName(this object @object, object value, string propertyName)
        {
            if (@object == null)
                return;

            var splited = propertyName.Split('.');
            var itemProperties = @object.GetType().GetProperty(splited[0]);
            if (itemProperties == null)
                return;
            var currentValue = itemProperties.GetValue(@object);

            if (splited.Length == 1)
            {
                itemProperties.SetValue(@object, value, null);
                return;
            }

            currentValue?.SetValueByPropertyName(value, string.Join(".", splited.Skip(1)));
        }

        public static Type GetPropertyTypeByName(this object value, string propertyName)
        {
            if (value == null)
                return default;
            var splited = propertyName.Split('.');
            var itemProperties = value.GetType().GetProperty(splited[0]);
            if (itemProperties == null)
                return default;

            var currentValue = itemProperties.GetValue(value);
            if (splited.Length == 1)
                return itemProperties.PropertyType;

            return GetPropertyTypeByName(currentValue, string.Join(".", splited.Skip(1)));
        }
    }
}
