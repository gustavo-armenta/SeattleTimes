using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Microsoft.AspNet.SignalR.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace AndroidClient
{
    [Activity(Label = "Seattle Times", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        private SynchronizationContext _context;
        private LinearLayout _layout;
        private HubConnection _connection;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var scrollView = new ScrollView(this)
            {
                LayoutParameters =
                    new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            this.SetContentView(scrollView);

            _context = SynchronizationContext.Current;
            _layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters =
                    new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            scrollView.AddView(_layout);            

            _connection = new HubConnection("http://signalr-xamarin.azurewebsites.net/");
            ConfigureLogging(false);

            this.RunAsync();
        }

        private void ConfigureLogging(bool enabled)
        {
            if(!enabled)
            {
                return;
            }

            var logging = new TextView(this);
            logging.TextSize = 8.0f;
            _layout.AddView(logging, 0);

            var traceWriter = new TextViewWriter(_context, logging);
            _connection.TraceWriter = traceWriter;
        }

        private async Task RunAsync()
        {                        
            var hub = _connection.CreateHubProxy("SeattleTimesHub");
            hub.On<IList<News>>("addNews", (items) =>
            {
                _connection.TraceWriter.WriteLine("items.Count " + items.Count);
                foreach (var item in items)
                {
                    var description = new TextView(this);
                    description.TextSize = 8.0f;
                    description.Text = item.Description + System.Environment.NewLine;
                    _context.Post(delegate
                    {
                        _layout.AddView(description, 0);
                    }, state: null);
                    
                    var title = new TextView(this);
                    title.TextSize = 9.0f;
                    title.SetTypeface(null, TypefaceStyle.Bold);
                    title.Text = item.Title;                    
                    title.Click += (sender, e) =>
                    {
                        var uri = Android.Net.Uri.Parse(item.Link);
                        var intent = new Intent(Intent.ActionView, uri);
                        StartActivity(intent);
                    };

                    _context.Post(delegate
                    {
                        _layout.AddView(title, 0);
                    }, state: null);
                }
            });

            await _connection.Start();
            await hub.Invoke("Sync");
        }
    }
}

