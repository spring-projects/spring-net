using System;
using System.Collections;

/// <summary>
/// EmployeeInfo domain class
/// </summary>
public class EmployeeInfo
{
    private int id;
    private string firstName;
    private string lastName;
    private DateTime dateOfBirth;
    private Gender gender = Gender.Male;
    private AddressInfo mailingAddress = new AddressInfo();
    private double salary;
    private ArrayList _hobbies = new ArrayList();
    private ArrayList _favoriteFood = new ArrayList();

    public EmployeeInfo()
    {}

    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public string FirstName
    {
        get { return firstName; }
        set { firstName = value; }
    }

    public string LastName
    {
        get { return lastName; }
        set { lastName = value; }
    }

    public DateTime DateOfBirth
    {
        get { return dateOfBirth; }
        set { dateOfBirth = value; }
    }

    public Gender Gender
    {
        get { return gender; }
        set { gender = value; }
    }

    public AddressInfo MailingAddress
    {
        get { return mailingAddress; }
        set { mailingAddress = value; }
    }

    public double Salary
    {
        get { return salary; }
        set { salary = value; }
    }

    public IList Hobbies
    {
        get { return this._hobbies; }
        set { this._hobbies = (value != null) ? new ArrayList(value) : null; }
    }

    public IList FavoriteFood
    {
        get { return this._favoriteFood; }
        set { this._favoriteFood = (value != null) ? new ArrayList(value) : null; }
    }
}

public enum Gender
{
    Male,
    Female
}

public class AddressInfo
{
    private AddressType addressType;
    private string street1;
    private string street2;
    private string city;
    private string state;
    private string postalCode;
    private string country;

    public AddressType AddressType
    {
        get { return addressType; }
        set { addressType = value; }
    }

    public string Street1
    {
        get { return street1; }
        set { street1 = value; }
    }

    public string Street2
    {
        get { return street2; }
        set { street2 = value; }
    }

    public string City
    {
        get { return city; }
        set { city = value; }
    }

    public string State
    {
        get { return state; }
        set { state = value; }
    }

    public string PostalCode
    {
        get { return postalCode; }
        set { postalCode = value; }
    }

    public string Country
    {
        get { return country; }
        set { country = value; }
    }
}

public enum AddressType
{
    Home,
    Office
}

public class Hobby
{
    private string _uniqueKey;
    private string _title;

    public Hobby(string uniqueKey, string title)
    {
        this._uniqueKey = uniqueKey;
        this._title = title;
    }

    public string UniqueKey
    {
        get { return this._uniqueKey; }
    }

    public string Title
    {
        get { return this._title; }
    }

    public override string ToString()
    {
        return this._uniqueKey;
    }
}