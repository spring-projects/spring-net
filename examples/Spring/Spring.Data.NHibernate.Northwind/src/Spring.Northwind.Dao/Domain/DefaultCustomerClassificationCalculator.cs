using System;

namespace Spring.Northwind.Domain
{
    /// <summary>
    /// Basic (dummy) implementation of customer classification calculation.
    /// </summary>
    /// <remarks>
    /// In real life we would need to check orders, their total and the actual
    /// reveneu that this customer brings us..
    /// </remarks>
    public class DefaultCustomerClassificationCalculator : ICustomerClassificationCalculator
    {

        public string CalculateClassification(Customer customer)
        {
            if (customer == null)
            {
                return null;
            }

            // we calculate based on customer's A alphabet cout (of course)
            int aCount = 0;
            foreach (char c in customer.CompanyName)
            {
                if (Char.ToLowerInvariant(c) == 'a')
                {
                    aCount++;
                }
            }

            if (aCount > 4)
            {
                // great customer!
                return "A";
            }
            if (aCount > 1)
            {
                // ok..
                return "B";
            }

            // a really lousy customer...
            return "C";
        }
    }
}