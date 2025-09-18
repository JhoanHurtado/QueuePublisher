Aquí tienes el archivo `README.md` completo y listo para copiar y pegar. He consolidado toda la información, incluyendo los ajustes y configuraciones adicionales que te he proporcionado, para que tengas un documento exhaustivo y claro.

-----

# QueuePublisher

**QueuePublisher** es una librería en **.NET 8** que proporciona una interfaz unificada para trabajar con diferentes **brokers de mensajería** como **AWS SQS** y **RabbitMQ**. El objetivo es simplificar el envío y consumo de mensajes sin acoplar tu aplicación a un proveedor específico.

-----

## Características

  * Interfaz común `IQueueProducer` y `IQueueConsumer` para todos los brokers.
  * Implementación para **AWS SQS**.
  * Implementación para **RabbitMQ** (compatible con CloudAMQP y servidores locales).
  * Mensajes persistentes.
  * **Soporte para múltiples colas** con una única instancia de productor/consumidor.
  * Configuración flexible vía `appsettings.json`.
  * Uso de **`IHostedService`** para consumo de mensajes en segundo plano, ideal para servicios de larga duración.
  * Soporte para **`long polling`** en SQS, optimizando el consumo y reduciendo costos.

-----

## Estructura del Proyecto

```
QueuePublisher/
│── Interfaces/
│   ├── IQueueProducer.cs       # Interfaz para publicar mensajes
│   ├── IQueueConsumer.cs       # Interfaz para consumir mensajes
│
│── SQS/
│   ├── AwsSqsService.cs        # Implementación de SQS (Producer + Consumer)
│   ├── AwsSqsMessage.cs        # Representación de un mensaje recibido
│
│── RabbitMQ/
│   ├── RabbitMQProducer.cs     # Implementación de RabbitMQ Producer
│   ├── RabbitMQConsumer.cs     # Implementación de RabbitMQ Consumer
│   ├── RabbitMQMessage.cs      # Representación de un mensaje recibido
│
│── Configuration/
│   ├── QueueSettings.cs        # Configuración unificada para colas
│
│── Program.cs                  # Ejemplo de configuración de un Worker Service
│── QueuePublisher.csproj
```

-----

## Instalación

Agrega las dependencias necesarias en tu proyecto:

```xml
<ItemGroup>
  <PackageReference Include="AWSSDK.Core" Version="4.0.0.22" />
  <PackageReference Include="AWSSDK.SQS" Version="4.0.0.20" />
  <PackageReference Include="RabbitMQ.Client" Version="7.1.2" />
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
</ItemGroup>
```

-----

## Configuración

### 1\. Configuración Unificada en `appsettings.json`

Usa la clase `QueueSettings` para mapear los valores de configuración de ambos brokers. Esto te permite gestionar toda la configuración desde un solo lugar.

```json
{
  "AWS": {
    "Region": "us-east-1",
    "Profile": "default"
  },
  "QueueSettings": {
    "SqsQueueName": "notification-queue-dev",
    "RabbitMqHost": "leopard-01.lmq.cloudamqp.com",
    "RabbitMqUser": "your-username",
    "RabbitMqPassword": "your-password",
    "RabbitMqVirtualHost": "your-vhost",
    "RabbitMqQueueName": "mi-cola-rabbit"
  }
}
```

### 2\. Configuraciones Alternas

#### Para Desarrollo Local 💻

Para usar RabbitMQ o una emulación de SQS (como **LocalStack**), solo necesitas ajustar los valores en tu `appsettings.json`.

**RabbitMQ Local:**

```json
{
  "QueueSettings": {
    "RabbitMqHost": "localhost",
    "RabbitMqUser": "guest",
    "RabbitMqPassword": "guest"
  }
}
```

**AWS SQS con LocalStack:**
Añade `ServiceURL` para redirigir el cliente de AWS a tu endpoint local.

```json
{
  "AWS": {
    "Region": "us-east-1",
    "ServiceURL": "http://localhost:4566"
  },
  "QueueSettings": {
    "SqsQueueName": "dev-local-sqs-queue"
  }
}
```

-----

## Ejemplo de Implementación Completo con `IHostedService`

Para escuchar las colas de forma continua y asíncrona, se utiliza un `BackgroundService` que implementa `IHostedService`. Este servicio se ejecuta en segundo plano durante toda la vida de la aplicación.

### 1\. Servicio de Consumo (`NotificationWorkerService.cs`)

