namespace Vip.DynamicFilter
{
    public class Order
    {
        #region Fields

        private string _direction;

        #endregion

        #region Constructors

        public Order()
        {
            DirectionType = OrderDirection.Asc;
        }

        public Order(string column, OrderDirection direction)
        {
            Column = column;
            DirectionType = direction;
        }

        #endregion

        #region Properties

        public string Column { get; set; }
        public OrderDirection DirectionType { get; set; }

        public string Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                DirectionType = ConvertDirection(_direction);
            }
        }

        #endregion

        #region Methods

        private OrderDirection ConvertDirection(string value)
        {
            if (string.IsNullOrEmpty(value)) return OrderDirection.Asc;
            value = value.Trim().ToLower();

            return value switch
            {
                "asc" => OrderDirection.Asc,
                "desc" => OrderDirection.Desc,
                _ => OrderDirection.Asc
            };
        }

        #endregion
    }
}