using QueuePublisher.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace QueuePublisher.RabbitMQ
{
    /// <summary>
    /// Consumidor genérico de mensajes en RabbitMQ.
    /// Implementa <see cref="IQueueConsumer"/> para integrarse en la librería.
    /// </summary>
    public class RabbitMQConsumer : IQueueConsumer, IDisposable
    {
        private readonly IChannel _channel;
        private readonly string _queueName;

        /// <summary>
        /// Inicializa una nueva instancia del consumidor RabbitMQ.
        /// </summary>
        /// <param name="connection">Conexión activa hacia RabbitMQ.</param>
        /// <param name="queueName">Nombre de la cola de la que se recibirán mensajes.</param>
        public RabbitMQConsumer(IConnection connection, string queueName)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

            // Crear canal usando la nueva API async
            _channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
            _queueName = queueName;

            Console.WriteLine($"[RabbitMQConsumer] Inicializado para la cola: {_queueName}");
        }

        /// <summary>
        /// Inicia la recepción de mensajes desde RabbitMQ en un loop persistente.
        /// </summary>
        /// <param name="handleMessage">Función async encargada de procesar cada mensaje recibido.</param>
        /// <param name="stoppingToken">Token de cancelación para detener el consumo.</param>
        public async Task ReceiveMessagesAsync(Func<string, Task> handleMessage, CancellationToken stoppingToken)
        {
            if (handleMessage == null) throw new ArgumentNullException(nameof(handleMessage));

            // Asegurar que la cola existe
            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);

            // Registrar callback async
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"[RabbitMQConsumer] Mensaje recibido: {message}");

                    await handleMessage(message);

                    // Confirmar que el mensaje fue procesado
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    Console.WriteLine($"[RabbitMQConsumer] Mensaje ACK enviado (DeliveryTag={ea.DeliveryTag})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQConsumer] Error procesando mensaje: {ex}");
                }
            };

            // Iniciar consumo
            var consumerTag = await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine($"[RabbitMQConsumer] Escuchando en la cola '{_queueName}' con tag {consumerTag}");

            // Mantener vivo hasta que se cancele
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("[RabbitMQConsumer] Cancelación recibida, cerrando consumidor...");
            }

            // Cancelar suscripción y cerrar
            await _channel.BasicCancelAsync(consumerTag);
            Console.WriteLine($"[RabbitMQConsumer] Suscripción cancelada en la cola '{_queueName}'");
        }

        public void Dispose()
        {
            _channel?.Dispose();
            Console.WriteLine("[RabbitMQConsumer] Canal cerrado y recursos liberados.");
        }
    }
}