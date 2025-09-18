using Amazon.SQS;
using Amazon.SQS.Model;
using QueuePublisher.Interfaces;

namespace QueuePublisher.SQS
{
    /// <summary>
    /// Servicio para enviar y recibir mensajes desde una cola Amazon SQS.
    /// Implementa <see cref="IQueueProducer"/> y <see cref="IQueueConsumer"/>.
    /// </summary>
    public class AwsSqsService : IQueueConsumer, IQueueProducer
    {
        private readonly IAmazonSQS _sqsClient;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AwsSqsService"/>.
        /// </summary>
        /// <param name="sqsClient">Cliente de Amazon SQS.</param>
        public AwsSqsService(IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
            Console.WriteLine($"[AwsSqsService] Inicializado.");
        }

        /// <summary>
        /// Envía un mensaje a la cola SQS configurada.
        /// </summary>
        /// <param name="queueName">Nombre de la cola de destino.</param>
        /// <param name="message">Contenido del mensaje en formato texto.</param>
        public async Task SendMessageAsync(string queueName, string message)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
            var queueUrl = (await _sqsClient.GetQueueUrlAsync(queueName)).QueueUrl;

            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = message
            };

            var response = await _sqsClient.SendMessageAsync(sendRequest);
            Console.WriteLine($"[AwsSqsService] Mensaje enviado a {queueName}, MessageId={response.MessageId}");
        }

        /// <summary>
        /// Recibe mensajes en un bucle persistente desde la cola SQS y ejecuta el manejador.
        /// </summary>
        /// <param name="queueName">Nombre de la cola de la que se consumirán los mensajes.</param>
        /// <param name="handleMessage">Función async encargada de procesar el mensaje.</param>
        /// <param name="stoppingToken">Token de cancelación para detener el loop.</param>
        public async Task ReceiveMessagesAsync(string queueName, Func<string, Task> handleMessage, CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
            var queueUrl = (await _sqsClient.GetQueueUrlAsync(queueName)).QueueUrl;

            Console.WriteLine($"[SQS] Escuchando cola {queueName}...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10
                }, stoppingToken);

                if (response?.Messages != null)
                {
                    foreach (var msg in response.Messages)
                    {
                        try
                        {
                            Console.WriteLine($"[SQS] Mensaje recibido: {msg.Body}");

                            await handleMessage(msg.Body);

                            await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                            {
                                QueueUrl = queueUrl,
                                ReceiptHandle = msg.ReceiptHandle
                            }, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue processing other messages
                            Console.WriteLine($"[SQS] Error procesando mensaje: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[SQS] La respuesta de ReceiveMessageAsync fue nula o no contenía mensajes. Posible cancelación.");
                }
            }
        }

    }
}