using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNet.SignalR;

namespace SeattleTimes.Hubs
{
    public class SeattleTimesHub : Hub
    {
        private static DateTime _lastUpdate;
        private static IList<object> _lastNews;

        internal static async Task SyncOutOfHub()
        {
            _lastUpdate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(24));
            _lastNews = new List<object>();
            var context = GlobalHost.ConnectionManager.GetHubContext<SeattleTimesHub>();

            while (true)
            {
                var items = await GetRecentNews();
                context.Clients.All.addNews(items);
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private static async Task<IList<object>> GetRecentNews()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://seattletimes.com/rss/home.xml");
            var stream = await response.Content.ReadAsStreamAsync();
            var xml = XDocument.Load(stream);

            var items = (from c in xml.Descendants("item")
                        where ParsePubDate((string)c.Element("pubDate")) > _lastUpdate
                        select new
                        {
                            title = (string)c.Element("title"),
                            link = (string)c.Element("link"),
                            description = (string)c.Element("description"),
                            category = (string)c.Element("category"),
                        }).ToList<object>();
            
            _lastUpdate = DateTime.UtcNow;
            
            if(items.Count > 0)
            {
                _lastNews = items;
            }

            return items;
        }

        private static DateTime ParsePubDate(string date)
        {
            const string FORMAT = "ddd, d MMM yyyy HH:mm:ss zzz";
            date = date.Replace(" PDT", " -08:00");
            var dateTime = DateTime.ParseExact(date, FORMAT, CultureInfo.InvariantCulture);
            var dateTimeUtc = dateTime.ToUniversalTime();
            return dateTimeUtc;
        }

        public Task Sync()
        {
            return Clients.Caller.addNews(_lastNews);
        }
    }
}