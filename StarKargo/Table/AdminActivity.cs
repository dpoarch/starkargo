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
using Android.Net;
using LogIn;
using StarKargo.Model;
using System.Security.Cryptography;

namespace StarKargo
{
   
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class AdminActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Admin);

            //GetConnectionStatus();
            //Initializing button from layout
            Button recPackage = FindViewById<Button>(Resource.Id.btnReceivePackage);
            Button loadContainer = FindViewById<Button>(Resource.Id.btnLoadContainer);
            Button unloadContainer = FindViewById<Button>(Resource.Id.btnUnloadContainer);
            Button delPackage = FindViewById<Button>(Resource.Id.btnDeliverPackage);
            Button lookUpPackage = FindViewById<Button>(Resource.Id.btnLookupPackage);
            Button manageUsers = FindViewById<Button>(Resource.Id.btnUsers);
            Button pending = FindViewById<Button>(Resource.Id.btnPending);
            ImageButton logout = FindViewById<ImageButton>(Resource.Id.ImgBtnLogout);
            ImageButton changePwd = FindViewById<ImageButton>(Resource.Id.ImgBtnChangePwd);

            // Receive Package
            recPackage.Click += (object sender, EventArgs e) =>
            {
                var recPackageActivity = new Intent(this, typeof(ReceivePackageActivity));
                StartActivity(recPackageActivity);

            };

            // Load Container
            loadContainer.Click += (object sender, EventArgs e) =>
            {
                var loadContainerActivity = new Intent(this, typeof(LoadContainerActivity));
                StartActivity(loadContainerActivity);

            };

            // UnLoad Container
            unloadContainer.Click += (object sender, EventArgs e) =>
            {

                var unloadContainerActivity = new Intent(this, typeof(UnloadContainerActivity));
                StartActivity(unloadContainerActivity);
            };

            // Deliver Package
            delPackage.Click += (object sender, EventArgs e) =>
            {
                var deliveryPackageActivity = new Intent(this, typeof(DeliverPackageActivity));
                StartActivity(deliveryPackageActivity);

            };

            // LookUp Package
            lookUpPackage.Click += (object sender, EventArgs e) =>
            {
                var lookupPackageActivity = new Intent(this, typeof(CheckPackageActivity));
                StartActivity(lookupPackageActivity);
            };

            // Users
            manageUsers.Click += (object sender, EventArgs e) =>
            {
                var manageUserActivity = new Intent(this, typeof(ManageUserActivity));
                StartActivity(manageUserActivity);
            };

            // Pending
            pending.Click += (object sender, EventArgs e) =>
            {
                var pendingActivity = new Intent(this, typeof(PendingActivity));
                StartActivity(pendingActivity);
            };

            // LogOut
            logout.Click += (object sender, EventArgs e) =>
            {
                UserSession.SaveUserCredentials("", "", 0);
                this.FinishAffinity();
               // System.Environment.Exit(0);
                //var loginActivity = new Intent(this, typeof(LoginActivity));
                //StartActivity(loginActivity);
            };

            // Change Password
            changePwd.Click += (object sender, EventArgs e) =>
            {
                var changePasswordActivity = new Intent(this, typeof(ChangePasswordActivity));
                StartActivity(changePasswordActivity);
            };
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