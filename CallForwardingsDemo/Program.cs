using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RingCentral;

namespace CallForwardingsDemo
{
    class NotificationPayload
    {
        public NotificationPayloadBody body;
    }

    class NotificationPayloadBody
    {
        public ActiveCall[] activeCalls;
    }

    class ActiveCall
    {
        public string id;
        public string direction;
        public string from;
        public string to;
        public string toName;
        public string startTime;
        public string telephonyStatus;
        public string sessionId;
        public string partyId;
        public string telephonySessionId;
    }
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
                   var subscription = new Subscription(rc, eventFilters, async message =>
                   {
                       var payload = JsonConvert.DeserializeObject<NotificationPayload>(message);
                       Console.WriteLine(payload.body.activeCalls);
                       foreach (var activeCall in payload.body.activeCalls)
                       {
                           if (activeCall.telephonyStatus == "Ringing" && activeCall.direction =="Inbound")
                           {
                               Console.WriteLine("Here I want to forward it!");
                               var r = await rc.Restapi().Account().Telephony().Sessions(activeCall.telephonySessionId).Parties(activeCall.partyId).Forward().Post(new ForwardTarget
                               {
                                   phoneNumber = "+" + Environment.GetEnvironmentVariable("RINGCENTRAL_CALLEE")
                               });
                               Console.WriteLine(r.id);
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