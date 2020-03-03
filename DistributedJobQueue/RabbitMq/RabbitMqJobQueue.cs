using DistributedJobQueue.Job;
using DistributedJobQueue.Requirements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Linq;
using RabbitMQ.Client.Events;
using DistributedJobQueue.Queue;
using DistributedJobQueue.Fulfillments;

namespace DistributedJobQueue.RabbitMq
{
    public class RabbitMqJobQueue : IJobQueue, IDisposable
    {
        private IConnection Connection { get; }
        private IModel Channel { get; }
        private string ExchangeName { get; }
        private string RoutingKey { get; }
        private string QueueName { get; }
        private IBasicConsumer Consumer { get; }
        public RabbitMqJobQueue(ConnectionFactory factory, string exchangeName, string queueName, string routingKey)
        {
            Connection = factory.CreateConnection();

            Channel = Connection.CreateModel();

            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            QueueName = queueName;

            Channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            Channel.QueueDeclare(QueueName, false, false, false, null);
            Channel.QueueBind(QueueName, ExchangeName, RoutingKey, null);

            Consumer = new EventingBasicConsumer(Channel);
            //Consumer.Received += (ch, ea) =>
            //{
            //    var body = ea.Body;
            //    // ... process the message
            //    channel.BasicAck(ea.DeliveryTag, false);
            //};
            //string consumerTag = Channel.BasicConsume(QueueName, false, Channel);
        }

        JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
        {

        };
        public Task<bool> TryEnqueueAsync(IJob job)
        {
            string json = JsonConvert.SerializeObject(job, JsonSerializerSettings);
            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(json);

            IBasicProperties props = Channel.CreateBasicProperties();
            props.ContentType = "text/json";
            props.DeliveryMode = 2;
            //props.Expiration = "36000000";

            props.Headers = new Dictionary<string, object>();
            props.Headers.Add("jobId", job.JobId.ToString());
            props.Headers.Add("requirementTags", job.Requirement.GetRequirementTags().Aggregate((a, b) => $"{a},{b}"));

            Channel.BasicPublish(ExchangeName, RoutingKey, props, messageBodyBytes);

            return Task.FromResult(true);
        }

        public void Dispose()
        {
            Channel.Close();
            Connection.Close();
        }

        public Task<(bool, IJob)> TryDequeueAsync(IEnumerable<IFulfillment> requirementsFulfillable = null)
        {
            throw new NotImplementedException();
        }

        public Task<(bool, object)> WaitForReturnValueAsync(Guid awaitingJobId, Guid jobToAwaitId)
        {
            throw new NotImplementedException();
        }
    }
}
