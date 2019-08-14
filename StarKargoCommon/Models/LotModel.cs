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
    public class LotModel
    {
        public Guid GUID { get; set; }
        public int Type { get; set; }
        public string Value { get; set; }
    }
}