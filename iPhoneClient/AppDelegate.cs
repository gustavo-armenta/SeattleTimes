using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace iPhoneClient
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        private SynchronizationContext _context;
        private Section _layout;
        private HubConnection _connection;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            var controller = new NewsDialogViewController();
            window.RootViewController = controller;
            window.MakeKeyAndVisible();

            _context = SynchronizationContext.Current;
            _layout = controller.Section;
            _connection = new HubConnection("http://signalr-xamarin.azurewebsites.net/");
            ConfigureLogging(false);
            RunAsync();

            return true;
        }

        private void ConfigureLogging(bool enabled)
        {
            if (!enabled)
            {
                return;
            }

            var logging = new UITextView(new RectangleF(0, 35, 320, 500))
            {
                Font = UIFont.SystemFontOfSize(8f),
            };
            _context.Post(delegate
            {
                _layout.Add(logging);
            }, state: null);

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
                    var title = new StyledMultilineElement(item.Title, () =>
                    {
                        UIApplication.SharedApplication.OpenUrl(new NSUrl(item.Link));
                    })
                    {
                        Font = UIFont.BoldSystemFontOfSize(12f),
                    };

                    var description = new StyledMultilineElement(item.Description)
                    {
                        Font = UIFont.SystemFontOfSize(10f),
                    };

                    _context.Post(delegate
                    {
                        _layout.Add(title);
                        _layout.Add(description);
                    }, state: null);
                }
            });

            await _connection.Start();
            await hub.Invoke("Sync");
        }
    }
}