using QueuePublisher.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace QueuePublisher.RabbitMQ
{
    /// <summary>
    /// Consumidor genérico de mensajes en RabbitMQ.
    /// Implementa <see cref="IQueueConsumer"/> para integrarse en la librería.
    /// </summary>
    public class RabbitMQConsumer : IQueueConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly ConcurrentDictionary<string, IChannel> _channels = new();

        /// <summary>
        /// Inicializa una nueva instancia del consumidor RabbitMQ.
        /// </summary>
        /// <param name="connection">Conexión activa hacia RabbitMQ.</param>
        public RabbitMQConsumer(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Console.WriteLine($"[RabbitMQConsumer] Inicializado.");
        }

        /// <summary>
        /// Inicia la recepción de mensajes desde una cola específica de RabbitMQ.
        /// Este método puede ser llamado múltiples veces para consumir de distintas colas simultáneamente.
        /// </summary>
        /// <param name="queueName">Nombre de la cola de la que se recibirán mensajes.</param>
        /// <param name="handleMessage">Función async encargada de procesar cada mensaje recibido.</param>
        /// <param name="stoppingToken">Token de cancelación para detener el consumo.</param>
        public async Task ReceiveMessagesAsync(string queueName, Func<string, Task> handleMessage, CancellationToken stoppingToken)
        {
            if (handleMessage == null) throw new ArgumentNullException(nameof(handleMessage));
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

            var channel = await _connection.CreateChannelAsync();
            _channels.TryAdd(queueName, channel);

            // Asegurar que la cola existe
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            // Registrar callback async
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"[RabbitMQConsumer] Mensaje recibido: {message}");

                    await handleMessage(message);

                    // Confirmar que el mensaje fue procesado
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    Console.WriteLine($"[RabbitMQConsumer] Mensaje ACK enviado (DeliveryTag={ea.DeliveryTag})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQConsumer] Error procesando mensaje: {ex}");
                }
            };

            // Iniciar consumo
            var consumerTag = await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine($"[RabbitMQConsumer] Escuchando en la cola '{queueName}' con tag {consumerTag}");

            // Registrar un callback para cuando se solicite la cancelación.
            // Esto asegura que la suscripción al consumidor se cancele limpiamente.
            stoppingToken.Register(async () =>
            {
                Console.WriteLine($"[RabbitMQConsumer] Cancelación recibida, cerrando consumidor para la cola '{queueName}'...");
                if (channel.IsOpen) await channel.BasicCancelAsync(consumerTag);
            });
        }

        public void Dispose()
        {
            foreach (var channel in _channels.Values)
            {
                channel?.Dispose();
            }
            Console.WriteLine($"[RabbitMQConsumer] {_channels.Count} canales cerrados y recursos liberados.");
        }
    }
}