using AutoMapper;
using Confluent.Kafka;
using ContactService.Contact.API.Repositories;
using ContactService.Contact.API.Services;
using PhoneBookMicroservices.Shared.DTOs;
using PhoneBookMicroservices.Shared.Models;

public class KafkaConsumerService : IHostedService
{
    private readonly string _bootstrapServers = "localhost:9092";
    private readonly string _topic = "phonebook-reports";
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;
    private readonly KafkaProducerService _kafkaProducerService;

    public KafkaConsumerService(
        IContactRepository contactRepository,
        IMapper mapper,
        KafkaProducerService kafkaProducerService)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
        _kafkaProducerService = kafkaProducerService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => StartConsuming(cancellationToken)); // Kafka consumer'ı başlatıyoruz
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        
        return Task.CompletedTask;
    }

    // Kafka mesajları dinleyecek metod
    public async Task StartConsuming(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "phonebook-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
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
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }

    private async Task ProcessMessageAsync(string message)
    {
        var contactDto = Newtonsoft.Json.JsonConvert.DeserializeObject<CreateContactDto>(message);

        if (contactDto != null)
        {
            var person = _mapper.Map<Person>(contactDto);
            await _contactRepository.AddAsync(person);
            await _contactRepository.SaveChangesAsync();
            await _kafkaProducerService.SendMessageAsync("phonebook-reports", "Rapor Oluşturuldu");
        }
    }
}
