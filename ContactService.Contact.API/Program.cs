using ContactService.Contact.API;
using ContactService.Contact.API.Infrastructure;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using ContactService.Contact.API.Repositories.ContactService.Contact.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Services configuration
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Middleware configuration
ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // PostgreSQL DbContext
    services.AddDbContext<ContactDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("ContactDbConnection")));

    // Controllers & Swagger
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "PhoneBook Microservices - Contact API",
            Version = "v1"
        });
    });

    // Dependency Injection
    services.AddScoped<IContactImplementationService, ContactImplementationService>();
    services.AddScoped<IContactRepository, ContactRepository>();
    services.AddScoped<KafkaProducerService>();

    // Kafka Consumer background service (uses IServiceScopeFactory inside)
    services.AddSingleton<IHostedService, KafkaConsumerService>();

    // AutoMapper
    services.AddAutoMapper(typeof(Program));
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhoneBook Microservices - Contact API v1");
        });
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}

// Global Exception Handling Middleware
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
    }
}
