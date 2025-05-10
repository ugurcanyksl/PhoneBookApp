using Microsoft.EntityFrameworkCore;
using ReportService.Report.API.Infrastructure;
using ReportService.Report.API.Infrastructure.AutoMapper;
using ReportService.Report.API.Infrastructure.Kafka;
using ReportService.Report.API.Repositories;
using ReportService.Report.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
ConfigureMiddleware(app);

app.Run();

// Method to configure services
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Report DB
    services.AddDbContext<ReportDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("ReportDbConnection")));

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddAutoMapper(typeof(ConfigureAutoMapper).Assembly);

    // Other services
    services.AddScoped<ReportConsumer>();
    services.AddScoped<IHostedService, ReportConsumerHostedService>();

    // Dependency injection
    services.AddScoped<IReportImplementationService, ReportImplementationService>();
    services.AddScoped<IReportRepository, ReportRepository>();
}

// Method to configure middleware
void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}
