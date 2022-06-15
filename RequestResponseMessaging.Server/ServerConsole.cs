using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RequestResponseMessaging.Config;
using System.Threading;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading.Tasks;


namespace RequestResponseMessaging.Server
{
    class ServerConsole
    {

        // Create request and response queue clients
        static QueueClient RequestQueueClient =
            new QueueClient(AccountDetails.ConnectionString, AccountDetails.RequestQueueName);

        static QueueClient ResponseQueueClient =
            new QueueClient(AccountDetails.ConnectionString, AccountDetails.ResponseQueueName);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Server Console");


            // Create a new management client
            var managementClient = new ManagementClient(AccountDetails.ConnectionString);

            Console.Write("Creatng queues...");

            // Delete any existing queues
            if (await managementClient.QueueExistsAsync(AccountDetails.RequestQueueName))
            {
                await managementClient.DeleteQueueAsync(AccountDetails.RequestQueueName);
            }

            if (await managementClient.QueueExistsAsync(AccountDetails.ResponseQueueName))
            {
                await managementClient.DeleteQueueAsync(AccountDetails.ResponseQueueName);
            }

            // Create Request Queue
            await managementClient.CreateQueueAsync(AccountDetails.RequestQueueName);

            // Create Response With Sessions 
            QueueDescription responseQueueDescription = 
                new QueueDescription(AccountDetails.ResponseQueueName)
            {
                RequiresSession = true
            };

            await managementClient.CreateQueueAsync(responseQueueDescription);

            Console.WriteLine("Done!");

            RequestQueueClient.RegisterMessageHandler
                (ProcessRequestMessage, new MessageHandlerOptions(ProcessMessageException));
            Console.WriteLine("Processing, hit Enter to exit.");
            Console.ReadLine();

            // Close the queue clients...
            await RequestQueueClient.CloseAsync();
            await ResponseQueueClient.CloseAsync();
        }



        private static async Task ProcessRequestMessage(Message requestMessage, CancellationToken arg2)
        {
            // Deserialize the message body into text.
            string text =  Encoding.UTF8.GetString(requestMessage.Body);
            Console.WriteLine("Received: " + text);

            Thread.Sleep(DateTime.Now.Millisecond * 20);

            string echoText = "Echo: " + text;

            // Create a response message using echoText as the message body.
            var responseMessage = new Message(Encoding.UTF8.GetBytes(echoText));

            // Set the session id
            responseMessage.SessionId = requestMessage.ReplyToSessionId;

            // Send the response message.
            await ResponseQueueClient.SendAsync(responseMessage);
            Console.WriteLine("Sent: " + echoText);
        }

        private static async Task ProcessMessageException(ExceptionReceivedEventArgs arg)
        {
            throw arg.Exception;
        }

    }


}
