using Confluent.Kafka;

namespace ContactService.Contact.API.Services
{
    public class KafkaProducerService
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "phonebook-reports";

        public async Task SendMessageAsync(string topic, string message)
        {
            var config = new ProducerConfig { BootstrapServers = _bootstrapServers };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var deliveryResult = await producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
                    Console.WriteLine($"Message delivered to {deliveryResult.TopicPartitionOffset}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occurred: {e.Message}");
                }
            }
        }
    }
}