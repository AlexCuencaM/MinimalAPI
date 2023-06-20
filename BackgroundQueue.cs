using System.Text.Json;
using System.Threading.Channels;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace BackgroundQueueService;

public class BackgroundQueue : BackgroundService
{
    private readonly Channel<ReadOnlyMemory<byte>> _queue;
    private readonly ILogger<BackgroundQueue> _logger;

    public BackgroundQueue(Channel<ReadOnlyMemory<byte>> queue,
                               ILogger<BackgroundQueue> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var dataStream in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                
                _logger.LogInformation($"Saving data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}


public class MyStorageQueue{
    public string Name {get; set;}
    public string StorageConnection {get; set;}
    public MyStorageQueue(string queueName, string storageConnection){
        Name = queueName;
        StorageConnection = storageConnection;
        
    }
    //-------------------------------------------------
    // Create a message queue
    //-------------------------------------------------
    public async Task<string> CreateQueue()
    {
        try
        {   
            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(StorageConnection, Name);

            // Create the queue
            await queueClient.CreateIfNotExistsAsync();

            if (await queueClient.ExistsAsync())
            {
                return $"Queue created: '{queueClient.Name}'";
            }
            else
            {
                return $"Make sure the Azurite storage emulator running and try again.";
            }
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }

    //-------------------------------------------------
// Insert a message into a queue
//-------------------------------------------------
    public async Task<string> InsertMessage(string message)
    {
        // Get the connection string from app settings
        // Instantiate a QueueClient which will be used to create and manipulate the queue
        QueueClient queueClient = new QueueClient(StorageConnection, Name);

        // Create the queue if it doesn't already exist
        await queueClient.CreateIfNotExistsAsync();

        if (await queueClient.ExistsAsync())
        {
            // Send a message to the queue
            await queueClient.SendMessageAsync(message);
        }

        return $"Inserted: {message}";
    }
    //-------------------------------------------------
// Peek at a message in the queue
//-------------------------------------------------
    public async Task<string> PeekMessage()
    {
        // Instantiate a QueueClient which will be used to manipulate the queue
        QueueClient queueClient = new QueueClient(StorageConnection, Name);

        if (await queueClient.ExistsAsync())
        { 
            // Peek at the next message
            PeekedMessage[] peekedMessage = await queueClient.PeekMessagesAsync();

            // Display the message
            return $"Peeked message: '{peekedMessage[0].Body}'";
        }
        return "Queue doesn't exist";
    }
    //-------------------------------------------------
    // Process and remove a message from the queue
    //-------------------------------------------------
    public async Task<string> DequeueMessage()
    {   

        // Instantiate a QueueClient which will be used to manipulate the queue
        QueueClient queueClient = new QueueClient(StorageConnection, Name);

        if (await queueClient.ExistsAsync())
        {
            // Get the next message
            QueueMessage[] retrievedMessage = await queueClient.ReceiveMessagesAsync();
            // Process (i.e. print) the message in less than 30 seconds
            string result = $"Dequeued message: '{retrievedMessage[0].Body}'";
            // Delete the message
            await queueClient.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
            return result;
        }
        return $"Queue {Name} doesn't exist";
    }
}

class Person
{
    public string Name { get; set; } = String.Empty;
    public int Age { get; set; }
    public string Country { get; set; } = String.Empty;
}