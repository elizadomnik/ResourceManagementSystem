using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Logging;

namespace ResourceManagementSystem.API.Services
{
    public interface IRabbitMQProducerService : IDisposable
    {
        void PublishMessage<T>(string exchangeName, string routingKey, T message, string? messageType = null);
    }

    public class RabbitMQProducerService : IRabbitMQProducerService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQProducerService> _logger;
        private readonly string _hostName;

        public RabbitMQProducerService(IConfiguration configuration, ILogger<RabbitMQProducerService> logger)
        {
            _logger = logger;
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                    UserName = configuration["RabbitMQ:UserName"], 
                    Password = configuration["RabbitMQ:Password"], 
                    Port = AmqpTcpEndpoint.UseDefaultPort
                };
                _hostName = factory.HostName;
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _logger.LogInformation("Successfully connected to RabbitMQ host: {HostName}", _hostName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ host: {HostName}. Messages will not be published.", _hostName ?? "config_not_found");
               
                throw; 
            }
        }

        public void PublishMessage<T>(string exchangeName, string routingKey, T message, string? messageType = null)
        {
            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogWarning("RabbitMQ channel is not open. Message of type {MessageType} for exchange {ExchangeName} not published.",
                    messageType ?? typeof(T).Name, exchangeName);
                return;
            }

            try
            {
                _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

                var jsonMessage = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                if (!string.IsNullOrEmpty(messageType))
                {
                    properties.Type = messageType;
                }
                else
                {
                    properties.Type = typeof(T).Name;
                }


                _channel.BasicPublish(exchange: exchangeName,
                                     routingKey: routingKey,
                                     basicProperties: properties,
                                     body: body);
                _logger.LogInformation("Message published to RabbitMQ. Exchange: {Exchange}, RoutingKey: {RoutingKey}, Type: {Type}",
                    exchangeName, routingKey, properties.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to RabbitMQ. Exchange: {Exchange}, RoutingKey: {RoutingKey}",
                    exchangeName, routingKey);
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection and channel closed for host: {HostName}.", _hostName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RabbitMQ Dispose for host: {HostName}.", _hostName);
            }
            GC.SuppressFinalize(this);
        }
    }
}