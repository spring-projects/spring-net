namespace Spring.Objects

{
	/// <summary>
	/// Test class used for Validator tests.
	/// </summary>
	/// <author>Goran Milosavljevic</author>
	public class Contact
	{
		private String m_name;
		private int m_age;
		private int m_loan;
		private string m_creditcard;

		public Contact()
		{
		}

		public Contact(string name, int age, int loan)
		{
			this.m_name = name;
			this.m_age = age;
			this.m_loan = loan;
		}

		public string Creditcard
		{
			get { return m_creditcard; }
			set { m_creditcard = value; }
		}

		public int Age
		{
			get { return m_age; }
			set { m_age = value; }
		}

		public int Loan
		{
			get { return m_loan; }
			set { m_loan = value; }
		}

		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
	}
}