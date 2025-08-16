namespace QueuePublisher.RabbitMQ
{
    /// <summary>
    /// Representa la configuración necesaria para conectarse a un servidor RabbitMQ.
    /// </summary>
    /// <remarks>
    /// Estos valores suelen leerse desde <c>appsettings.json</c> o variables de entorno 
    /// y luego inyectarse en la aplicación mediante <see cref="IOptions{T}"/>.
    /// 
    /// Ejemplo en <c>appsettings.json</c>:
    /// <code>
    /// "RabbitMQ": {
    ///   "HostName": "leopard-01.lmq.cloudamqp.com",
    ///   "UserName": "yunpypuq",
    ///   "Password": "your-password",
    ///   "VirtualHost": "yunpypuq",
    ///   "QueueName": "notification-queue-dev"
    /// }
    /// </code>
    /// </remarks>
    public class RabbitMQSettings
    {
        /// <summary>
        /// Dirección del host RabbitMQ (ejemplo: "localhost" o "leopard-01.lmq.cloudamqp.com").
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// Usuario para autenticarse en RabbitMQ.
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// Virtual host configurado en RabbitMQ (por defecto "/").
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// Nombre de la cola por defecto donde se publican/leen los mensajes.
        /// </summary>
        public string QueueName { get; set; } = "default-queue";
    }
}