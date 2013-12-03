using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace iPhoneClient
{
    public partial class NewsDialogViewController : DialogViewController
    {
        public Section Section { get; private set; }

        public NewsDialogViewController()
            : base(UITableViewStyle.Plain, null)
        {
            Section = new Section("Seattle Times");
            Root = new RootElement("NewsDialogViewController")
            {
                new Section(" "),
                Section
            };				
        }
    }
}