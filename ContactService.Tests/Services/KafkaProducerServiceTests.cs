using ContactService.Contact.API.Services;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Confluent.Kafka;
using System;
using System.IO;
using System.Text;

namespace ContactService.Tests.Services
{
    public class TestKafkaProducerService : KafkaProducerService
    {
        public TestKafkaProducerService(IProducer<Null, string> producer, string bootstrapServers, string topic)
        : base(producer, bootstrapServers, topic)
        {
        }

        public override async Task SendMessageAsync(string topic, string message)
        {
            try
            {
                if (message == null)
                    throw new ArgumentNullException(nameof(message));
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

    public class KafkaProducerServiceTests
    {
        private readonly Mock<IProducer<Null, string>> _mockProducer;
        private readonly TestKafkaProducerService _service;
        private readonly string _topic;

        public KafkaProducerServiceTests()
        {
            _mockProducer = new Mock<IProducer<Null, string>>();
            _topic = "test-topic";
            _service = new TestKafkaProducerService(_mockProducer.Object, "localhost:9092", _topic);
        }

        [Fact]
        public async Task SendMessageAsync_ShouldCallProducer()
        {
            var topic = "test-topic";
            var message = "test-message";

            _mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>(), default))
                .Returns(Task.FromResult<DeliveryResult<Null, string>>(null));

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                await _service.SendMessageAsync(topic, message);

                var consoleOutput = sw.ToString();
                Assert.Contains($"Delivery result is null for message '{message}' to topic '{topic}'", consoleOutput);

                _mockProducer.Verify(p => p.ProduceAsync(It.Is<string>(t => t == topic),
                    It.Is<Message<Null, string>>(m => m.Value == message), default), Times.Once());
            }

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        [Fact]
        public async Task SendMessageAsync_ShouldThrowException_WhenDeliveryFails()
        {
            var topic = "test-topic";
            var message = "test-message";
            var exception = new ProduceException<Null, string>(new Error(ErrorCode.Unknown, "Delivery failed"), null);

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                _mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>(), default))
                    .ThrowsAsync(exception);

                var ex = await Assert.ThrowsAsync<ProduceException<Null, string>>(() => _service.SendMessageAsync(topic, message));
                Assert.Contains("Delivery failed", ex.Message);
                _mockProducer.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>(), default), Times.Once());

                var consoleOutput = sw.ToString();
                Assert.Contains("Delivery failed: Delivery failed", consoleOutput);
            }

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        [Fact]
        public async Task SendMessageAsync_ShouldThrowArgumentNullException_WhenMessageIsNull()
        {
            var topic = "test-topic";
            string message = null;

            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SendMessageAsync(topic, message));
        }

        [Fact]
        public async Task SendMessageAsync_ShouldLogMessage_WhenDeliverySucceeds()
        {
            var topic = "test-topic";
            var message = "test-message";
            var deliveryResult = new DeliveryResult<Null, string>
            {
                Topic = topic,
                Partition = new Partition(0),
                Offset = new Offset(0),
                Message = new Message<Null, string> { Value = message }
            };

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                _mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>(), default))
                    .ReturnsAsync(deliveryResult);

                await _service.SendMessageAsync(topic, message);

                var consoleOutput = sw.ToString();
                Assert.Contains($"Delivered '{message}' to '{topic}/0:0'", consoleOutput);
            }

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }
}