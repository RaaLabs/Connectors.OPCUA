// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Threading;

namespace RaaLabs.Edge.Connectors.OPCUA
{
    public class OPCUAClient
    {
        public string ServerAddress { get; set; }
        public string ServerPortNumber { get; set; }
        public string MyApplicationName { get; set; }
        public Session OPCSession { get; set; }
        public bool SessionRenewalRequired { get; set; }
        public double SessionRenewalPeriodMins { get; set; }
        public DateTime LastTimeSessionRenewed { get; set; }
        public DateTime LastTimeOPCServerFoundAlive { get; set; }
        public bool ClassDisposing { get; set; }
        private Thread RenewerTHread { get; set; }
        
        public OPCUAClient(string serverAddres, string serverport, bool sessionrenewalRequired, double sessionRenewalMinutes, string nameSpace)
        {
            ServerAddress = serverAddres;
            ServerPortNumber = serverport;
            MyApplicationName = "MyApplication";
            SessionRenewalRequired = sessionrenewalRequired;
            SessionRenewalPeriodMins = sessionRenewalMinutes;
            LastTimeOPCServerFoundAlive = DateTime.Now;
            InitializeOPCUAClient();

            if (SessionRenewalRequired)
            {
                LastTimeSessionRenewed = DateTime.Now;
                RenewerTHread = new Thread(renewSessionThread);
                RenewerTHread.Start();
            }
        }

        private void renewSessionThread()
        {
            while (!ClassDisposing)
            {
                if ((DateTime.Now - LastTimeSessionRenewed).TotalMinutes > SessionRenewalPeriodMins
                    || (DateTime.Now - LastTimeOPCServerFoundAlive).TotalSeconds > 60)
                {
                    Console.WriteLine("Renewing Session");
                    try
                    {
                        OPCSession.Close();
                        OPCSession.Dispose();
                    }
                    catch { }
                    InitializeOPCUAClient();
                    LastTimeSessionRenewed = DateTime.Now;

                }
                Thread.Sleep(60000);
            }
        }

        public void InitializeOPCUAClient()
        {
            Console.WriteLine("Step 1 - Create application configuration and certificate.");
            var config = new ApplicationConfiguration()
            {
                ApplicationName = MyApplicationName,
                ApplicationUri = Utils.Format(@"urn:{0}:" + MyApplicationName + "", ServerAddress),
                ApplicationType = ApplicationType.Client,
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration()
            };

            var application = new ApplicationInstance
            {
                ApplicationName = MyApplicationName,
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };

            string serverAddress = ServerAddress;
            var selectedEndpoint = CoreClientUtils.SelectEndpoint("opc.tcp://" + serverAddress + ":" + ServerPortNumber + "/OPCUA/SimulationServer", useSecurity: false);

            Console.WriteLine($"Step 2 - Create a session with your server: {selectedEndpoint.EndpointUrl} ");
            OPCSession = Session.Create(config, new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)), false, "", 60000, null, null).GetAwaiter().GetResult();
            {
                Console.WriteLine("Step 4 - Create a subscription. Set a faster publishing interval if you wish.");
                var subscription = new Subscription(OPCSession.DefaultSubscription) { PublishingInterval = 1000 };

                Console.WriteLine("Step 5 - Add a list of items you wish to monitor to the subscription.");
                var list = new List<MonitoredItem> { };
                list.Add(new MonitoredItem(subscription.DefaultItem) { DisplayName = "Random", StartNodeId = "ns=3;i=1002" });
                list.Add(new MonitoredItem(subscription.DefaultItem) { DisplayName = "Counter", StartNodeId = "ns=3;i=1001" });

                list.ForEach(i => i.Notification += OnTagValueChange);
                subscription.AddItems(list);

                Console.WriteLine("Step 6 - Add the subscription to the session.");
                OPCSession.AddSubscription(subscription);
                subscription.Create();
            }
        }

        public void OnTagValueChange(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in item.DequeueValues())
            {
                if (item.DisplayName == "ServerStatusCurrentTime")
                {
                    LastTimeOPCServerFoundAlive = value.SourceTimestamp.ToLocalTime();
                }
                else
                {
                    if (value.Value != null)
                        Console.WriteLine("{0}: {1}, {2}, {3}", item.DisplayName, value.Value.ToString(), value.SourceTimestamp.ToLocalTime(), value.StatusCode);
                    else
                        Console.WriteLine("{0}: {1}, {2}, {3}", item.DisplayName, "Null Value", value.SourceTimestamp, value.StatusCode);
                }
            }
        }
    }
}
