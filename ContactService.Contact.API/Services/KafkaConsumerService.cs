using AutoMapper;
using Confluent.Kafka;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using Newtonsoft.Json;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;

public class KafkaConsumerService : IHostedService
{
    private readonly string _bootstrapServers = "localhost:9092";
    private readonly string _topic = "phonebook-reports";
    private readonly IServiceScopeFactory _scopeFactory;
    private CancellationTokenSource _cts;

    public KafkaConsumerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task.Run(() => StartConsuming(_cts.Token));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel(); // Tüketici düzgün dursun
        return Task.CompletedTask;
    }

    private async Task StartConsuming(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "phonebook-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    Console.WriteLine($"Message received: {consumeResult.Message.Value}");
                    await ProcessMessageAsync(consumeResult.Message.Value);
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Kafka consume error: {ex.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Kafka consumer gracefully shutting down.");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(string message)
    {
        using var scope = _scopeFactory.CreateScope(); // Scoped servisler için scope oluşturuluyor

        var contactRepository = scope.ServiceProvider.GetRequiredService<IContactRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var kafkaProducerService = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

        var contactDto = JsonConvert.DeserializeObject<CreateContactDto>(message);

        if (contactDto != null)
        {
            var person = mapper.Map<Person>(contactDto);
            await contactRepository.AddAsync(person);
            await contactRepository.SaveChangesAsync();

            await kafkaProducerService.SendMessageAsync("phonebook-reports", "Rapor Oluşturuldu");
        }
    }
}
