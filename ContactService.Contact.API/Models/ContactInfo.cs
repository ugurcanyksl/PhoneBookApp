namespace ContactService.Contact.API.Models
{
    public class ContactInfo
    {
        public Guid Id { get; set; }
        public string InfoType { get; set; }  // Telefon Numarası, E-mail, Konum vb.
        public string InfoContent { get; set; }
        public Guid ContactId { get; set; }
        public Contact Contact { get; set; }
        public string Location { get; set; } // Konum bilgisi ekleniyor
    }
}
