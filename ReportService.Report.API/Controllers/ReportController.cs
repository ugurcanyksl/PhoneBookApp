using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Report.API.Dtos;
using ReportService.Report.API.Services;


namespace ReportService.Report.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportImplementationService _reportService;

        public ReportController(IReportImplementationService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportRequestDto dto)
        {
            var report = await _reportService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var report = await _reportService.GetByIdAsync(id);
            if (report == null) return NotFound();
            return Ok(report);
        }
    }

}
