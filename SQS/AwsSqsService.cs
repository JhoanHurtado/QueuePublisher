using Amazon.SQS;
using Amazon.SQS.Model;
using QueuePublisher.Interfaces;

namespace QueuePublisher.SQS
{
    /// <summary>
    /// Implementación de <see cref="IQueueProducer"/> y <see cref="IQueueConsumer"/> 
    /// que utiliza Amazon Simple Queue Service (SQS) como backend de mensajería.
    /// </summary>
    /// <remarks>
    /// Esta clase permite enviar y recibir mensajes de una cola SQS.
    /// 
    /// - El nombre de la cola (<see cref="_queueName"/>) se pasa en el constructor.
    /// - Se utiliza <see cref="IAmazonSQS"/> para interactuar con AWS SQS.
    /// - Al recibir mensajes, los elimina de la cola una vez procesados.
    /// 
    /// Ejemplo de uso:
    /// <code>
    /// var sqsClient = new AmazonSQSClient();
    /// var sqsService = new AwsSqsService(sqsClient, "mi-cola");
    ///
    /// // Enviar un mensaje
    /// await sqsService.SendMessageAsync("Hola desde SQS");
    ///
    /// // Recibir y procesar mensajes
    /// await sqsService.ReceiveMessagesAsync(async msg =>
    /// {
    ///     Console.WriteLine($"Mensaje recibido: {msg}");
    ///     await Task.CompletedTask;
    /// });
    /// </code>
    /// </remarks>
    public class AwsSqsService : IQueueConsumer, IQueueProducer
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueName;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AwsSqsService"/>.
        /// </summary>
        /// <param name="sqsClient">Cliente de Amazon SQS para realizar las operaciones.</param>
        /// <param name="queueName">Nombre de la cola SQS a utilizar.</param>
        public AwsSqsService(IAmazonSQS sqsClient, string queueName)
        {
            _sqsClient = sqsClient;
            _queueName = queueName;
        }

        /// <summary>
        /// Envía un mensaje a la cola SQS configurada.
        /// </summary>
        /// <param name="message">Contenido del mensaje en formato texto.</param>
        public async Task SendMessageAsync(string message)
        {
            var queueUrl = (await _sqsClient.GetQueueUrlAsync(_queueName)).QueueUrl;

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message
            };

            await _sqsClient.SendMessageAsync(sendRequest);
        }

        /// <summary>
        /// Recibe mensajes de la cola SQS configurada y ejecuta un manejador para cada mensaje.
        /// </summary>
        /// <param name="handleMessage">
        /// Función asíncrona que define cómo procesar el contenido del mensaje.
        /// </param>
        /// <remarks>
        /// Los mensajes se eliminan automáticamente de la cola una vez procesados exitosamente.
        /// </remarks>
        public async Task ReceiveMessagesAsync(Func<string, Task> handleMessage)
        {
            var queueUrl = (await _sqsClient.GetQueueUrlAsync(_queueName)).QueueUrl;

            var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 10
            });

            foreach (var msg in response.Messages)
            {
                await handleMessage(msg.Body);

                await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = msg.ReceiptHandle
                });
            }
        }
    }
}