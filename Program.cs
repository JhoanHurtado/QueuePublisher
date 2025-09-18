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
            string queueNameSqs = "notification-queue-dev";

            // Usar la librería AwsSqsService
            AwsSqsService sqsService = new AwsSqsService(sqsClient);

            // Enviar mensaje
            Console.WriteLine("📤 Enviando mensaje a SQS...");
            await sqsService.SendMessageAsync(queueNameSqs, "Hola desde SQS 🚀");

            // Recibir mensajes
            Console.WriteLine("📥 Recibiendo mensajes de SQS...");
            /*await sqsService.ReceiveMessagesAsync(queueNameSqs, async message =>
            {
                Console.WriteLine($"➡️ SQS: {message}");
                await Task.CompletedTask;
            });*/
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

            string queueNameA = "cola_A";
            string queueNameB = "cola_B";

            // Usar la librería RabbitMQProducer y RabbitMQConsumer
            // Una sola instancia para manejar múltiples colas
            RabbitMQProducer producer = new RabbitMQProducer(connection);
            using RabbitMQConsumer consumer = new RabbitMQConsumer(connection);

            // Publicar mensajes en diferentes colas
            Console.WriteLine($"📤 Enviando mensaje a la cola '{queueNameA}'...");
            await producer.SendMessageAsync(queueNameA, $"Hola desde RabbitMQ en '{queueNameA}' 🚀");

            Console.WriteLine($"📤 Enviando mensaje a la cola '{queueNameB}'...");
            await producer.SendMessageAsync(queueNameB, $"Hola desde RabbitMQ en '{queueNameB}' 🚀");

            // Consumir mensajes
            Console.WriteLine("📥 Escuchando mensajes de RabbitMQ en paralelo...");

            // Crear un token de cancelación para detener los consumidores
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) => { e.Cancel = true; cts.Cancel(); };

            // Iniciar consumidores para cada cola
            Task consumerA = consumer.ReceiveMessagesAsync(queueNameA, async message =>
            {
                Console.WriteLine($"➡️ Mensaje de '{queueNameA}': {message}");
                await Task.CompletedTask;
            }, cts.Token);

            Task consumerB = consumer.ReceiveMessagesAsync(queueNameB, async message =>
            {
                Console.WriteLine($"➡️ Mensaje de '{queueNameB}': {message}");
                await Task.CompletedTask;
            }, cts.Token);

            Console.WriteLine("🔄 Consumidores de RabbitMQ activos (presiona Ctrl+C para detener)...");
            await Task.WhenAll(consumerA, consumerB);
        }
    }
}