using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedJobQueue.RabbitMq
{
    public class RabbitMqJobConsumer : IBasicConsumer
    {
        public IModel Model => throw new NotImplementedException();

        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;

        public void HandleBasicCancel(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            throw new NotImplementedException();
        }

        public void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
            throw new NotImplementedException();
        }
    }
}
