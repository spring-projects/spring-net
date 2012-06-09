using NHibernate;
using Spring.Data.NHibernate;
using Spring.Northwind.Domain;

/// <summary>
/// </summary>
/// <author>erich.eichinger</author>
public class NHibernateCustomerEditController : ICustomerEditController
{
    private readonly ISessionFactory sessionFactory;
    private Customer currentCustomer;

    public NHibernateCustomerEditController(ISessionFactory sessionFactory)
    {
        this.sessionFactory = sessionFactory;
    }

    private ISession Session
    {
        get { return sessionFactory.GetCurrentSession(); }
    }

    public void EditCustomer(Customer customer)
    {
        currentCustomer = customer;
    }

    public void Clear()
    {
        currentCustomer = null;
    }

    public Customer CurrentCustomer
    {
        get
        {
            Customer customer = currentCustomer;
			
			//since the Customer entity may have been retrieved from a prior request, we need to reattach it to the current session
			// in order to support lazy-loading, etc. on the Customer
            Session.Lock(customer, LockMode.None);
            
			return customer;
        }
    }
}
