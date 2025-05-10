using ContactService.Contact.API;
using ContactService.Contact.API.Infrastructure;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Repositories.ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Microsoft.EntityFrameworkCore;

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
    services.AddSwaggerGen();

    // Dependency Injection
    services.AddScoped<IContactImplementationService, ContactImplementationService>();
    services.AddScoped<IContactRepository, ContactRepository>();
    services.AddScoped<KafkaProducerService>();
    services.AddHostedService<KafkaConsumerService>();

    // AutoMapper
    services.AddAutoMapper(typeof(Program));
}

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
