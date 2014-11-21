﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac;
using Autofac.Integration.Wcf;
using PersistentLayer.Domain;
using WcfJsonFormatter;
using WcfJsonFormatter.Ns;
using Newtonsoft.Json;
using System.Net;

namespace WcfJsonService.Example
{
    public class WcfHost
    {
        static void Main()
        {
            WcfHost host = new WcfHost();
            
            //host.Initialize();
            //host.Run();

            host.RunServiceWithWebRequest();
        }

        private void Initialize()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterInstance("string dependency");
            
            builder.Register(n => 1)
                   .AsSelf();

            builder.RegisterType<SalesService>()
                   .As<ISalesService>();

            AutofacHostFactory.Container = builder.Build();

        }

        private void Run()
        {
            Console.WriteLine("Run a ServiceHost via programmatic configuration...");
            Console.WriteLine();

            string baseAddress = "http://" + Environment.MachineName + ":8000/Service.svc";

            using (ServiceHost serviceHost = new ServiceHost(typeof(SalesService), new Uri(baseAddress)))
            {
                WebHttpBinding webBinding = new WebHttpBinding
                {
                    ContentTypeMapper = new RawContentMapper(),
                    MaxReceivedMessageSize = 4194304,
                    MaxBufferSize = 4194304
                };

                serviceHost.AddServiceEndpoint(typeof(ISalesService), webBinding, "json")
                    .Behaviors.Add(new WebHttpJsonNetBehavior());

                serviceHost.AddServiceEndpoint(typeof(ISalesService), new BasicHttpBinding(), baseAddress);
                serviceHost.AddDependencyInjectionBehavior<ISalesService>(AutofacHostFactory.Container);

                Console.WriteLine("Opening the host");
                serviceHost.Open();

                try
                {
                    AutofacHostFactory.Container.Resolve<ISalesService>();
                    Console.WriteLine("The service is ready.");
                    Console.WriteLine("Press <ENTER> to terminate service.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on initializing the service host.");
                    Console.WriteLine(ex.Message);
                }
                
                Console.WriteLine();
                Console.ReadLine();

                serviceHost.Close();
            }
        }


        public void RunServiceWithWebRequest()
        {
            WebHttpBinding webBinding = new WebHttpBinding
            {
                ContentTypeMapper = new RawContentMapper(),
                MaxReceivedMessageSize = 4194304,
                MaxBufferSize = 4194304
            };

            string baseAddress = "http://" + Environment.MachineName + ":8000/Service/jargs";
            Uri uriBase = new Uri(baseAddress);

            ServiceHost host = new ServiceHost(typeof(Service), uriBase);
            host.AddServiceEndpoint(typeof(ITest), webBinding, uriBase)
                            .Behaviors.Add(new WebHttpJsonNetBehavior());
            
            host.Open();
            Console.WriteLine("Host opened");

            WebClient client = null;
            
            //client = new WebClient();
            //client.Headers[HttpRequestHeader.ContentType] = "application/json";
            //Console.WriteLine(client.UploadString(baseAddress + "/InsertData", "{param1:{\"FirstName\":\"John\",\"LastName\":\"Doe\"}}"));
            
            client = new WebClient();
            Console.WriteLine(client.DownloadString(baseAddress + "/InsertData2?param1={\"FirstName\":\"John\",\"LastName\":\"Doe\"},str=\"string test\""));

            Console.WriteLine("new calling...");
            Console.WriteLine();

            client = new WebClient();
            Console.WriteLine(client.DownloadString(baseAddress + "/InsertData3?param1={\"FirstName\":\"John\",\"LastName\":\"Doe\"},str=\"string test\""));
            Console.WriteLine();

            Console.Write("Press ENTER to close the host");
            Console.ReadLine();
            host.Close();
        }
    }
}
