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
using StarKargoCommon.Models;

namespace StarKargoCommon.Models
{
    public class ResponseCheckOrder
    {
        public string Message { get; set; }
        public bool Result { get; set; }
//        public List<OrderModel> Data { get; set; }
        public CheckPackageModel Data { get; set; }
    }
}