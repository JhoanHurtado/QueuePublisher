namespace QueuePublisher.Interfaces
{
    /// <summary>
    /// Define el contrato para un consumidor de colas de mensajes.
    /// Se encarga de suscribirse a una cola y procesar los mensajes recibidos.
    /// </summary>
    public interface IQueueConsumer
    {
        /// <summary>
        /// Inicia la recepción de mensajes desde la cola configurada.
        /// Cada mensaje recibido será entregado al <paramref name="handleMessage"/> para su procesamiento.
        /// </summary>
        /// <param name="queueName">Nombre de la cola de la que se consumirán los mensajes.</param>
        /// <param name="handleMessage">
        /// Función asincrónica que procesa el contenido de cada mensaje recibido (en formato string).
        /// </param>
        /// <param name="stoppingToken">Token para detener la operación de consumo.</param>
        /// <returns>
        /// Una tarea que representa la operación de consumo de mensajes.
        /// </returns>
        Task ReceiveMessagesAsync(string queueName, Func<string, Task> handleMessage, CancellationToken stoppingToken);
    }
}