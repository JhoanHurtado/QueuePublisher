using QueuePublisher.RabbitMQ;

namespace QueuePublisher.Configuration
{
    /// <summary>
    /// Representa la configuración necesaria para conectarse y trabajar con servicios de colas,
    /// soportando tanto AWS SQS como RabbitMQ.
    /// </summary>
    /// <remarks>
    /// Esta clase actúa como el contenedor raíz para las configuraciones específicas de cada proveedor de colas.
    /// </remarks>
    public class QueueSettings
    {
        /// <summary>
        /// Configuración para AWS SQS.
        /// </summary>
        public SqsSettings Sqs { get; set; } = new();
        /// <summary>
        /// Configuración para RabbitMQ.
        /// </summary>
        public RabbitMQSettings RabbitMq { get; set; } = new();
    }
}