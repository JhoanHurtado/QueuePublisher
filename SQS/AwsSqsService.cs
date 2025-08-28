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
        private readonly string _queueName;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AwsSqsService"/>.
        /// </summary>
        /// <param name="sqsClient">Cliente de Amazon SQS.</param>
        /// <param name="queueName">Nombre de la cola.</param>
        public AwsSqsService(IAmazonSQS sqsClient, string queueName)
        {
            _sqsClient = sqsClient;
            _queueName = queueName;
            Console.WriteLine($"[AwsSqsService] Inicializado para la cola: {_queueName}");
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

            var response = await _sqsClient.SendMessageAsync(sendRequest);
            Console.WriteLine($"[AwsSqsService] Mensaje enviado a {_queueName}, MessageId={response.MessageId}");
        }

        /// <summary>
        /// Recibe mensajes en un bucle persistente desde la cola SQS y ejecuta el manejador.
        /// </summary>
        /// <param name="handleMessage">Función async encargada de procesar el mensaje.</param>
        /// <param name="stoppingToken">Token de cancelación para detener el loop.</param>
        public async Task ReceiveMessagesAsync(Func<string, Task> handleMessage, CancellationToken stoppingToken)
        {
            var queueUrl = (await _sqsClient.GetQueueUrlAsync(_queueName)).QueueUrl;

            Console.WriteLine($"[SQS] Escuchando cola {_queueName}...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10 // long polling
                }, stoppingToken);

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
                        Console.WriteLine($"[SQS] Error procesando mensaje: {ex.Message}");
                    }
                }
            }
        }

    }
}