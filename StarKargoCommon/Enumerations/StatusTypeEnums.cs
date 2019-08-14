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
using StarKargoCommon.Helpers;

namespace StarKargoCommon.Enumerations
{
    public enum StatusTypeEnums
    {
        [StringValue("RECEIVED")]
        RECEIVED = 0,
        [StringValue("LOADED")]
        LOADED = 1,
        [StringValue("SHIPPED")]
        SHIPPED = 2,
        [StringValue("UNLOADED")]
        UNLOADED = 3,
        [StringValue("DELIVERED")]
        DELIVERED = 4
    }
}