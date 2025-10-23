using MassTransit;

namespace RabbitMQContract.Consumer
{
    public record TestMessage
    {
        public string Text { get; init; }
    }

    public class TestMessageConsumer : IConsumer<TestMessage>
    {
        public Task Consume(ConsumeContext<TestMessage> context)
        {
            Console.WriteLine($"Received: {context.Message.Text}");
            return Task.CompletedTask;
        }
    }
}
