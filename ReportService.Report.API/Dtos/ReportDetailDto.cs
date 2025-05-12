namespace ReportService.Report.API.Dtos
{
    public class ReportDetailDto
    {
        public Guid Id { get; set; }
        public string Location { get; set; }
        public int TotalContacts { get; set; }
        public int TotalPhoneNumbers { get; set; }
        public Guid ContactReportId { get; set; }
    }
}
