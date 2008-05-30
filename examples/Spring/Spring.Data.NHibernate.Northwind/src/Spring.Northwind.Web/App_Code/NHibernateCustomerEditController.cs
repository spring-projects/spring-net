
using System.Web;
using NHibernate;
using Spring.Data.NHibernate;
using Spring.Northwind.Domain;

/// <summary>
/// </summary>
/// <author>erich.eichinger</author>
/// <version>$Id: NHibernateCustomerEditController.cs,v 1.1 2007/09/29 21:32:09 oakinger Exp $</version>
public class NHibernateCustomerEditController:ICustomerEditController
{
  private readonly ISessionFactory sessionFactory;
  private Customer currentCustomer;

  public NHibernateCustomerEditController(ISessionFactory sessionFactory)
  {
    this.sessionFactory = sessionFactory;
  }

  private ISession Session
  {
    get
    {
        return SessionFactoryUtils.GetSession( sessionFactory,false );
    }
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
      Session.Lock(customer, LockMode.None);
      return customer;
    }
  }
}
