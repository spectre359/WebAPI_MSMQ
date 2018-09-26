using WebAPI_MSMQ.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CustomersHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            var queueAddress = ".\\private$\\customersQueue";

            using (var queue = new MessageQueue(queueAddress))
            {
                while (true)
                {
                    Console.WriteLine("Listening on: {0}", queueAddress);
                    var message = queue.Receive();
                    var reader = new StreamReader(message.BodyStream);
                    var json = reader.ReadToEnd();
                    var requestMessage = JsonConvert.DeserializeObject<WebAPI_MSMQ.Models.Message>(json);

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(requestMessage.URL);
                        var responseTask = await client.GetAsync(message.Label);
                        var customers = new List<CustomerViewModel>();
                        
                        if (responseTask.IsSuccessStatusCode)
                        {

                            var readTask = await responseTask.Content.ReadAsStringAsync();
                            
                            customers = JsonConvert.DeserializeObject<List<CustomerViewModel>>(readTask);
                            if (customers == null)
                            {
                                customers = new List<CustomerViewModel>();
                            }

                        }

                        using (var responseQueue = message.ResponseQueue)
                        {
                            var response = new System.Messaging.Message();
                            var jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(customers);
                            response.BodyStream = new MemoryStream(Encoding.Default.GetBytes(jsonMessage));
                            response.Label = customers.GetType().AssemblyQualifiedName;
                            responseQueue.Send(response);
                            Console.WriteLine("Found: {0} customers", customers.Count);
                        }

                    }

                }
            }
        }
    }
}
