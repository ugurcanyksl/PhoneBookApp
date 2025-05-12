using Microsoft.EntityFrameworkCore;
using ReportService.Report.API.Infrastructure;
using PhoneBookMicroservices.Shared.Models;
using System.Text.Json;

namespace ReportService.Report.API.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReportRepository> _logger;

        public ReportRepository(ReportDbContext dbContext, IHttpClientFactory httpClientFactory, ILogger<ReportRepository> logger)
        {
            _dbContext = dbContext;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5000/");
            _logger = logger;
        }

        public async Task<ContactReport> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Fetching report with ID {Id}", id);
            var report = await _dbContext.Reports.Include(r => r.Details).FirstOrDefaultAsync(r => r.Id == id);
            if (report == null)
            {
                _logger.LogWarning("Report with ID {Id} not found", id);
            }
            return report;
        }

        public async Task AddAsync(ContactReport report)
        {
            _logger.LogInformation("Adding new report with ID {Id}", report.Id);
            await _dbContext.Reports.AddAsync(report);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Report with ID {Id} added successfully", report.Id);
        }

        public async Task<IEnumerable<Person>> GetContactDataByLocationAsync(string location)
        {
            _logger.LogInformation("Fetching contacts for location {Location}", location);
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/Contact/location/{location}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var persons = JsonSerializer.Deserialize<IEnumerable<Person>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _logger.LogInformation("Successfully fetched {Count} contacts for location {Location}", persons.Count(), location);
                return persons;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch contacts for location {Location} from ContactService", location);
                throw new Exception($"Failed to fetch contacts for location {location}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize contacts for location {Location}", location);
                throw new Exception($"Failed to deserialize contacts for location {location}", ex);
            }
        }

        public async Task<List<ContactReport>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all reports");
            var reports = await _dbContext.Reports.Include(r => r.Details).ToListAsync();
            _logger.LogInformation("Fetched {Count} reports", reports.Count);
            return reports;
        }

        public async Task<List<ContactReport>> GetAllPagedAsync(int page, int pageSize)
        {
            _logger.LogInformation("Fetching reports - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var reports = await _dbContext.Reports.Include(r => r.Details)
                                                  .Skip((page - 1) * pageSize)
                                                  .Take(pageSize)
                                                  .ToListAsync();
            _logger.LogInformation("Fetched {Count} reports", reports.Count);
            return reports;
        }
    }
}
