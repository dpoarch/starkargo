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
using StarKargoCommon.Helpers;
using System.Net;
using Newtonsoft.Json;
using StarKargoCommon;

namespace StarKargoService.TransactionService
{
    public class TransactionService : ITransactionService
    {

        public bool DeliverPackage(DeliverPackageModel model)
        {
            bool isResult = false;

            string URL = string.Format("{0}{1}/?UpdateTS={2}", Utils.BASE_PATH, Utils.POST_ORDERS, model.UpdateTS);
            // string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_ORDERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    var jsonObj = JsonConvert.SerializeObject(model.ModelOrder);
                    var response = client.UploadString(URL, jsonObj);
                    ResponseModel res = JsonConvert.DeserializeObject<ResponseModel>(response);

                    if (String.IsNullOrEmpty(res.Message))
                    {
                        isResult = true;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public bool LoadContainer(LoadContainerModel model)
        {
            bool isResult = false;

            string URL = string.Format("{0}{1}/?UpdateTS={2}", Utils.BASE_PATH, Utils.POST_ORDERS, model.UpdateTS);
           // string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_ORDERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    var jsonObj = JsonConvert.SerializeObject(model.ModelOrder);
                    var response = client.UploadString(URL, jsonObj);
                    ResponseModel res = JsonConvert.DeserializeObject<ResponseModel>(response);

                    if (String.IsNullOrEmpty(res.Message))
                    {
                        isResult = true;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public bool ReceivePackage(ReceivePackageModel model)
        {
            bool isResult = false;

            //  string URL = string.Format("{0}{1}/?updateTS={2}", Utils.BASE_PATH, Utils.POST_ORDERS, model.UpdateTS);
            string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_ORDERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    var jsonObj = JsonConvert.SerializeObject(model.ModelOrder);
                    var response = client.UploadString(URL, jsonObj);
                    ResponseModel res = JsonConvert.DeserializeObject<ResponseModel>(response);

                    if (String.IsNullOrEmpty(res.Message))
                    {
                        isResult = true;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public bool UnloadContainer(UnloadContainerModel model)
        {
            bool isResult = false;

            string URL = string.Format("{0}{1}/?UpdateTS={2}", Utils.BASE_PATH, Utils.POST_ORDERS, model.UpdateTS);
            //string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.POST_ORDERS);

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";

                    var jsonObj = JsonConvert.SerializeObject(model);
                    var response = client.UploadString(URL, jsonObj);
                    ResponseModel res = JsonConvert.DeserializeObject<ResponseModel>(response);

                    if (String.IsNullOrEmpty(res.Message))
                    {
                        isResult = true;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return isResult;
        }

        public CheckPackageModel CheckPackageOrder(string barcode)
        {
            CheckPackageModel retVal = null;

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);
                    string URL = string.Format("{0}{1}?OrderNumber={2}", Utils.BASE_PATH, Utils.POST_ORDERS, barcode);
                    var response = client.DownloadString(URL);
                    ResponseCheckOrder order = JsonConvert.DeserializeObject<ResponseCheckOrder>(response);
                    retVal = order.Data;
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