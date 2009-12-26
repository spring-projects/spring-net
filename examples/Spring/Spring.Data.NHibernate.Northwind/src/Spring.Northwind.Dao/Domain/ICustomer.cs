using System.Collections.ObjectModel;

namespace Spring.Northwind.Domain
{
    /// <summary>
    /// Interface for customer entity data.
    /// </summary>
    public interface ICustomer
    {
        string Id { get; }
        string CompanyName { get; set; }
        string ContactName { get; set; }
        string ContactTitle { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string Region { get; set; }
        string PostalCode { get; set; }
        string Country { get; set; }
        string Phone { get; set; }
        string Fax { get; set; }
        ReadOnlyCollection<Order> Orders { get; }
        string Classification { get; }
    }
}