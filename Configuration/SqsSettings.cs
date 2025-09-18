namespace QueuePublisher.Configuration
{
    /// <summary>
    /// Contiene la configuración específica para AWS SQS.
    /// </summary>
    public class SqsSettings
    {
        /// <summary>
        /// Un diccionario de colas donde la llave es un nombre lógico y el valor es el nombre real de la cola en SQS.
        /// </summary>
        public Dictionary<string, string> Queues { get; set; } = new();
    }
}