

using AllegroSearchService.Common;

namespace KioskBrains.Clients.AllegroPl.Models
{
    public class Offer
    {
        public string Id { get; set; }

        public string CategoryId { get; set; }

        public MultiLanguageString Name { get; set; }

        public OfferStateEnum State { get; set; }

        public decimal Price { get; set; }

        public string PriceCurrencyCode { get; set; }

        public MultiLanguageString Description { get; set; }

        public OfferImage[] Images { get; set; }

        public DeliveryOption[] DeliveryOptions { get; set; }

        public OfferExtraData ExtraData { get; set; }
    }    
}