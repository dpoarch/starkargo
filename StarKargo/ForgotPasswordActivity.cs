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
using StarKargo.Model;
using SQLite;
using StarKargo.Table;
using StarKargoService.UserService;
using StarKargoCommon.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class ForgotPasswordActivity : Activity
    {
        private static string FROM_EMAIL = "davidpdeveloper2@gmail.com";
        private static string EMAIL_PWD = "Baboy22!";

        IUserService _userService = new UserService();

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ForgotPassword);

            // Create your application here
            Button cancelAction = FindViewById<Button>(Resource.Id.btnCancel);
            Button submitAction = FindViewById<Button>(Resource.Id.btnSubmit);


            // Cancel
            cancelAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };

            // Submit
            submitAction.Click += (object sender, EventArgs e) =>
            {
                EditText txtEmail = FindViewById<EditText>(Resource.Id.txtEmail);

                var email = txtEmail.Text;
                if (String.IsNullOrEmpty(email))
                {
                    Android.Widget.Toast.MakeText(this, "Missing  Email Address!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                // check if email is registered
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);
                var data = db.Table<UserTable>();
                var userDataEntity = data.Where(x => x.Email == email).FirstOrDefault(); //Linq Query  

                if (userDataEntity == null)
                {
                    Android.Widget.Toast.MakeText(this, "Email Address Not Found!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                // generate random code
                var randomCode = RandomString(8);

                // hash email/username and password
                var newPwd = Utils.GeneratePassword(email, randomCode);

                // send email
                var isSuccess =  SendEmail(userDataEntity.FirstName, email, randomCode);

                if (isSuccess)
                {
                    // update local info
                    userDataEntity.Password = newPwd;
                    db.Update(userDataEntity);

                    ChangePasswordModel model = new ChangePasswordModel();
                    model.GUID = userDataEntity.GUID;
                    model.Password = userDataEntity.Password;
                    model.NewPassword = newPwd;
                    bool retVal = false;

                    try
                    {
                        // call api
                        retVal = _userService.ChangePassword(model);


                        var users = _userService.GetUsers();
                    }
                    catch (Exception ex)
                    {
                        Android.Widget.Toast.MakeText(this, " Error!", Android.Widget.ToastLength.Short).Show();
                        return;
                    }

                    // display suceess 
                    Android.Widget.Toast.MakeText(this, "Successful!", Android.Widget.ToastLength.Short).Show();

                    Finish();
                }else
                {
                    Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                }
            };
        }

        private static bool SendEmail(string fName, string toEmail, string newPwdCode)
        {
            bool isSuccess = false;

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
               // SmtpClient SmtpServer = new SmtpClient("shopnzip-com.mail.protection.outlook.com");
                mail.From = new MailAddress(FROM_EMAIL, "do not reply");
                // mail.To.Add("davidpdeveloper2@gmail.com");
                mail.To.Add(toEmail);
                mail.Subject = "Reset Password";
                mail.Body = "<html><body><p>Hi <b>" + fName + "</b>,</p><br/>" +
                          "<p>Your new password is :</p> <b> " + newPwdCode + " </b>" +
                          "<br/><br/>" +
                          "<p>Thanks,</p><p>Admin</p></body></html>";
                mail.IsBodyHtml = true;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential(FROM_EMAIL, EMAIL_PWD);
                SmtpServer.EnableSsl = true;
                ServicePointManager.ServerCertificateValidationCallback = delegate (object sender1, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
                SmtpServer.Send(mail);
                Toast.MakeText(Application.Context, "Mail Send Sucessufully", ToastLength.Short).Show();
                isSuccess = true;
            }

            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.ToString(), ToastLength.Long);
            }

            return isSuccess;
        }
    }
}