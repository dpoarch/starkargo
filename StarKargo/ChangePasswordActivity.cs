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
using StarKargoCommon.Models;
using StarKargo.Model;
using StarKargoService.UserService;
using StarKargoCommon.Helpers;
using SQLite;
using StarKargo.Table;
using Android.Net;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class ChangePasswordActivity : Activity
    {
        IUserService _userService = new UserService();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ChangePassword);

            Button cancelAction = FindViewById<Button>(Resource.Id.btnCancel);
            Button updatePwdAction = FindViewById<Button>(Resource.Id.btnUpdatePwd);

            // Cancel
            cancelAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };

            // Update Password
            updatePwdAction.Click += (object sender, EventArgs e) =>
            {

                if (!GetConnectionStatus())
                {
                    Android.Widget.Toast.MakeText(this, " No Connection. Try later!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                EditText existingPwdTxt = FindViewById<EditText>(Resource.Id.txtExistingPwd);
                EditText newPwdTxt = FindViewById<EditText>(Resource.Id.txtNewPwd);
                EditText confirmPwdTxt = FindViewById<EditText>(Resource.Id.txtConfirmPwd);

                // get data
                ChangePasswordModel model = new ChangePasswordModel
                {
                    Password = existingPwdTxt.Text,
                    NewPassword = newPwdTxt.Text,
                    ConfirmPassword = confirmPwdTxt.Text
                };

                if (Validation(model))
                {
                    var pwd = Utils.GeneratePassword(UserSession.Email, model.NewPassword);
                    model.GUID = UserSession.GUID;
                    model.Password = pwd;

                    bool retVal = false;

                    try
                    {
                        // call api
                        retVal = _userService.ChangePassword(model);
                    }
                    catch (Exception ex)
                    {
                        Android.Widget.Toast.MakeText(this, " Error!", Android.Widget.ToastLength.Short).Show();
                        return;
                    }

                    if (retVal)
                    {
                        // update local data 
                        string dpPath = UserSession.DB_PATH;
                        var db = new SQLiteConnection(dpPath);
                        var data = db.Table<UserTable>(); 
                        var userDataEntity = data.Where(x => x.GUID == model.GUID).FirstOrDefault(); //Linq Query  
                        userDataEntity.Password = pwd;
                        db.Update(userDataEntity);

                        // display message
                        Android.Widget.Toast.MakeText(this, " Successful!", Android.Widget.ToastLength.Short).Show();

                        // Call Finish
                        Finish();
                    }
                    else
                    {
                        Android.Widget.Toast.MakeText(this, " Error In Changing Password!", Android.Widget.ToastLength.Short).Show();
                    }

                }
            };
        }



        public bool GetConnectionStatus()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
            bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

            return isOnline;
        }

        bool Validation(ChangePasswordModel model)
        {
            bool result = false;

            if (String.IsNullOrEmpty(model.Password))
            {
                Android.Widget.Toast.MakeText(this, " Missing  Existing Password!", Android.Widget.ToastLength.Short).Show();
                return result;
            }

            if (String.IsNullOrEmpty(model.NewPassword))
            {
                Android.Widget.Toast.MakeText(this, " Missing  New Password!", Android.Widget.ToastLength.Short).Show();
                return result;
            }

            if (String.IsNullOrEmpty(model.ConfirmPassword))
            {
                Android.Widget.Toast.MakeText(this, " Missing Confirm Password!", Android.Widget.ToastLength.Short).Show();
                return result;
            }

            if(model.NewPassword != model.ConfirmPassword)
            {
                Android.Widget.Toast.MakeText(this, " New Password and  Confirm Password is not equal!", Android.Widget.ToastLength.Short).Show();
                return result;
            }

            result = true;
            return result;
        }
    }
}