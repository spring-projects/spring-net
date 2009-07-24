using Spring.Northwind.Domain;

public interface ICustomerEditController
{
    void EditCustomer(Customer customer);
    void Clear();
    Customer CurrentCustomer { get; }
}
