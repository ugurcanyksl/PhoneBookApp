namespace ContactService.Contact.API.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public int TotalContacts { get; set; }
        public int TotalPhoneNumbers { get; set; }
    }
}
