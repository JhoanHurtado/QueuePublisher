namespace QueuePublisher.RabbitMQ
{
    /// <summary>
    /// Representa un mensaje genérico recibido o enviado a través de RabbitMQ.
    /// </summary>
    /// <remarks>
    /// Esta clase actúa como un contenedor simple para el contenido del mensaje.
    /// Puede usarse tanto en productores (<see cref="RabbitMQProducer"/>)
    /// como en consumidores (<see cref="RabbitMQConsumer"/>).
    /// </remarks>
    public class RabbitMQMessage
    {
        /// <summary>
        /// Contenido del mensaje en formato texto (UTF-8).
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Identificador opcional del mensaje (útil para trazabilidad).
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Routing key usada para entregar el mensaje (solo si aplica).
        /// </summary>
        public string RoutingKey { get; set; } = string.Empty;
    }
}