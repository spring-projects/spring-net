

using System;

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
    }
}