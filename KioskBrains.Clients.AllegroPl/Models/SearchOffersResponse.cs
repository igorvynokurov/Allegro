namespace KioskBrains.Clients.AllegroPl.Models
{
    public class SearchOffersResponse
    {
        public Offer[] Offers { get; set; }

        /// <summary>
        /// Use it instead of phrase for next-page requests.
        /// </summary>
        public string TranslatedPhrase { get; set; }

        public int Total { get; set; }
        public string ErrorMessage { get; set; }
    }
}