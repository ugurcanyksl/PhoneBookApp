using Microsoft.EntityFrameworkCore;
using ReportService.Report.API.Infrastructure;
using ReportService.Report.API.Infrastructure.AutoMapper;
using ReportService.Report.API.Infrastructure.Configuration;
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

// --- Service Registration ---
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // DB
    services.AddDbContext<ReportDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("ReportDbConnection")));

    // Controllers
    services.AddControllers();

    // Kafka config
    services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

    // Kafka producer
    services.AddSingleton<IReportProducer, ReportProducer>();

    // Kafka consumer and hosted service
    services.AddScoped<ReportConsumer>(); 
    services.AddSingleton<IHostedService, ReportConsumerHostedService>(); // Singleton hosted service

    // AutoMapper
    services.AddAutoMapper(typeof(ConfigureAutoMapper).Assembly);

    // Swagger
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    // App Services
    services.AddScoped<IReportImplementationService, ReportImplementationService>();
    services.AddScoped<IReportRepository, ReportRepository>();
}

// --- Middleware Configuration ---
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
