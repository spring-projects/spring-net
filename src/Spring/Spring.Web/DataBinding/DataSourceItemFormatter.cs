using System.Collections;
using System.Web.UI;

namespace Spring.DataBinding
{
    /// <summary>
    /// This formatter acts as an adapter to a datasource. It parses a given datasource into
    /// a table to allow converting back and forth between objects and their keys during formatting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DataSourceItemFormatter expects the "source" to have 2 properties named "DataSourceField" and "DataValueField".
    /// </para>
    /// </remarks>
    public class DataSourceItemFormatter : IBindingAwareFormatter
    {
        private string _dataSourceFieldName;
        private string _dataValueFieldName;

        [ThreadStatic]
        private Hashtable _dataItemsByKey;
        [ThreadStatic]
        private string _dataItemKeyField;

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        /// <param name="dataSourceField">The name of the "source"s property containing the dataSource</param>
        /// <param name="dataValueField">The name of the "source"s property containing the name of the key property</param>
        public DataSourceItemFormatter(string dataSourceField, string dataValueField)
        {
            this._dataSourceFieldName = dataSourceField;
            if(dataValueField == null)
            {
                throw new ArgumentNullException("DataValueField must not be null.");
            }
            this._dataValueFieldName = dataValueField;
        }

        /// <summary>
        /// Sets the bindingContext for the current thread.
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="target">The target object</param>
        /// <param name="variables">Variables to be used during binding</param>
        /// <param name="direction">The current binding's direction</param>
        public void SetBindingContext(object source, object target, IDictionary<string, object> variables, BindingDirection direction)
        {
            // Extract dataSource from source object
            IEnumerable dataSource = DataBinder.GetPropertyValue(source, this._dataSourceFieldName) as IEnumerable;
            // Extract dataItemKeyField from source object
            string dataItemKeyField = DataBinder.GetPropertyValue(source, this._dataValueFieldName) as String;

            Initialize(dataSource, dataItemKeyField, direction);
        }

        /// <summary>
        /// Reset the current thread's binding context.
        /// </summary>
        public void ClearBindingContext()
        {
            _dataItemsByKey = null;
            _dataItemKeyField = null;
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="dataSource">The datasource containing list items</param>
        /// <param name="dataItemKeyField">The name of the listitem's property that evaluates to the item's key</param>
        /// <param name="direction">The direction of the current binding</param>
        private void Initialize(IEnumerable dataSource, string dataItemKeyField, BindingDirection direction)
        {
            _dataItemKeyField = dataItemKeyField;

            // if we are binding from target to source only, we can save some performance
            // here because only Format() will be called.
            if (direction != (direction&BindingDirection.TargetToSource))
            {
                if(dataSource == null)
                {
                    throw new ArgumentNullException(
                        string.Format("DataSource must not be null."));
                }

                _dataItemsByKey = new Hashtable();
                foreach(object dataItem in dataSource)
                {
                    string key = Format(dataItem);
                    this._dataItemsByKey.Add(key, dataItem);
                }
            }
        }

        /// <summary>
        /// Returns the key of the specified value object.
        /// </summary>
        /// <param name="value">The value to extract the key from.</param>
        /// <returns>Key extracted from <paramref name="value"/>.</returns>
        public string Format(object value)
        {
            return DataBinder.GetPropertyValue(value, this._dataItemKeyField, null);
        }

        /// <summary>
        /// Return the object with the specified key from the datasource.
        /// </summary>
        /// <param name="key">The key of the object.</param>
        /// <returns>Parsed <paramref name="key"/>.</returns>
        public object Parse(string key)
        {
            object item = this._dataItemsByKey[key];
            if (item==null)
            {
                throw new ArgumentOutOfRangeException("key", key, string.Format("Item with key {0} does not exist", key));
            }
            return item;
        }
    }
}
