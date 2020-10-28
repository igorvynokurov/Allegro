namespace KioskBrains.Clients.AllegroPl.Rest.Models
{
    internal class OfferSellingMode
    {
        public OfferPrice Price { get; set; }

        public OfferPrice FixedPrice { get; set; }

        public OfferPrice GetMaxPrice()
        {
            if (FixedPrice == null)
            {
                return Price;
            }

            if (Price == null)
            {
                return FixedPrice;
            }

            return FixedPrice.Amount > Price.Amount
                ? FixedPrice
                : Price;
        }
    }
}