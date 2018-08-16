using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Nito.AsyncEx;

namespace SignalR.HubConnection.Leak
{
    class Program
    {
        private static Subscriber Subscriber { get; set; }

        static void Main(string[] args)
        {
            AsyncContext.Run(() => MainAsync(args));
          
            Console.ReadLine();
        }

        static async Task MainAsync(string[] args)
        {
            await StartChangesListener();
        }

        private static async Task StartChangesListener()
        {
           
            try
            {
                await InitiateSubscriber();
            }
            catch (Exception e)
            {
            }
            Task.Run(()=>MonitorSubscriber());
        }

        private static async Task InitiateSubscriber()
        {
            try
            {
                if (Subscriber != null)
                {
                    try
                    {
                        Subscriber.DisconnectFromSignal(TimeSpan.FromSeconds(5));
                    }
                    catch (NullReferenceException e)
                    {

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                Subscriber = new Subscriber(AddToQueue);
                await Subscriber.ConnectForSignalAsync("EventsHub");



            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void AddToQueue(EventDataEventArgs e)
        {
           
        }

        private static async Task MonitorSubscriber()
        {
         
            while (true)
            {
               
                try
                {
                    var isDisconnected = Subscriber.HubConnection == null ||
                                         Subscriber.HubConnection.State == ConnectionState.Disconnected;
                    if (isDisconnected)
                    {
                     
                      
                        await InitiateSubscriber();
                    }
                    
                    else if (Subscriber.HubConnection.State == ConnectionState.Connected)
                    {
                        try
                        {
                            var whoAmI = await Subscriber.WhoAmI();
                            if (!whoAmI.StartsWith("For Connection", StringComparison.InvariantCultureIgnoreCase))
                            {
                               
                         
                                await InitiateSubscriber();
                            }
                            

                        }
                        catch (Exception e)
                        {
                            await InitiateSubscriber();
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(15));
                }
                catch (Exception exception)
                {
                  
                }
            }
        }
    }
}
