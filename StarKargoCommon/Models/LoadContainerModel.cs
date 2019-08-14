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
    public  class LoadContainerModel
    {
        //public string LotNo { get; set; }
        //public int LotNoID { get; set; }
        //public string Barcode { get; set; }
        //public List<ScanPackageModel> ScanPackages { get; set; }

        public bool UpdateTS { get; set; }
        public ModelOrder ModelOrder { get; set; }
    }
}