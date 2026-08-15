namespace Vip.DynamicFilter.DemoApi.Models;

public class Address
{
    #region Properties

    public Guid AddressId { get; set; }
    public Guid ClientId { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public int ZipCode { get; set; }

    #endregion
}