Este servicio es el encargado de procesar los mensajes de las colas configuradas en `appsettings.json`. Inyecta los consumidores y la configuración para saber de qué colas escuchar.

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QueuePublisher.Configuration;
using QueuePublisher.RabbitMQ;
using QueuePublisher.SQS;

public class NotificationWorkerService : BackgroundService
{
    private readonly AwsSqsService _sqsConsumer;
    private readonly RabbitMQConsumer _rabbitConsumer;
    private readonly QueueSettings _queueSettings;

    public NotificationWorkerService(
        AwsSqsService sqsConsumer,
        RabbitMQConsumer rabbitConsumer,
        IOptions<QueueSettings> queueSettings)
    {
        _sqsConsumer = sqsConsumer;
        _rabbitConsumer = rabbitConsumer;
        _queueSettings = queueSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Tarea para procesar mensajes de SQS
        Task sqsTask = _sqsConsumer.ReceiveMessagesAsync(
            _queueSettings.SqsQueueName, // Especifica la cola
            async message =>
            {
                Console.WriteLine($"[SQS Worker] Mensaje de '{_queueSettings.SqsQueueName}': {message}");
                await Task.CompletedTask; // Lógica de procesamiento
            },
            stoppingToken);

        // Tarea para procesar mensajes de RabbitMQ
        Task rabbitMqTask = _rabbitConsumer.ReceiveMessagesAsync(
            _queueSettings.RabbitMqQueueName, // Especifica la cola
            async message =>
            {
                Console.WriteLine($"[RabbitMQ Worker] Mensaje de '{_queueSettings.RabbitMqQueueName}': {message}");
                await Task.CompletedTask; // Lógica de procesamiento
            },
            stoppingToken);

        // Esperar a que ambas tareas finalicen (esto solo ocurre si se detiene el servicio)
        await Task.WhenAll(sqsTask, rabbitMqTask);
    }
}
```

### 2\. Configuración en `Program.cs`

Este es el código clave que configura la inyección de dependencias y asegura que tus consumidores estén siempre escuchando.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueuePublisher.Configuration;
using QueuePublisher.RabbitMQ;
using QueuePublisher.SQS;
using Amazon.SQS;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Mapear la sección "QueueSettings" a la clase QueueSettings
        services.Configure<QueueSettings>(hostContext.Configuration.GetSection("QueueSettings"));
        var queueSettings = hostContext.Configuration.GetSection("QueueSettings").Get<QueueSettings>();

        // Configurar el cliente SQS
        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var config = new AmazonSQSConfig();
            config.Region = Amazon.RegionEndpoint.GetBySystemName(hostContext.Configuration["AWS:Region"]);
            // Opcional: Configurar el ServiceURL para LocalStack
            if (!string.IsNullOrEmpty(hostContext.Configuration["AWS:ServiceURL"]))
            {
                config.ServiceURL = hostContext.Configuration["AWS:ServiceURL"];
            }

            return new AmazonSQSClient(config);
        });

        // Registrar el consumidor de SQS como Singleton
        services.AddSingleton<AwsSqsService>(sp => new AwsSqsService(
            sp.GetRequiredService<IAmazonSQS>(),
            queueSettings.SqsQueueName
        ));

        // Configurar la conexión de RabbitMQ (conexión única)
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = queueSettings.RabbitMqHost,
                UserName = queueSettings.RabbitMqUser,
                Password = queueSettings.RabbitMqPassword,
                VirtualHost = queueSettings.RabbitMqVirtualHost
            };
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        // Registrar el consumidor de RabbitMQ como Singleton
        services.AddSingleton<RabbitMQConsumer>(sp => new RabbitMQConsumer(
            sp.GetRequiredService<IConnection>(),
            queueSettings.RabbitMqQueueName
        ));

        // Registrar el Hosted Service que consumirá los mensajes en bucle
        services.AddHostedService<NotificationWorkerService>();
    })
    .Build();

await host.RunAsync();
```

-----

## Interfaces

### Producer

```csharp
public interface IQueueProducer
{
    Task SendMessageAsync(string message);
}
```

### Consumer

```csharp
public interface IQueueConsumer
{
    Task ReceiveMessagesAsync(Func<string, Task> handleMessage, CancellationToken stoppingToken);
}
```

-----

## Roadmap

  * [ ] Soporte para **Azure Service Bus**
  * [ ] Manejo avanzado de **Dead Letter Queues (DLQ)**
  * [ ] Integración con **Polly** para resiliencia
  * [ ] Publicación como **NuGet Package**

-----

## 📝 Licencia

MIT License © 2025 - Desarrollado por **Jhoan Hurtado**