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
using LogIn;
using StarKargo.Model;
using Android.Net;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class WarehouseActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Warehouse);

            //GetConnectionStatus();
            //Initializing button from layout
            Button recPackage = FindViewById<Button>(Resource.Id.btnReceivePackage);
            Button loadContainer = FindViewById<Button>(Resource.Id.btnLoadContainer);
            Button lookUpPackage = FindViewById<Button>(Resource.Id.btnLookupPackage);
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

            // LookUp Package
            lookUpPackage.Click += (object sender, EventArgs e) =>
            {

                var lookupPackageActivity = new Intent(this, typeof(CheckPackageActivity));
                StartActivity(lookupPackageActivity);
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