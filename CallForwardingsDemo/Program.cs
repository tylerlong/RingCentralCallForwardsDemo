using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RingCentral;

namespace CallForwardingsDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
               using (var rc = new RestClient(
                   Environment.GetEnvironmentVariable("RINGCENTRAL_CLIENT_ID"),
                   Environment.GetEnvironmentVariable("RINGCENTRAL_CLIENT_SECRET"),
                   Environment.GetEnvironmentVariable("RINGCENTRAL_SERVER_URL")
               ))
               {
                   await rc.Authorize(
                       Environment.GetEnvironmentVariable("RINGCENTRAL_USERNAME"),
                       Environment.GetEnvironmentVariable("RINGCENTRAL_EXTENSION"),
                       Environment.GetEnvironmentVariable("RINGCENTRAL_PASSWORD")
                   );
                   Console.WriteLine(Environment.GetEnvironmentVariable("RINGCENTRAL_USERNAME"));
                   Console.WriteLine(rc.token.access_token);
                   
                   var eventFilters = new[]
                   {
                       "/restapi/v1.0/account/~/extension/~/presence?detailedTelephonyState=true"
                   };
                   var subscription = new Subscription(rc, eventFilters, message =>
                   {
                       var presenceEvent = JsonConvert.DeserializeObject<DetailedExtensionPresenceEvent>(message);
                       Console.WriteLine(presenceEvent.body.activeCalls);
                       foreach (var activeCall in presenceEvent.body.activeCalls)
                       {
                           if (activeCall.telephonyStatus == "Ringing")
                           {
                               Console.WriteLine("Here I want to forward it!");
                           }
                       }
                   });
                   var subscriptionInfo = await subscription.Subscribe();
                   Console.WriteLine(subscriptionInfo.id);
                   await Task.Delay(999999999); // never exit, keep listening
               }
            }).GetAwaiter().GetResult();
        }
    }
}