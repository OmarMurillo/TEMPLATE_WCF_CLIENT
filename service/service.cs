//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.Samples.MessageInterceptor
{
    // Create a service contract and define the service operations.
    [ServiceContract]
    public interface ISampleContract
    {
        [OperationContract(IsOneWay = true, Action = "http://company_fake/http_endpoint")]
        void ReportWindSpeed(XmlElement speed);
    }

    // The Service implementation implements your service contract.
    public class SampleService : ISampleContract
    {
        public void ReportWindSpeed(XmlElement speed)
        {
            Console.WriteLine(speed + " --------------------- ");
           
        }
    }

    class DroppingServerElement : InterceptingElement
    {
        protected override ChannelMessageInterceptor CreateMessageInterceptor()
        {
            return new DroppingServerInterceptor();
        }
    }

    public class DroppingServerInterceptor : ChannelMessageInterceptor
    {
        int messagesSinceLastReport = 0;
        readonly int reportPeriod = 5;

        public DroppingServerInterceptor() { }

        public override void OnReceive(ref Message msg)
        {

            msg.Headers.To = new Uri("urn:SAP:");
            //Console.WriteLine(msg.ToString());  
            Console.WriteLine(msg.Headers.To);
          
            if(msg.Headers.Action == "http://tempuri.org/ISampleContract/ReportWindSpeed")
            {
                msg.Headers.Action = "http://company_fake/http_endpoint";
            }
            msg.Headers.To = new Uri("http://localhost:8000/windspeed/");
            Console.WriteLine(msg.Headers.Action);
            return;
            if (msg.Headers.To.ToString() == "urn:SAP:")
            {
                Console.WriteLine(reportPeriod + " x2 wind speed reports have been received.");
                return;
            }
           
            // Drop incoming Message if the Message does not have the special header
            msg = null;
        }

        public override ChannelMessageInterceptor Clone()
        {
            return new DroppingServerInterceptor();
        }
    }

    class Service
    {
        static void Main(string[] args)
        {
            ServiceHost serviceHost = new ServiceHost(typeof(SampleService), new Uri("http://localhost:8000/windspeed/"));
            bool success = false;

            try
            {
                serviceHost.Open();

                System.Console.WriteLine("Press ENTER to exit.");
                System.Console.ReadLine();

                serviceHost.Close();
                success = true;
            }
            finally
            {
                if (!success)
                    serviceHost.Abort();
            }
        }
    }
}
