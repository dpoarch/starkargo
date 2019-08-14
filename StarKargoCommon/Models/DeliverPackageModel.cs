using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace StarKargoCommon.Models
{
    public class DeliverPackageModel
    {
        public bool UpdateTS { get; set; }
        public ModelOrder ModelOrder { get; set; }
    }
}