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
using StarKargoCommon.Enumerations;
using StarKargo.Model;
using StarKargoCommon.Models;
using Newtonsoft.Json;
using StarKargoService.UserService;
using StarKargoCommon.Helpers;
using Android.Net;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class ManageUserActivity : Activity
    {
        IUserService _userService = new UserService();

        List<UserTableItem> userTableItems = new List<UserTableItem>();
        ListView listView;

        bool executeOnce = false;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ManageUser);
            LoadTableItems();
            InitializeListView();

            Button doneAction = FindViewById<Button>(Resource.Id.btnDone);
            Button addUserAction = FindViewById<Button>(Resource.Id.btnAddUser);

            // Add User 
            addUserAction.Click += (object sender, EventArgs e) =>
            {
                var createActivity = new Intent(this, typeof(CreateUserActivity));
                StartActivity(createActivity);
            };

            // Done
            doneAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };
        }

        private void InitializeListView()
        {
            // Initialize the ListView with the data first.
            listView = FindViewById<ListView>(Resource.Id.listViewUsers);
            listView.Adapter = new UserHomeScreenAdapter(this, userTableItems);

            // Important - Set the ChoiceMode
            listView.ChoiceMode = ChoiceMode.Single;

            listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {

                if (!executeOnce)
                {
                    var t = userTableItems[e.Position];

                    UserModel model = new UserModel();
                    model.Name_First = t.FirstName;
                    model.Name_Last = t.LastName;
                    model.Email = t.Email;
                    model.GUID = t.GUID;
                    model.Role = t.Role;
                    model.Password = t.Password;
                    var updateUserActivity = new Intent(this, typeof(UpdateUserActivity));
                    updateUserActivity.PutExtra("User", JsonConvert.SerializeObject(model));
                    StartActivity(updateUserActivity);
                }
            };
        }

        private void LoadTableItems()
        {
            if(!GetConnectionStatus())
            {
                Android.Widget.Toast.MakeText(this, "No Connection!", Android.Widget.ToastLength.Short).Show();
                return;
            }

            try
            {
                // Call API 
                var users = _userService.GetUsers();

                foreach (var user in users)
                {
                    // Add Item on List
                    userTableItems.Add(new UserTableItem
                    {
                        Heading = string.Format("{0} {1}", user.Name_First, user.Name_Last),
                        SubHeading = Utils.GetRoleTypeValue(user.Role),
                        FirstName = user.Name_First,
                        LastName = user.Name_Last,
                        Email = user.Email,
                        Role = user.Role,
                        Location = user.Location,
                        LocationStr = user.LocationStr,
                        GUID = user.GUID,
                        Password = user.Password
                    });
                }
            }catch(Exception ex)
            {
                Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
            }

        }

        public bool GetConnectionStatus()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

            return isOnline;
        }
    }
}