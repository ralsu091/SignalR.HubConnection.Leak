using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace SignalR.HubConnection.Leak
{
    public class Subscriber
    {
        public Subscriber(Action<EventDataEventArgs> onEventMessageAction)
        {
            if (onEventMessageAction != null)
            {
                OnEventMessageAction = onEventMessageAction;
            }
        }
        public Microsoft.AspNet.SignalR.Client.HubConnection HubConnection { get; set; }
        public IHubProxy HubProxy { get; set; }
        private Action<EventDataEventArgs> OnEventMessageAction { get; set; }

        public async Task<bool> ConnectForSignalAsync(string hubName, TextWriter traceOutput = null)
        {
            
            HubConnection = new Microsoft.AspNet.SignalR.Client.HubConnection("http://serverdown.bogus.address.com", new Dictionary<string, string> { { "bearerToken", "Random" } });
            
            if (traceOutput != null)
            {
                HubConnection.TraceLevel = TraceLevels.All;

                HubConnection.TraceWriter = traceOutput;
            }
            
            HubProxy = HubConnection.CreateHubProxy(hubName);

           HubProxy.On<object>("OnEventMessage",  OnEventMessage);
            try
            {
                await HubConnection.Start();
                return true;
            }
            catch (HttpRequestException e)
            {
                return false;
            }
        }
        
        public void DisconnectFromSignal(TimeSpan? timeout = null)
        {
            if (HubConnection == null)
                throw new NullReferenceException("HubConnection is null.  Did you forget to call ConnectForSignalAsync");
            if (timeout.HasValue)
            {
                HubConnection.Stop(timeout.Value);
                HubConnection.Dispose();
               
            }
            else
            {
                HubConnection.Stop();
                HubConnection.Dispose();
            }
            OnEventMessageAction = null;
         
            HubConnection = null;
            HubProxy = null;
        }


        public async Task<string> WhoAmI()
        {
            var result = await HubProxy.Invoke<string>("WhoAmI");
            return result;
        }
        public async Task<string> WhoAreYou()
        {
            var result = await HubProxy.Invoke<string>("WhoAreYou");
            return result;
        }

        private void OnEventMessage(object data)
        {
            OnEventMessageAction?.Invoke(new EventDataEventArgs {Data = data});

        }
    }
    public class EventDataEventArgs : EventArgs
    {
        public object Data { get; set; }
    }
}
