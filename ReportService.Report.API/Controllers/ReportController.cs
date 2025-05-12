using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Report.API.Dtos;
using ReportService.Report.API.Infrastructure.Kafka;
using ReportService.Report.API.Services;


namespace ReportService.Report.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportImplementationService _reportService;
        private readonly IReportProducer _reportProducer;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportImplementationService reportService, IReportProducer reportProducer, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _reportProducer = reportProducer;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportRequestDto dto)
        {
            _logger.LogInformation("Received report request for location {Location}", dto.Location);
            await _reportProducer.SendReportRequestAsync(dto);
            _logger.LogInformation("Report request queued for location {Location}", dto.Location);
            return Accepted(new { Message = "Report request queued", Location = dto.Location });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Fetching report with ID {Id}", id);
            var report = await _reportService.GetByIdAsync(id);
            if (report == null)
            {
                _logger.LogWarning("Report with ID {Id} not found", id);
                return NotFound();
            }
            return Ok(report);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReports(int page = 1, int pageSize = 10)
        {
            _logger.LogInformation("Fetching reports - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var reports = await _reportService.GetAllPagedAsync(page, pageSize);
            _logger.LogInformation("Fetched {Count} reports", reports.Count);
            return Ok(new { Data = reports, Page = page, PageSize = pageSize });
        }
    }
}
