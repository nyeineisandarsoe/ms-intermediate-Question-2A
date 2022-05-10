using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Producer.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;

namespace Producer.Controllers
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class TokenController : ControllerBase
    {

        [HttpPost]
        async public void Post()
        {
            //get token
            string token = await getToken();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost" , 
                Port = 5672
                //HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                //Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            Console.WriteLine(factory.HostName + ":" + factory.Port);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "TaskQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = token;
                var body = Encoding.UTF8.GetBytes(message);

                if(token != "")
                {
                    channel.BasicPublish(exchange: "",
                                     routingKey: "TaskQueue",
                                     basicProperties: null,
                                     body: body);
                } else
                {
                    throw new System.Web.Http.HttpResponseException(HttpStatusCode.Unauthorized);
                }
                
            }
        }

        private async Task<string> getToken()
        {
            var values = new Dictionary<string, string>
              {
                  { "email", "eve.holt@reqres.in" },
                  { "password", "pistol" },
                  { "task", "Any Task" }
              };

            var content = new FormUrlEncodedContent(values);

            var token = "";
            var url = "https://reqres.in/api/register";
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var tokenObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                if (tokenObj != null)
                {
                    token = tokenObj["token"];
                }
            }

            return token;
        }
    }

}
