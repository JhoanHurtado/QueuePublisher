namespace QueuePublisher.Interfaces
{
    /// <summary>
    /// Define la funcionalidad básica que debe implementar un productor de mensajes en una cola.
    /// Se encarga únicamente de enviar mensajes a la cola configurada.
    /// </summary>
    public interface IQueueProducer
    {
        /// <summary>
        /// Publica un mensaje en la cola.
        /// </summary>
        /// <param name="queueName">Nombre de la cola de destino.</param>
        /// <param name="message">Contenido del mensaje a enviar (generalmente en formato JSON o texto plano).</param>
        /// <returns>Tarea asincrónica que representa la operación de envío.</returns>
        Task SendMessageAsync(string queueName, string message);
    }
}