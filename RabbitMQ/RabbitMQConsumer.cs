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
    public class RabbitMQConsumer : IQueueConsumer
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
        }

        /// <summary>
        /// Inicia la recepción de mensajes desde RabbitMQ.
        /// </summary>
        /// <param name="handleMessage">Función async encargada de procesar cada mensaje recibido.</param>
        public async Task ReceiveMessagesAsync(Func<string, Task> handleMessage)
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
                    await handleMessage(message);

                    // Confirmar que el mensaje fue procesado
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQConsumer] Error procesando mensaje: {ex.Message}");
                }
            };

            // Iniciar consumo
            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );
        }
    }
}