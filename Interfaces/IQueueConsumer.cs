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
        /// <param name="handleMessage">
        /// Función asincrónica que procesa el contenido de cada mensaje recibido (en formato string).
        /// </param>
        /// <returns>
        /// Una tarea que representa la operación de consumo de mensajes.
        /// </returns>
        Task ReceiveMessagesAsync(Func<string, Task> handleMessage);
    }
}