namespace Spring.Util
{
    /// <summary>
    /// General utility methods for working with annotations
    /// </summary>
    public class AttributeUtils
    {
        /// <summary>
        /// Find a single Attribute of the type 'attributeType' from the supplied class,
        /// traversing it interfaces and super classes if no attribute can be found on the
        /// class iteslf.
        /// </summary>
        /// <remarks>
        /// This method explicitly handles class-level attributes which are not declared as
        /// inherited as well as attributes on interfaces.
        /// </remarks>
        /// <param name="type">The class to look for attributes on .</param>
        /// <param name="attributeType">Type of the attribibute to look for.</param>
        /// <returns>the attribute of the given type found, or <code>null</code></returns>
        public static Attribute FindAttribute(Type type, Type attributeType)
        {
            Attribute[] attributes = Attribute.GetCustomAttributes(type, attributeType, false);  // we will traverse hierarchy ourselves.
            if (attributes.Length > 0)
            {
                return attributes[0];
            }
            foreach (Type interfaceType in type.GetInterfaces())
            {
                Attribute attrib = FindAttribute(interfaceType, attributeType);
                if (attrib != null)
                {
                    return attrib;
                }
            }
            if (type.BaseType == null)
            {
                return null;
            }
            return FindAttribute(type.BaseType, attributeType);
        }

        /// <summary>
        /// Get all attribute properties with values for a specific attribute type
        /// </summary>
        /// <param name="attribute">attribute to check against</param>
        /// <returns>collection of all properties with values</returns>
        public static IDictionary<string, object> GetAttributeProperties(Attribute attribute)
        {
            Type attributeType = attribute.GetType();
            IDictionary<string, object> attributes = new Dictionary<string, object>();
            foreach(var property in attributeType.GetProperties())
            {
                object value = property.GetValue(attribute, null);
                attributes.Add(property.Name, value);
            }
            return attributes;
        }

        /// <summary>
        /// Get the default name value of an attribute and a specific property
        /// </summary>
        /// <param name="attribute">attribute from where to get the default value</param>
        /// <param name="propertyName">property to get the default value</param>
        /// <returns></returns>
        public static object GetDefaultValue(Attribute attribute, string propertyName)
        {
            Type attributeType = attribute.GetType();
            try
            {
                var property = attributeType.GetProperty(propertyName);
                if (property == null)
                    return null;
                var instance = Activator.CreateInstance(attributeType);

                return property.GetValue(instance, null);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
