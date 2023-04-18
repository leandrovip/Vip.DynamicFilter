namespace Vip.DynamicFilter.Tests.Models;

public class Address
{
    #region Properties

    public Guid AddressId { get; set; }
    public Guid ClientId { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public int ZipCode { get; set; }

    #endregion

    #region Constructor

    public Address(Guid clientId, string street, string city, int zipCode)
    {
        AddressId = Guid.NewGuid();
        ClientId = clientId;
        Street = street;
        City = city;
        ZipCode = zipCode;
    }

    #endregion
}