

using System;
using System.Collections;
using Spring.Core.TypeResolution;

namespace Spring.EmsQuickStart.Common.Converters
{
    public class TypeMapper : ITypeMapper
    {
        private string defaultNamespace;
        private string defaultAssemblyName;

        //Generics not used to support both 1.1 and 2.0
        private IDictionary idTypeMapping;
        private IDictionary typeIdMapping;
        
        
        //TODO generalize?
        private string defaultHashtableTypeId = "Hashtable";

        private Type defaultHashtableClass = typeof(Hashtable);

        public TypeMapper()
        {
            idTypeMapping = new Hashtable();
            typeIdMapping = new Hashtable();
        }


        public IDictionary IdTypeMapping
        {
            get { return idTypeMapping; }
            set { idTypeMapping = value; }
        }

        public string TypeIdFieldName
        {
            get { return "__TypeId__"; }
        }


        public Type DefaultHashtableClass
        {
            set { defaultHashtableClass = value; }
        }

        public string FromType(Type typeOfObjectToConvert)
        {

            if (typeIdMapping.Contains(typeOfObjectToConvert))
            {
                return typeIdMapping[typeOfObjectToConvert] as string;
            }
            else
            {
                if ( typeof (IDictionary).IsAssignableFrom(typeOfObjectToConvert))
                {
                    return defaultHashtableTypeId;
                }
                return typeOfObjectToConvert.Name;
            }
        }

        public Type ToType(string typeId)
        {
            if (idTypeMapping.Contains(typeId))
            {
                return idTypeMapping[typeId] as Type;
            }
            else
            {
                if (typeId.Equals(defaultHashtableTypeId))
                {
                    return defaultHashtableClass;
                }
                string fullyQualifiedTypeName = defaultNamespace + "." +
                                 typeId + ", " +
                                 DefaultAssemblyName;
                return TypeResolutionUtils.ResolveType(fullyQualifiedTypeName); 
            }

        }

        public string DefaultNamespace
        {
            get
            {
                return defaultNamespace;
            }
            set
            {
                defaultNamespace = value;
            }
            
        }

        public string DefaultAssemblyName
        {
            get
            {
                return defaultAssemblyName;
            }
            set
            {
                defaultAssemblyName = value;
            }
        }

        public void AfterPropertiesSet()
        {
            ValidateIdTypeMapping();
            if (DefaultAssemblyName != null && DefaultNamespace == null)
            {
                throw new ArgumentException("Default Namespace required when DefaultAssemblyName is set.");
            }
            if (DefaultNamespace != null && DefaultAssemblyName == null)
            {
                throw new ArgumentException("Default Assembly Name required when DefaultNamespace is set.");
            }
        }


        private void ValidateIdTypeMapping()
        {
            IDictionary finalIdTypeMapping = new Hashtable();
            foreach (DictionaryEntry entry in idTypeMapping)
            {
                string id = entry.Key.ToString();
                Type t = entry.Value as Type;
                if (t == null)
                {
                    //convert from string value.
                    string typeName = entry.Value.ToString();
                    t = TypeResolutionUtils.ResolveType(typeName);
                }
                finalIdTypeMapping.Add(id,t);
                typeIdMapping.Add(t,id);
                
            }
            idTypeMapping = finalIdTypeMapping;
        }
    }
}