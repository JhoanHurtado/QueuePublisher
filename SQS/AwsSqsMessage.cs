namespace QueuePublisher.SQS
{
    /// <summary>
    /// Representa un mensaje recibido o enviado a través de Amazon SQS.
    /// </summary>
    /// <remarks>
    /// Esta clase actúa como un contenedor simple para la información esencial de un mensaje en SQS.
    /// - <see cref="MessageId"/> es asignado por el servicio de SQS cuando el mensaje es enviado.
    /// - <see cref="Body"/> contiene el contenido del mensaje en formato string (puede ser JSON u otro texto).
    /// 
    /// Ejemplo de uso al recibir mensajes:
    /// <code>
    /// var messages = await sqsService.ReceiveMessagesAsync();
    /// foreach (AwsSqsMessage msg in messages)
    /// {
    ///     Console.WriteLine($"ID: {msg.MessageId}, Contenido: {msg.Body}");
    /// }
    /// </code>
    /// </remarks>
    public class AwsSqsMessage
    {
        /// <summary>
        /// Contenido del mensaje enviado o recibido desde la cola SQS.
        /// </summary>
        /// <value>
        /// Cadena en formato texto que puede contener JSON u otra representación serializada de los datos.
        /// Nunca es <c>null</c>, por defecto se inicializa como <see cref="string.Empty"/>.
        /// </value>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del mensaje en SQS.
        /// </summary>
        /// <remarks>
        /// Este valor lo asigna automáticamente Amazon SQS cuando se envía un mensaje.
        /// Puede usarse para rastrear mensajes, depurar o registrar eventos de procesamiento.
        /// </remarks>
        public string MessageId { get; set; } = string.Empty;
    }
}