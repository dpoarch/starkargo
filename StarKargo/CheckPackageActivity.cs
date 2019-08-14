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
using Android.Net;
using StarKargo.Model;
using StarKargoCommon.Models;
using StarKargoService.TransactionService;
using StarKargoCommon.Helpers;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class CheckPackageActivity : Activity
    {
        ITransactionService _transactionService = new TransactionService();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.CheckPackage);

            Button doneAction = FindViewById<Button>(Resource.Id.btnDone);
            ImageButton checkAction = FindViewById<ImageButton>(Resource.Id.ImgBtnScan);

            // Cancel
            doneAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };


            EditText barcodeEditText = FindViewById<EditText>(Resource.Id.txtBarcode);

            barcodeEditText.KeyPress += (object sender, View.KeyEventArgs e) => {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    //add your logic here
                    CheckPackage();
                    e.Handled = true;
                }
            };


            // Check 
            checkAction.Click += (object sender, EventArgs e) =>
            {
                CheckPackage();
            };

            SetUIVisibility();
        }

        private void SetUIVisibility()
        {
            TextView txtAgentID = FindViewById<TextView>(Resource.Id.txtAgentID);
            txtAgentID.Visibility = ViewStates.Invisible;

            TextView txtLotNo = FindViewById<TextView>(Resource.Id.txtLotNo);
            txtLotNo.Visibility = ViewStates.Invisible;

            TextView txtDeliveryDate = FindViewById<TextView>(Resource.Id.txtDeliveryDate);
            txtDeliveryDate.Visibility = ViewStates.Invisible;

            TextView txtReceivedBy = FindViewById<TextView>(Resource.Id.txtReceivedBy);
            txtReceivedBy.Visibility = ViewStates.Invisible;
        }

        private void CheckPackage()
        {

            // reset visibility
            SetUIVisibility();

            // check connection
            var isConnected = GetConnectionStatus();

            if (isConnected)
            {
                // get barcdoe 
                EditText barcodeTxt = FindViewById<EditText>(Resource.Id.txtBarcode);
                var barcode = barcodeTxt.Text;

                barcode = barcode.Trim();

                if (String.IsNullOrEmpty(barcode))
                {
                    Android.Widget.Toast.MakeText(this, "Package Barcode is empty!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                CheckPackageModel model = null;

                try
                {
                    // call api
                    model = _transactionService.CheckPackageOrder(barcode);

                }
                catch (Exception ex)
                {
                    Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (model == null)
                {
                    Android.Widget.Toast.MakeText(this, "Package does not exist!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                // Update UI
                TextView packageCountView = FindViewById<TextView>(Resource.Id.txtViewPackageCount);
                TextView packageOrderView = FindViewById<TextView>(Resource.Id.txtViewPackageOrder);
                TextView statusView = FindViewById<TextView>(Resource.Id.txtViewPackageStatus);

                if (packageCountView != null)
                    packageCountView.Text = string.Format("{0} Packages", model.PackageQty);

                if (packageOrderView != null)
                    packageOrderView.Text = string.Format("SKE Order: {0}", model.OrderNumber);

                if (statusView != null)
                    statusView.Text = string.Format("Status: {0}", Utils.GetStatusTypeValue(model.Status));

                switch (model.Status)
                {
                    case 0:
                        //StatusTypeEnums.RECEIVED
                        TextView txtAgentID = FindViewById<TextView>(Resource.Id.txtAgentID);
                        txtAgentID.Visibility = ViewStates.Visible;
                        txtAgentID.Text = string.Format("Agent ID: {0}", model.AgentStr);
                        break;
                    case 1:
                        //StatusTypeEnums.LOADED
                        TextView txtLotNo = FindViewById<TextView>(Resource.Id.txtLotNo);
                        txtLotNo.Visibility = ViewStates.Invisible;
                        txtLotNo.Text = string.Format("Lot No: {0}", model.LotNoStr);
                        break;
                    case 2:
                        //StatusTypeEnums.SHIPPED

                        break;
                    case 3:
                        //StatusTypeEnums.UNLOADED
                        TextView txtLotNoUnload = FindViewById<TextView>(Resource.Id.txtLotNo);
                        txtLotNoUnload.Visibility = ViewStates.Invisible;
                        txtLotNoUnload.Text = string.Format("Lot No: {0}", model.LotNoStr);
                        break;
                    case 4:
                        //StatusTypeEnums.DELIVERED
                        TextView txtDeliveryDate = FindViewById<TextView>(Resource.Id.txtDeliveryDate);
                        txtDeliveryDate.Visibility = ViewStates.Invisible;
                        txtDeliveryDate.Text = string.Format("Delivery Date: {0}", model.DeliveredTS);

                        TextView txtReceivedBy = FindViewById<TextView>(Resource.Id.txtReceivedBy);
                        txtReceivedBy.Visibility = ViewStates.Invisible;
                        txtReceivedBy.Text = string.Format("Received By: {0}", model.DeliveredTo);
                        break;
                }
            }
            else
            {
                Android.Widget.Toast.MakeText(this, "No Connection!", Android.Widget.ToastLength.Short).Show();
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