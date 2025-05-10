namespace ReportService.Report.API.Dtos
{
    public class ReportDto
    {
        public Guid Id { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
        public List<ReportDetailDto> Details { get; set; }
    }
}
