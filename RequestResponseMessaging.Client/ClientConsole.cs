using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RequestResponseMessaging.Config;
using System.Diagnostics;
using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;

namespace RequestResponseMessaging.Client
{
    class ClientConsole
    {
        // Create request and response queue clients
        static QueueClient RequestQueueClient =
            new QueueClient(AccountDetails.ConnectionString, AccountDetails.RequestQueueName);

        static QueueClient ResponseQueueClient =
            new QueueClient(AccountDetails.ConnectionString, AccountDetails.ResponseQueueName);


        static async Task Main(string[] args)
        {
            Console.WriteLine("Client Console");







            while (true)
            {
                Console.WriteLine("Enter text:");
                string text = Console.ReadLine();

                // Create a session identifyer for the response message
                string responseSessionId = Guid.NewGuid().ToString();

                // Create a message using text as the body.
                var requestMessage = new Message(Encoding.UTF8.GetBytes(text));

                // Set the appropriate message properties.
                requestMessage.ReplyToSessionId = responseSessionId;

                var stopwatch = Stopwatch.StartNew();

                // Send the message on the request queue.
                await RequestQueueClient.SendAsync(requestMessage);

                // Create a session client
                var sessionClient = 
                    new SessionClient(AccountDetails.ConnectionString, AccountDetails.ResponseQueueName);

                // Accept a message session
                var messageSession = await sessionClient.AcceptMessageSessionAsync(responseSessionId);

                // Receive the response message.
                var responseMessage = await messageSession.ReceiveAsync();
                stopwatch.Stop();

                // Close the session, we got the message.
                await sessionClient.CloseAsync();

                // Deserialise the message body to echoText.
                string echoText = Encoding.UTF8.GetString(responseMessage.Body);
                
                Console.WriteLine(echoText);
                Console.WriteLine("Time: {0} ms.", stopwatch.ElapsedMilliseconds);
                Console.WriteLine();


            }
        }
    }
}
