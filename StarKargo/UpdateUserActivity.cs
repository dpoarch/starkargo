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
using StarKargo.Model;
using Newtonsoft.Json;
using StarKargoCommon.Models;
using StarKargoService.UserService;
using StarKargoCommon.Helpers;
using SQLite;
using Android.Net;
using StarKargo.Table;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class UpdateUserActivity : Activity
    {
        IUserService _userService = new UserService();

        UserModel userModel = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.UpdateUser);

            // Role ID
            Spinner spinnerRole = FindViewById<Spinner>(Resource.Id.spinRoleID);
            spinnerRole.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinnerRole_ItemSelected);
            var adapterRole = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.role_array, Resource.Layout.spinner_item);
            adapterRole.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerRole.Adapter = adapterRole;

            // Location ID
            Spinner spinnerLocation = FindViewById<Spinner>(Resource.Id.spinLocationID);
            var adapterLocation = new ArrayAdapter(this, Resource.Layout.spinner_item, UserSession.LocationsStringList);
            spinnerLocation.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinnerLocation_ItemSelected);
            adapterLocation.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerLocation.Adapter = adapterLocation;


            userModel = JsonConvert.DeserializeObject<UserModel>(Intent.GetStringExtra("User"));

            // Initialize UI
            InitializeUI(userModel);

            // Cancel Action
            Button cancelAction = FindViewById<Button>(Resource.Id.btnCancel);

            // Update Action  
            Button updateAction = FindViewById<Button>(Resource.Id.btnSave);

            // Delete Action
            Button deleteAction = FindViewById<Button>(Resource.Id.btnDelete);

            // Cancel Action
            cancelAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
                // Return to Manage User
                //var cancelActivity = new Intent(this, typeof(ManageUserActivity));
                //StartActivity(cancelActivity);
            };

            // Update Action
            updateAction.Click += (object sender, EventArgs e) =>
            {

                if(!GetConnectionStatus())
                {
                    Android.Widget.Toast.MakeText(this, " No Connection. Try later!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                EditText firstNameTxt = FindViewById<EditText>(Resource.Id.txtFirstName);
                EditText lastNameTxt = FindViewById<EditText>(Resource.Id.txtLastName);
                EditText emailTxt = FindViewById<EditText>(Resource.Id.txtEmail);
                EditText passwordTxt = FindViewById<EditText>(Resource.Id.txtPassword);

                userModel.Name_First = firstNameTxt.Text;
                userModel.Name_Last = lastNameTxt.Text;
                userModel.Email = emailTxt.Text;

                var newPassword = passwordTxt.Text;
                string pwd = string.Empty;
                // check password 
                if(!String.IsNullOrEmpty(newPassword))
                {
                    pwd = Utils.GeneratePassword(userModel.Email, newPassword);
                    userModel.Password = pwd;
                }

                var result = _userService.UpdateUser(userModel);
                // If typed password, overwrite existing password
                if (result)
                {
                    // update local data 
                    if (!String.IsNullOrEmpty(newPassword))
                    {
                        string dpPath = UserSession.DB_PATH;
                        var db = new SQLiteConnection(dpPath);
                        var data = db.Table<UserTable>(); //Call Table  
                        var userDataEntity = data.Where(x => x.GUID == userModel.GUID).FirstOrDefault(); //Linq Query  
                        userDataEntity.Password = pwd;
                        userDataEntity.Email = userModel.Email;
                        userDataEntity.FirstName = userModel.Name_First;
                        userDataEntity.LastName = userModel.Name_Last;
                        userDataEntity.Role = userModel.Role;
                        db.Update(userDataEntity);
                    }

                    Android.Widget.Toast.MakeText(this, " Successfully Updated!", Android.Widget.ToastLength.Short).Show();
                    Finish();
                }
            };

            // Delete Action
            deleteAction.Click += (object sender, EventArgs e) =>
            {
                if (!GetConnectionStatus())
                {
                    Android.Widget.Toast.MakeText(this, " No Connection. Try later!", Android.Widget.ToastLength.Short).Show();
                    return;
                }
    
                userModel.DT_DELETED = DateTime.UtcNow;
                var result = _userService.DeleteUser(userModel);

                if (result)
                {
                    Android.Widget.Toast.MakeText(this, " Successfully Deleted!", Android.Widget.ToastLength.Short).Show();
                    Finish();
                }
            };
        }

        void InitializeUI(UserModel userModel)
        {

            EditText firstNameTxt = FindViewById<EditText>(Resource.Id.txtFirstName);
            EditText lastNameTxt = FindViewById<EditText>(Resource.Id.txtLastName);
            EditText emailTxt = FindViewById<EditText>(Resource.Id.txtEmail);
            EditText passwordTxt = FindViewById<EditText>(Resource.Id.txtPassword);
            Spinner spinnerRole = FindViewById<Spinner>(Resource.Id.spinRoleID);
            Spinner spinnerLocation = FindViewById<Spinner>(Resource.Id.spinLocationID);

            firstNameTxt.Text = userModel.Name_First;
            lastNameTxt.Text = userModel.Name_Last;
            emailTxt.Text = userModel.Email;
            spinnerRole.SetSelection(userModel.Role);
        }

        private void spinnerRole_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            userModel.Role = e.Position;
            userModel.RoleStr = Convert.ToString(spinner.GetItemAtPosition(e.Position));
        }

        private void spinnerLocation_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            var locStr = Convert.ToString(spinner.GetItemAtPosition(e.Position));
            var locModel = UserSession.Locations.Where(x => x.Value == locStr).FirstOrDefault();
            if (locModel != null)
            {
                userModel.Location = locModel.GUID;
                userModel.LocationStr = Convert.ToString(spinner.GetItemAtPosition(e.Position));
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