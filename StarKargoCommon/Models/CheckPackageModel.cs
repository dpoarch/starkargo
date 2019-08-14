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
    public class CheckPackageModel
    {
        public int PackageQty { get; set; }
        public string OrderNumber { get; set; }
        public int Status { get; set; }


        // ADDED
        public Guid GUID { get; set; }
        public string AgentStr { get; set; }
        public string LotNoStr { get; set; }
        public string DeliveredTo { get; set; }
        public DateTime? ReceivedTS { get; set; }
        public DateTime? LoadedTS { get; set; }
        public DateTime? UnloadedTS { get; set; }
        public DateTime? DeliveredTS { get; set; }
        public Guid? Agent { get; set; }
        public Guid? LotNo { get; set; }
    }
}