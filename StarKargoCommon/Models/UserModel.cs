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
    public class UserModel
    {
        public Guid GUID { get; set; }
        public DateTime? DT_DELETED { get; set; }
        
        public string Email { get; set; }
        public string Name_First { get; set; }
        public string Name_Last { get; set; }
        public int UserID { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public int Role { get; set; }
        public string RoleStr { get; set; }
        public Guid? Location { get; set; }
        public string LocationStr { get; set; }
    }
}