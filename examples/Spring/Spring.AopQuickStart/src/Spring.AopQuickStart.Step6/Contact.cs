namespace Spring.AopQuickStart
{
	public class Contact
	{
        private string firstName;
        public virtual string FirstName
		{
            get { return firstName; }
		    set { firstName = value; }
		}

        private string lastName;
        public virtual string LastName
	    {
	        get { return lastName; }
	        set { lastName = value; }
	    }

        private string emailAddress;
        public virtual string EmailAddress
	    {
	        get { return emailAddress; }
	        set { emailAddress = value; }
	    }
	}
}
