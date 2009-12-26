namespace Spring.Northwind.Domain
{
    public interface ICustomerClassificationCalculator 
    {
        /// <summary>
        /// Calculates the classification of customer (the value of customer to our company).
        /// </summary>
        /// <param name="customer">
        ///   Customer to base calculation on.
        /// </param>
        /// <returns>Human readable classifcation indicator for given customer.</returns>
        string CalculateClassification(Customer customer);
    }
}