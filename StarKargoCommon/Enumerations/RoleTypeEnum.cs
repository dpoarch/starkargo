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
    public enum RoleTypeEnums
    {
        [StringValue("Administrator")]
        Administrator = 0,

        [StringValue("Warehouse")]
        Warehouse = 1,

        [StringValue("Manila")]
        Manila = 2
    }
}