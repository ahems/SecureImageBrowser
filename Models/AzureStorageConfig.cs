namespace WebApp_OpenIDConnect_DotNet.Models
{
    public class AzureStorageConfig
    {
        public string AccountName { get; set; }
        public string FullAccountName { get { return string.Format("https://{0}.blob.core.windows.net/", AccountName); } }
        public string Scope { get { return string.Format("{0}user_impersonation", FullAccountName); } }
        public string ImageContainer { get; set; }
    }
}
