using Amazon.SQS;
using QueuePublisher.SQS;
using QueuePublisher.RabbitMQ;
using RabbitMQ.Client;

namespace QueuePublisher
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("===== PRUEBA AWS SQS =====");
            await TestSqs();

            Console.WriteLine();
            Console.WriteLine("===== PRUEBA RABBITMQ =====");
            await TestRabbitMq();
        }

        private static async Task TestSqs()
        {
            AmazonSQSClient sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.USEast1);
            string queueName = "notification-queue-dev";

            // Usar la librería AwsSqsService
            AwsSqsService sqsService = new AwsSqsService(sqsClient, queueName);

            // Enviar mensaje
            Console.WriteLine("📤 Enviando mensaje a SQS...");
            await sqsService.SendMessageAsync("Hola desde SQS 🚀");

            // Recibir mensajes
            Console.WriteLine("📥 Recibiendo mensajes de SQS...");
            await sqsService.ReceiveMessagesAsync(async message =>
            {
                Console.WriteLine($"➡️ SQS: {message}");
                await Task.CompletedTask;
            });
        }

        private static async Task TestRabbitMq()
        {
            var factory = new ConnectionFactory
            {
                HostName = "leopard-01.lmq.cloudamqp.com",
                Port = 5672,
                UserName = "yunpypuq",
                Password = "dz0qjGV4FrhR9jLRKAR598a3d2xUrh41",
                VirtualHost = "yunpypuq"
            };

            using var connection = await factory.CreateConnectionAsync();

            string queueName = "mi-cola-rabbit";

            // Usar la librería RabbitMQProducer y RabbitMQConsumer
            RabbitMQProducer producer = new RabbitMQProducer(connection, queueName);
            RabbitMQConsumer consumer = new RabbitMQConsumer(connection, queueName);

            // Publicar mensaje
            Console.WriteLine("📤 Enviando mensaje a RabbitMQ...");
            await producer.SendMessageAsync("Hola desde RabbitMQ 🚀");

            // Consumir mensajes
            Console.WriteLine("📥 Escuchando mensajes de RabbitMQ...");
            await consumer.ReceiveMessagesAsync(async message =>
            {
                Console.WriteLine($"➡️ RabbitMQ: {message}");
                await Task.CompletedTask;
            });

            Console.WriteLine("🔄 RabbitMQ consumer activo (presiona Ctrl+C para detener)...");
            await Task.Delay(-1); // Mantener app viva
        }
    }
}