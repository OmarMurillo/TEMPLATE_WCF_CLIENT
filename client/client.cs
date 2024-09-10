//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Samples.MessageInterceptor
{
    [ServiceContract]
    public interface ISampleContract
    {
        [OperationContract(IsOneWay = true)]
        void ReportWindSpeed(XElement speed);
    }

    public class MessageModifier : ChannelMessageInterceptor
    {
        int send = 0;

        public override void OnSend(ref Message msg)
        {
            Console.WriteLine("Hola");
          
            Console.WriteLine("Urn interceptor");
            send++;
            if (send % 2 == 0)
            {
                // Add extra header so that when the message arrives on service side, the message won't be dropped
              
                msg.Headers.Add(MessageHeader.CreateHeader("ByPass", "urn:InterceptorNamespace", "ByPassPassword"));
       
            }
            msg.Headers.Add(MessageHeader.CreateHeader("ByPass", "urn:InterceptorNamespace", "ByPassPassword"));
            msg.Headers.To = new Uri("urn:Interceptor");
            Console.WriteLine(msg.Headers.To);
        }

        public override ChannelMessageInterceptor Clone()
        {
            return new MessageModifier();
        }
    }

    class MessageModifierElement : InterceptingElement
    {
        protected override ChannelMessageInterceptor CreateMessageInterceptor()
        {
            return new MessageModifier();
        }
    }

    class MessageModifierImporter : InterceptingBindingElementImporter
    {
        protected override ChannelMessageInterceptor CreateMessageInterceptor()
        {
            return new MessageModifier();
        }
    }

    class Client
    {
        static void Main(string[] args)
        {
            ChannelFactory<ISampleContract> channelFactory = new ChannelFactory<ISampleContract>("sampleProxy");
            ISampleContract proxy = channelFactory.CreateChannel();

            int[] windSpeeds = new int[] { 100, 90, 80, 70, 60, 50, 40, 30, 20, 10 };

            Console.WriteLine("Reporting the next 10 wind speeds.");
            for (int i = 0; i < 1; i++)
            {
                Console.WriteLine(windSpeeds[i] + " kph");
                try
                {
                    XElement element = new XElement("orders", 100);
                    proxy.ReportWindSpeed(element);
                }
                catch (CommunicationException)
                {
                    Console.WriteLine("Server dropped a message.");
                }
            }

            Console.WriteLine("Press ENTER to shut down client");
            Console.ReadLine();

            ((IChannel)proxy).Close();
            channelFactory.Close();
        }
    }
}
