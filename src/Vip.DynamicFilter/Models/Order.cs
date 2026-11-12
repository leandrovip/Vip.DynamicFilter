namespace Vip.DynamicFilter
{
    public class Order
    {
        #region Fields

        private string _direction;
        private OrderDirection _directionType;
        private bool _directionUnknown;

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

        public OrderDirection DirectionType
        {
            get => _directionType;
            set
            {
                _directionType = value;
                _directionUnknown = false;
            }
        }

        public string Direction
        {
            get => _direction;
            set
            {
                _direction = value;

                var known = TryConvertDirection(value, out var directionType);
                DirectionType = directionType;
                _directionUnknown = !known;
            }
        }

        /// <summary>
        ///     Indica que <see cref="Direction" /> recebeu um valor diferente de "asc" e "desc".
        /// </summary>
        internal bool IsDirectionUnknown => _directionUnknown;

        #endregion

        #region Methods

        private static bool TryConvertDirection(string value, out OrderDirection directionType)
        {
            directionType = OrderDirection.Asc;
            if (string.IsNullOrWhiteSpace(value)) return true;

            switch (value.Trim().ToLowerInvariant())
            {
                case "asc":
                    return true;
                case "desc":
                    directionType = OrderDirection.Desc;
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
