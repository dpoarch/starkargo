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
using SQLite;

namespace StarKargo.Table
{
    public class UserTable
    {
        [PrimaryKey]
        public Guid GUID { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string WarehouseLocationId { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }
        public Guid? Location { get; set; }
        public string LocationStr { get; set; }
    }
}