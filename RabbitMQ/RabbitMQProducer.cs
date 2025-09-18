using System.Text;
using QueuePublisher.Interfaces;
using RabbitMQ.Client;

namespace QueuePublisher.RabbitMQ
{
    /// <summary>
    /// Productor de mensajes para RabbitMQ.
    /// Se encarga de publicar mensajes en una cola específica.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa <see cref="IQueueProducer"/> y está diseñada para usarse
    /// junto con <see cref="RabbitMQConsumer"/> dentro de la librería QueuePublisher.
    ///
    /// Ejemplo de configuración en <c>appsettings.json</c>:
    /// <code>
    /// "RabbitMQ": {
    ///   "HostName": "leopard-01.lmq.cloudamqp.com",
    ///   "UserName": "usuario",
    ///   "Password": "password",
    ///   "VirtualHost": "vhost",
    ///   "QueueName": "notification-queue-dev"
    /// }
    /// </code>
    ///
    /// Ejemplo de uso en código:
    /// <code>
    /// var factory = new ConnectionFactory
    /// {
    ///     HostName = settings.HostName,
    ///     UserName = settings.UserName,
    ///     Password = settings.Password,
    ///     VirtualHost = settings.VirtualHost
    /// };
    ///
    /// using var connection = await factory.CreateConnectionAsync();
    /// var producer = new RabbitMQProducer(connection, settings.QueueName);
    ///
    /// await producer.SendMessageAsync("Hola RabbitMQ!");
    /// </code>
    /// </remarks>
    public class RabbitMQProducer : IQueueProducer
    {
        private readonly IConnection _connection;

        /// <summary>
        /// Inicializa una nueva instancia del productor RabbitMQ.
        /// </summary>
        /// <param name="connection">Conexión activa a RabbitMQ.</param>
        public RabbitMQProducer(IConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Envía un mensaje a la cola de RabbitMQ configurada.
        /// </summary>
        /// <param name="queueName">Nombre de la cola donde se publicará el mensaje.</param>
        /// <param name="message">Mensaje en formato <see cref="string"/> a publicar.</param>
        public async Task SendMessageAsync(string queueName, string message)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

            // Crear canal async
            await using var channel = await _connection.CreateChannelAsync();

            // Declarar la cola (aseguramos que existe antes de publicar)
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Serializar mensaje a UTF8
            var body = Encoding.UTF8.GetBytes(message);

            // Propiedades del mensaje (persistente en disco)
            var properties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            // Publicar el mensaje
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }
    }
}