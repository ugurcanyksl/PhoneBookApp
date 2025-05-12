namespace ContactService.Contact.API.Services
{
    public interface IKafkaProducerService
    {
        Task SendMessageAsync(string topic, string message);
    }
}
