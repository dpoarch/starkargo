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
using StarKargo.Model;
using StarKargoService.UserService;
using System.Security.Cryptography;
using StarKargoCommon.Helpers;
using Android.Net;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class CreateUserActivity : Activity
    {
        UserModel userModel = new UserModel();

        IUserService _service = new UserService();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.CreateUser);

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


            // Cancel Action
            Button cancelAction = FindViewById<Button>(Resource.Id.btnCancel);

            // Save Action
            Button saveUserAction = FindViewById<Button>(Resource.Id.btnSave);

            // Cancel 
            cancelAction.Click += (object sender, EventArgs e) =>
            {
                // Return to Manage User
                Finish();
                //var cancelActivity = new Intent(this, typeof(ManageUserActivity));
                //StartActivity(cancelActivity);
            };

            // Save User 
            saveUserAction.Click += (object sender, EventArgs e) =>
            {
                if (!GetConnectionStatus())
                {
                    Android.Widget.Toast.MakeText(this, " No Connection. Try later!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                EditText firstNameTxt = FindViewById<EditText>(Resource.Id.txtFirstName);
                EditText lastNameTxt = FindViewById<EditText>(Resource.Id.txtLastName);
                EditText passwordTxt = FindViewById<EditText>(Resource.Id.txtPassword);
                EditText emailTxt = FindViewById<EditText>(Resource.Id.txtEmail);

                userModel.Name_First = firstNameTxt.Text;
                userModel.Name_Last = lastNameTxt.Text;
                userModel.Email = emailTxt.Text;
                userModel.Password = passwordTxt.Text;
                var pwd = Utils.GeneratePassword(userModel.Email, userModel.Password);
                userModel.Password = pwd;

                if (String.IsNullOrEmpty(userModel.LocationStr))
                {
                    userModel.Location = Guid.Empty;
                }

                if (Validation(userModel))
                {
                    var retVal = _service.CreateUser(userModel);

                    if (retVal)
                    {
                        Android.Widget.Toast.MakeText(this, "Successfully Added!", Android.Widget.ToastLength.Short).Show();
                        Finish();
                    }
                }
            };
        }

        private bool Validation(UserModel model)
        {
            bool isValid = false;

            if (String.IsNullOrEmpty(model.Name_First))
            {
                Android.Widget.Toast.MakeText(this, "Missing  First Name!", Android.Widget.ToastLength.Short).Show();
                isValid = false;
                return isValid;
            }

            if (String.IsNullOrEmpty(model.Name_Last))
            {
                Android.Widget.Toast.MakeText(this, " Missing  Last Name!", Android.Widget.ToastLength.Short).Show();
                isValid = false;
                return isValid;
            }

            if (String.IsNullOrEmpty(model.Email))
            {
                Android.Widget.Toast.MakeText(this, " Missing  Email!", Android.Widget.ToastLength.Short).Show();
                isValid = false;
                return isValid;
            }

            if (String.IsNullOrEmpty(model.Password))
            {
                Android.Widget.Toast.MakeText(this, " Missing  Password!", Android.Widget.ToastLength.Short).Show();
                isValid = false;
                return isValid;
            }

            isValid = true;

            return isValid;
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