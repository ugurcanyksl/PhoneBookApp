using Confluent.Kafka;

namespace ContactService.Contact.API.Services
{
    public class KafkaProducerService : IKafkaProducerService
    {
        protected readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaProducerService(IProducer<Null, string> producer, string bootstrapServers, string topic)
        {
            _producer = producer ?? new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = bootstrapServers }).Build();
            _topic = topic;
        }

        public virtual async Task SendMessageAsync(string topic, string message)
        {
            try
            {
                var deliveryResult = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
                if (deliveryResult != null)
                {
                    Console.WriteLine($"Delivered '{deliveryResult.Value}' to '{deliveryResult.Topic}/{deliveryResult.Partition.Value}:{deliveryResult.Offset.Value}'");
                }
                else
                {
                    Console.WriteLine($"Delivery result is null for message '{message}' to topic '{topic}'");
                }
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                throw;
            }
        }
    }
}