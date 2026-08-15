namespace Vip.DynamicFilter.DemoApi.Models;

public class Client
{
    #region Properties

    public Guid ClientId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime Birthday { get; set; }
    public ClientEnum ClientType { get; set; }
    public ICollection<Address> Address { get; set; }

    #endregion
}
