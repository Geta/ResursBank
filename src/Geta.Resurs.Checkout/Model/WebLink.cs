namespace Geta.Resurs.Checkout.Model
{
    public class WebLink
    {
        public WebLink()
        {
        }

        public WebLink(bool appendPriceLast, string endUserDescription, string url)
        {
            AppendPriceLast = appendPriceLast.ToString();
            EndUserDescription = endUserDescription;
            Url = url;
        }

        public string AppendPriceLast { get; set; }
        public string EndUserDescription { get; set; }
        public string Url { get; set; }
    }
}
