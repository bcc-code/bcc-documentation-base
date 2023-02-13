namespace BccCode.DocumentationSite.Models
{
    public class Members
    {
        public string? login { get; set; }
        public int id { get; set; }
        public string? node_id { get; set; }
        public string? type { get; set; }

        //Format : {"login":"","id":,"node_id":"","avatar_url":"","gravatar_id":"","url":"","html_url":"","followers_url":"","following_url":"","gists_url":"","starred_url":"","subscriptions_url":"","organizations_url":"","repos_url":"","events_url":"","received_events_url":"","type":"User","site_admin":false}

    }
}
