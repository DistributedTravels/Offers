using System.Collections.Concurrent;
using System.Text;
using Models;
using Models.Offers;
using Newtonsoft.Json;
using Offers.Handlers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Offers;

public class EventManager
{
    private List<Type> events = new List<Type>(); // list of events handled by this service
    private Dictionary<string, IHandler> handlers =  new Dictionary<string, IHandler>(); // list of handlers and corresponding events
    private IConnection _connection; // connection to RabbitMQ
    private IModel _channel; // connection channel to RabbitMQ, required to ACK messages
    private readonly WebApplication app; // used for retreiving service context (mostly DB)

    private ConcurrentDictionary<string, TaskCompletionSource<string>> _taskQueue =
        new ConcurrentDictionary<string, TaskCompletionSource<string>>();
    
    public EventManager(WebApplication app)
    {
        this.app = app;
        this.RegisterHandler(new GetOffersHandler(this.Publish, this.Call, this.app), typeof(GetOffersEvent));
        // register the rest of Handlers + Events
    }

    /**
         * <summary>
         * Method ListenForEvents sets up connection and all queues that belong to this Service.
         * </summary>
         */
    public void ListenForEvents()
    {
        // connection details to "rabbitmq" broker on default port with credentials guest:guest
        // DispatchConsumersAsync to allow for Async Consumers
        var factory = new ConnectionFactory()
            { HostName = "rabbitmq", Password = "guest", UserName = "guest", DispatchConsumersAsync = true };
        var retry = new ManualResetEventSlim(false); // connection retry timeout
        while (!retry.Wait(3000)) // checks for connection every 3s or till successful connection
        {
            try
            {
                _connection = factory.CreateConnection();
                Console.WriteLine("Connected to message broker");
                retry.Set();
                _channel = _connection.CreateModel();
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += ReceiveEvent;
                var asyncResponseConsumer = new AsyncEventingBasicConsumer(_channel);
                asyncResponseConsumer.Received += ReceiveResponse;
                foreach (var @event in this.events) // create all queues and consumers handled in this service
                {
                    _channel.QueueDeclare(queue: @event.Name, durable: false, exclusive: false, autoDelete: false,
                        arguments: null);
                    _channel.QueueDeclare(queue: @event.Name + "ResponseQueue", durable: false, exclusive: false, autoDelete: false,
                        arguments: null);
                    _channel.BasicConsume(queue: @event.Name, autoAck: false, consumer: consumer);
                    _channel.BasicConsume(queue: @event.Name + "ResponseQueue", autoAck: false,
                        consumer: asyncResponseConsumer);
                }
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.WriteLine("Broker unreachable");
            }
        }
    }

    private void RegisterHandler(IHandler handler, Type @event)
    {
        this.RegisterEvent(@event);
        this.handlers.Add(@event.Name, handler);
    }

    /**
     * <summary>
     * Method RegisterEvent registers an event type. Type passed by using typeof().
     * </summary>
     */
    private void RegisterEvent(Type @event)
    {
       this.events.Add(@event);
    }

    /**
     * <summary>
     * Method FindHandler checks if there is a handler for received event and returns corresponding handler and event type.
     * </summary>
     */
    private IHandler? FindHandler(string eventName)
    {
        foreach(var @event in this.events)
        {
            if (@event.Name == eventName)
                return this.handlers[eventName];
        }
        return null;
    }

    /**
     * <summary>
     * Method ReceiveEvent fires when RabbitMQ consumer receives a message and passes the event to proper handler.
     * </summary>
     */
    private async Task ReceiveEvent(object sender, BasicDeliverEventArgs args)
    {
        Console.WriteLine("RabbitMQ Messege Received");
        var eventName = args.RoutingKey; // type of message in queue
        var handler = FindHandler(eventName);
        if (handler != null)
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            await handler.HandleEvent(message); // process event
            _channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false); // ACK message procesing, possibly unneeded (?)              
        } else {
            // write log about unknown event received?
            Console.WriteLine($"Received unknown event: {eventName}");
        }
    }

    private async Task ReceiveResponse(object sender, BasicDeliverEventArgs args)
    {
        Console.WriteLine("RabbitMQ Response Received");
        var message = Encoding.UTF8.GetString(args.Body.ToArray());
        if (_taskQueue.TryRemove(args.BasicProperties.CorrelationId, out var taskCompletionSource))
        {
            taskCompletionSource.SetResult(message);
        }
    }

    /**
     * <summary>
     * Method Publish sends passed event to proper RabbitMQ queue.
     * </summary>
     */
    private void Publish(EventModel @event)
    {
        // TODO: it'd be nice to add retry policy here
        var eventName = @event.GetType().Name; // name of IntegrationEvent that gets published
        using(var channel = _connection.CreateModel())
        {
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            channel.BasicPublish(
                exchange:"", // set exchange name
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }
    }

    private Task<string> Call(EventModel @event)
    {
        var callName = @event.GetType().Name;
        var taskCompletionSource = new TaskCompletionSource<string>();
        using (var channel = _connection.CreateModel())
        {
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);
            var messageId = Guid.NewGuid().ToString();
            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = callName + "ResponseQueue";
            properties.CorrelationId = messageId;
            channel.BasicPublish(
                exchange:"",
                routingKey: callName,
                basicProperties: properties,
                body: body);
            _taskQueue.TryAdd(messageId, taskCompletionSource);
        }
        return taskCompletionSource.Task;
    }
}