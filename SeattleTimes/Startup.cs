using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using SeattleTimes.Hubs;

[assembly: OwinStartup(typeof(SeattleTimes.Startup))]

namespace SeattleTimes
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            SeattleTimesHub.SyncOutOfHub();
        }
    }
}
