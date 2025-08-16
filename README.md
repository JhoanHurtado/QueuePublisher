# QueuePublisher

**QueuePublisher** es una librería en **.NET 8** que proporciona una interfaz unificada para trabajar con diferentes **brokers de mensajería** como **AWS SQS** y **RabbitMQ**.  
El objetivo es simplificar el envío y consumo de mensajes sin acoplar tu aplicación a un proveedor específico.

---

## Características

- Interfaz común `IQueueProducer` y `IQueueConsumer` para todos los brokers.  
- Implementación para **AWS SQS**.  
- Implementación para **RabbitMQ** (compatible con CloudAMQP y servidores locales).  
- Mensajes persistentes.  
- Configuración flexible vía `appsettings.json`.  
- Ejemplo de uso con `Program.cs` para pruebas rápidas.

---

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
│   ├── RabbitMQSettings.cs     # Configuración de RabbitMQ
│
│── Configuration/
│   ├── QueueSettings.cs        # Configuración unificada para colas
│
│── Program.cs (ejemplo de prueba rápida)
│── QueuePublisher.csproj
```

---

## Instalación

Agrega las dependencias necesarias en tu proyecto:

```xml
<ItemGroup>
  <PackageReference Include="AWSSDK.Core" Version="4.0.0.22" />
  <PackageReference Include="AWSSDK.SQS" Version="4.0.0.20" />
  <PackageReference Include="RabbitMQ.Client" Version="7.1.2" />
</ItemGroup>
```

---

## Configuración

Ejemplo de `appsettings.json`:

```json
{
  "AWS": {
    "Region": "us-east-1",
    "Profile": "default",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key"
  },
  "SQSQueueName": "notification-queue-dev",
  "RabbitMQ": {
    "HostName": "leopard-01.lmq.cloudamqp.com",
    "UserName": "your-username",
    "Password": "your-password",
    "VirtualHost": "your-vhost",
    "QueueName": "mi-cola-rabbit"
  }
}
```

---

## Ejemplo de Uso

```csharp
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using QueuePublisher.SQS;
using QueuePublisher.RabbitMQ;
using RabbitMQ.Client;

class Program
{
    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        await TestSqs(config);
        await TestRabbitMq(config);
    }

    private static async Task TestSqs(IConfiguration config)
    {
        var sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(config["AWS:Region"]));
        var sqsService = new AwsSqsService(sqsClient, config["SQSQueueName"]);

        Console.WriteLine("📤 Enviando mensaje a SQS...");
        await sqsService.SendMessageAsync("Hola desde SQS 🚀");

        Console.WriteLine("📥 Recibiendo mensajes de SQS...");
        await sqsService.ReceiveMessagesAsync(async message =>
        {
            Console.WriteLine($"➡️ SQS: {message}");
            await Task.CompletedTask;
        });
    }

    private static async Task TestRabbitMq(IConfiguration config)
    {
        var rabbit = config.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = rabbit["HostName"],
            UserName = rabbit["UserName"],
            Password = rabbit["Password"],
            VirtualHost = rabbit["VirtualHost"]
        };

        using var connection = await factory.CreateConnectionAsync();

        var producer = new RabbitMQProducer(connection, rabbit["QueueName"]);
        var consumer = new RabbitMQConsumer(connection, rabbit["QueueName"]);

        Console.WriteLine("📤 Enviando mensaje a RabbitMQ...");
        await producer.SendMessageAsync("Hola desde RabbitMQ 🚀");

        Console.WriteLine("📥 Escuchando mensajes de RabbitMQ...");
        await consumer.ReceiveMessagesAsync(async message =>
        {
            Console.WriteLine($"➡️ RabbitMQ: {message}");
            await Task.CompletedTask;
        });

        await Task.Delay(-1); // Mantener app viva
    }
}
```

---

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
    Task ReceiveMessagesAsync(Func<string, Task> handleMessage);
}
```

---

## RabbitMQ Ejemplo con CloudAMQP

```csharp
var factory = new ConnectionFactory
{
    HostName = "your-host",
    UserName = "your-user",
    Password = "your-pass",
    VirtualHost = "your-vhost"
};
using var connection = await factory.CreateConnectionAsync();
var producer = new RabbitMQProducer(connection, "mi-cola-rabbit");
await producer.SendMessageAsync("Mensaje de prueba 🚀");
```

---

## AWS SQS Ejemplo

```csharp
var sqsClient = new AmazonSQSClient(Amazon.RegionEndpoint.USEast1);
var sqsService = new AwsSqsService(sqsClient, "notification-queue-dev");

await sqsService.SendMessageAsync("Mensaje de prueba en SQS 🚀");
await sqsService.ReceiveMessagesAsync(async message =>
{
    Console.WriteLine($"➡️ SQS: {message}");
});
```

---

## Roadmap

- [ ] Soporte para **Azure Service Bus**  
- [ ] Manejo avanzado de **Dead Letter Queues (DLQ)**  
- [ ] Integración con **Polly** para resiliencia  
- [ ] Publicación como **NuGet Package**

---

## 📝 Licencia

MIT License © 2025 - Desarrollado por **Jhoan Hurtado**
