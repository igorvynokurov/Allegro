using KioskBrains.Clients.AllegroPl.Models;

namespace KioskBrains.Clients.AllegroPl.Rest.Models
{
    internal class Offer
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public OfferImage[] Images { get; set; }

        public OfferSellingMode SellingMode { get; set; }

        public OfferCategory Category { get; set; }

        public OfferDelivery Delivery { get; set; }
    }
}