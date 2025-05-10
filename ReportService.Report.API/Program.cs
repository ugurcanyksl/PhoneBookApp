using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ReportService.Report.API.Infrastructure;
using ReportService.Report.API.Infrastructure.AutoMapper;
using ReportService.Report.API.Infrastructure.Kafka;
using ReportService.Report.API.Repositories;
using ReportService.Report.API.Services;

var builder = WebApplication.CreateBuilder(args);

//Report DB
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReportDbConnection")));


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(ConfigureAutoMapper).Assembly);

// Diðer servisler
builder.Services.AddScoped<ReportConsumer>();
builder.Services.AddScoped<IHostedService, ReportConsumerHostedService>();

// Baðýmlýlýk injeksiyonu
builder.Services.AddScoped<IReportImplementationService, ReportImplementationService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
