using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBookMicroservices.Shared.Models
{
    public class ContactReport
    {
        public Guid Id { get; set; }
        public DateTime RequestedAt { get; set; }
        public ReportStatus Status { get; set; }
        public List<ReportDetail> Details { get; set; }
    }

    public class ReportDetail
    {
        [Key]  // Bu satır birincil anahtar olduğunu belirtir
        public Guid Id { get; set; }
        public string Location { get; set; }
        public int TotalContacts { get; set; }
        public int TotalPhoneNumbers { get; set; }

        public Guid ContactReportId { get; set; }  // Yabancı anahtar
        public ContactReport ContactReport { get; set; }  // İlişkiyi belirtmek
    }

    public enum ReportStatus
    {
        Preparing,
        Completed
    }

}
