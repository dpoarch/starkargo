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
    public class ScanPackageModel
    {
        public string OrderNumber { get; set; }
        public int PackageCount { get; set; }
        public DateTime ReceivedTS { get; set; }
    }
}