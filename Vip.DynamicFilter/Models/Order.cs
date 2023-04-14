namespace Vip.DynamicFilter
{
    public class Order
    {
        #region Constructors

        public Order()
        {
            Direction = OrderDirection.Asc;
        }

        #endregion

        #region Properties

        public string Column { get; set; }
        public OrderDirection Direction { get; set; }

        #endregion
    }
}