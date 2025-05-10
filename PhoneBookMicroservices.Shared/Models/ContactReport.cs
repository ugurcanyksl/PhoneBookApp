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
        [Key]
        public Guid Id { get; set; }
        public string Location { get; set; }
        public int TotalContacts { get; set; }
        public int TotalPhoneNumbers { get; set; }

        public Guid ContactReportId { get; set; }
        public ContactReport ContactReport { get; set; }
    }

    public enum ReportStatus
    {
        Preparing,
        Completed
    }

}
