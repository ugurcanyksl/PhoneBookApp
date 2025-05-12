using ContactService.Contact.API.Infrastructure;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.OpenApi.Models;
using Confluent.Kafka;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

namespace ContactService.Tests
{
    public class ProgramTests
    {
        private readonly IConfiguration _configuration;

        public ProgramTests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "Kafka:BootstrapServers", "localhost:9092" },
                { "Kafka:Topic", "contact-topic" },
                { "ConnectionStrings:ContactDbConnection", "Host=localhost;Database=TestDb;Username=postgres;Password=password" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public void ConfigureServices_ShouldRegisterServices()
        {
            var services = new ServiceCollection();
            var builder = WebApplication.CreateBuilder(new string[] { });

            ConfigureServices(services, _configuration);

            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IContactRepository>());
            Assert.NotNull(serviceProvider.GetService<IKafkaProducerService>());
            Assert.NotNull(serviceProvider.GetService<IContactImplementationService>());
            Assert.NotNull(serviceProvider.GetService<ContactDbContext>());
        }

        [Fact]
        public void ConfigureServices_ShouldRegisterKafkaConsumerService()
        {
            var services = new ServiceCollection();

            ConfigureServices(services, _configuration);
            var serviceProvider = services.BuildServiceProvider();

            Assert.NotNull(serviceProvider.GetService<IHostedService>());
        }

        [Fact]
        public void ConfigureMiddleware_ShouldSetupMiddleware()
        {
            var builder = WebApplication.CreateBuilder(new string[] { });
            builder.Environment.EnvironmentName = "Development";
            ConfigureServices(builder.Services, _configuration);
            var app = builder.Build();

            ConfigureMiddleware(app);

            Assert.NotNull(app);
        }

        [Fact]
        public void ConfigureServices_ShouldRegisterSwagger()
        {
            var services = new ServiceCollection();
            var webHostEnvironment = new Mock<IWebHostEnvironment>();
            webHostEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            webHostEnvironment.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            webHostEnvironment.Setup(e => e.ApplicationName).Returns(typeof(ProgramTests).Assembly.GetName().Name);
            services.AddSingleton<IWebHostEnvironment>(webHostEnvironment.Object);

            var hostEnvironment = new Mock<IHostEnvironment>();
            hostEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
            hostEnvironment.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            services.AddSingleton<IHostEnvironment>(hostEnvironment.Object);

            services.Configure<MvcOptions>(options =>
            {
                var manager = services.BuildServiceProvider().GetRequiredService<ApplicationPartManager>();
                manager.ApplicationParts.Clear();
                manager.ApplicationParts.Add(new AssemblyPart(typeof(ProgramTests).Assembly));
            });

            ConfigureServices(services, _configuration);
            var serviceProvider = services.BuildServiceProvider();
            var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

            Assert.NotNull(swaggerProvider);
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ContactDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

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

            services.AddLogging();

            services.AddAuthorization();

            services.AddScoped<IContactImplementationService, ContactImplementationService>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<KafkaProducerService>();

            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            var topic = configuration["Kafka:Topic"];
            services.AddSingleton<IKafkaProducerService>(sp =>
            {
                var config = new ProducerConfig { BootstrapServers = bootstrapServers ?? "localhost:9092" };
                var producer = new ProducerBuilder<Null, string>(config).Build();
                return new KafkaProducerService(producer, bootstrapServers ?? "localhost:9092", topic ?? "contact-topic");
            });

            services.AddSingleton<IHostedService, KafkaConsumerService>();
            services.AddAutoMapper(typeof(ReportService.Report.API.Infrastructure.AutoMapper.ConfigureAutoMapper));
        }

        private void ConfigureMiddleware(WebApplication app)
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
    }
}