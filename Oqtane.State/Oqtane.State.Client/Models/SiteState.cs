namespace Oqtane.State.Client.Models
{
    // this class is used for sharing state between components and services on the client
    public class SiteState
    {
        public string RemoteIPAddress { get; set; }
        public bool IsPrerendering { get; set; }

        public void Hydrate(SiteState siteState)
        {
            RemoteIPAddress = siteState.RemoteIPAddress;
            IsPrerendering = siteState.IsPrerendering;
        }
    }
}
