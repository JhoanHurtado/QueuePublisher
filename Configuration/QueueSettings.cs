namespace QueuePublisher.Configuration
{
    /// <summary>
    /// Representa la configuración necesaria para conectarse y trabajar con servicios de colas,
    /// soportando tanto AWS SQS como RabbitMQ.
    /// </summary>
    public class QueueSettings
    {
        /// <summary>
        /// Nombre de la cola de AWS SQS.
        /// Ejemplo: my-queue
        /// </summary>
        public string SqsQueueName { get; set; } = string.Empty;

        /// <summary>
        /// Hostname o dirección del servidor RabbitMQ.
        /// Ejemplo: "localhost" o "leopard-01.lmq.cloudamqp.com"
        /// </summary>
        public string RabbitMqHost { get; set; } = "localhost";

        /// <summary>
        /// Nombre de usuario para autenticarse en RabbitMQ.
        /// Por defecto: "guest".
        /// </summary>
        public string RabbitMqUser { get; set; } = "guest";

        /// <summary>
        /// Contraseña para autenticarse en RabbitMQ.
        /// Por defecto: "guest".
        /// </summary>
        public string RabbitMqPassword { get; set; } = "guest";

        /// <summary>
        /// Virtual Host configurado en RabbitMQ.
        /// Por defecto: "/". En CloudAMQP suele coincidir con el usuario.
        /// </summary>
        public string RabbitMqVirtualHost { get; set; } = "/";

        /// <summary>
        /// Nombre de la cola en RabbitMQ sobre la cual se publicarán y consumirán mensajes.
        /// </summary>
        public string RabbitMqQueueName { get; set; } = "default-queue";
    }
}