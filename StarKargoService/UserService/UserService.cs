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
using System.Net;
using Newtonsoft.Json;
using StarKargoCommon.Helpers;

namespace StarKargoService.UserService
{
    public class UserService : IUserService
    {
        public bool CreateUser(UserModel userModel)
        {
            bool isResult = false;

            string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_USERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var jsonObj = JsonConvert.SerializeObject(userModel);
                    var dataString = client.UploadString(URL, jsonObj);
                    isResult = true;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public bool UpdateUser(UserModel userModel)
        {
            bool isResult = false;

            string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_USERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var jsonObj = JsonConvert.SerializeObject(userModel);
                    var dataString = client.UploadString(URL, jsonObj);
                    isResult = true;
 
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public bool DeleteUser(UserModel userModel)
        {
            bool isResult = false;
            string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_USERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var jsonObj = JsonConvert.SerializeObject(userModel);
                    var dataString = client.UploadString(URL, jsonObj);
                    isResult = true;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public bool ChangePassword(ChangePasswordModel userModel)
        {
            bool isResult = false;
            string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_USERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    var jsonObj = JsonConvert.SerializeObject(userModel);
                    var dataString = client.UploadString(URL, jsonObj);
                    isResult = true;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public List<UserModel> GetUsers()
        {
            List<UserModel> retVal = null;

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);

                    string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.GET_USERS);
                    var response = client.DownloadString(URL);
                    ResponseUser account = JsonConvert.DeserializeObject<ResponseUser>(response);
                    retVal = account.Data;

                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return retVal;
        }
    }
}