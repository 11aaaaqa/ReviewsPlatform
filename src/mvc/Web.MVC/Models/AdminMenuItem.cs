namespace Web.MVC.Models
{
    public class AdminMenuItem
    {
        public string Title { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
