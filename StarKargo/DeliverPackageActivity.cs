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
using Android.Net;
using StarKargoService.TransactionService;
using SQLite;
using StarKargo.Table;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class DeliverPackageActivity : Activity
    {
        ITransactionService _transactionService = new TransactionService();

        List<TableItem> tableItems = new List<TableItem>();
        ListView listView;

        int totalCount = 0;
        string selectedBarcode = String.Empty;
        int selectedPackageCount = 0;

        bool executeOnce = false;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.DeliveryPackage);

            Button cancelAction = FindViewById<Button>(Resource.Id.btnCancel);
            Button completeAction = FindViewById<Button>(Resource.Id.btnComplete);
            Button completeNextAction = FindViewById<Button>(Resource.Id.btnCompleNextDeliver);
            ImageButton barcodeAction = FindViewById<ImageButton>(Resource.Id.ImgBtnScan);

            EditText barcodeEditText = FindViewById<EditText>(Resource.Id.txtBarcode);

            barcodeEditText.KeyPress += (object sender, View.KeyEventArgs e) => {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    //add your logic here
                    AddNewPackage();
                    e.Handled = true;
                }
            };

            // complete
            barcodeAction.Click += (object sender, EventArgs e) =>
            {
                AddNewPackage();
            };

            // Cancel
            cancelAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };

            // complete
            completeAction.Click += (object sender, EventArgs e) =>
            {
                EditText txtReceivedBy = FindViewById<EditText>(Resource.Id.txtReceivedBy);
                EditText packageQtyTxt = FindViewById<EditText>(Resource.Id.txtPackageQty);
                EditText barcodeTxt = FindViewById<EditText>(Resource.Id.txtBarcode);
                EditText deliveredDateTxt = FindViewById<EditText>(Resource.Id.txtDateDelivered);

                // Get barcode
                var barcodeValue = barcodeTxt.Text;

                if (String.IsNullOrEmpty(barcodeValue))
                {
                    Android.Widget.Toast.MakeText(this, "Empty Package Barcode!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                // Delivered To
                var deliveredTo = txtReceivedBy.Text;

                if(String.IsNullOrEmpty(deliveredTo))
                {
                    Android.Widget.Toast.MakeText(this, "Received By is Empty!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                // Package Count
                var packageQty = packageQtyTxt.Text;

                if (String.IsNullOrEmpty(packageQty))
                {
                    Android.Widget.Toast.MakeText(this, "Package Count is Empty!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (Convert.ToInt32(packageQty) < 0)
                {
                    Android.Widget.Toast.MakeText(this, "Negative value is not allowed!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                var tempModel = _transactionService.CheckPackageOrder(barcodeValue);

                if (tempModel == null)
                {
                    Android.Widget.Toast.MakeText(this, "Package does not exist!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (tempModel.Status != (int)StatusTypeEnums.UNLOADED)
                {
                    Android.Widget.Toast.MakeText(this, "Invalid Package Status!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                var _tempDeliveredDate = deliveredDateTxt.Text;

                if (String.IsNullOrEmpty(_tempDeliveredDate))
                {
                    Android.Widget.Toast.MakeText(this, "Delivered Date is Empty!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                var DeliveredDate = Convert.ToDateTime(_tempDeliveredDate);

                ModelOrder modelOrder = new ModelOrder
                {
                    OrderNumber = barcodeValue,
                    PackageQty = Convert.ToInt32(packageQty),
                    DeliveredTS = DateTime.UtcNow,
                    Status = (int)StatusTypeEnums.DELIVERED,
                    DeliveredTo = deliveredTo,
                    GUID = tempModel.GUID,
                    DeliveredDate = DeliveredDate
                };

                DeliverPackageModel model = new DeliverPackageModel
                {
                    ModelOrder = modelOrder,
                    UpdateTS = true
                };

                if (GetConnectionStatus())
                {
                    try
                    {
                        var result = _transactionService.DeliverPackage(model);

                        // If Error
                        if (!result)
                        {
                            Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                            AddToDeliverTable(model);

                            return;
                        }

                        UserSession.SendingAllPendingTransactions();

                        Android.Widget.Toast.MakeText(this, "Transaction Successful!", Android.Widget.ToastLength.Short).Show();
                    }
                    catch(Exception ex)
                    {
                        Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                    }
                }
                else
                {
                    // save to local db
                    AddToDeliverTable(model);
                }



                // go back to main menu
                Finish();
            };


            // complete and deliver next
            completeNextAction.Click += (object sender, EventArgs e) =>
            {
                EditText barcodeTxt = FindViewById<EditText>(Resource.Id.txtBarcode);
                EditText packageQtyTxt = FindViewById<EditText>(Resource.Id.txtPackageQty);
                EditText txtReceivedBy = FindViewById<EditText>(Resource.Id.txtReceivedBy);


                // Get barcode
                var barcodeValue = barcodeTxt.Text;

                if (String.IsNullOrEmpty(barcodeValue))
                {
                    Android.Widget.Toast.MakeText(this, "Empty Package Barcode!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                // Delivered To
                var deliveredTo = txtReceivedBy.Text;

                if (String.IsNullOrEmpty(deliveredTo))
                {
                    Android.Widget.Toast.MakeText(this, "Received By is Empty!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                // Package Count
                var packageQty = packageQtyTxt.Text;

                if (String.IsNullOrEmpty(packageQty))
                {
                    Android.Widget.Toast.MakeText(this, "Package Count is Empty!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (Convert.ToInt32(packageQty) < 0)
                {
                    Android.Widget.Toast.MakeText(this, "Negative value is not allowed!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                var tempModel = _transactionService.CheckPackageOrder(barcodeValue);

                if (tempModel == null)
                {
                    Android.Widget.Toast.MakeText(this, "Package does not exist!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (tempModel.Status != (int)StatusTypeEnums.UNLOADED)
                {
                    Android.Widget.Toast.MakeText(this, "Invalid Package Status!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                ModelOrder modelOrder = new ModelOrder
                {
                    OrderNumber = barcodeValue,
                    PackageQty = Convert.ToInt32(packageQty),
                    DeliveredTS = DateTime.UtcNow,
                    Status = (int)StatusTypeEnums.DELIVERED,
                    DeliveredTo = deliveredTo,
                    GUID = tempModel.GUID
                };

                DeliverPackageModel model = new DeliverPackageModel
                {
                    ModelOrder = modelOrder,
                    UpdateTS = true
                };

                if (GetConnectionStatus())
                {
                    try
                    {
                        var result = _transactionService.DeliverPackage(model);

                        // If Error
                        if (!result)
                        {
                            Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                            AddToDeliverTable(model);

                            return;
                        }

                        UserSession.SendingAllPendingTransactions();
                        Android.Widget.Toast.MakeText(this, "Transaction Successful!", Android.Widget.ToastLength.Short).Show();
                    }
                    catch (Exception ex)
                    {
                        Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                    }
                }
                else
                {
                    // save to local db
                    AddToDeliverTable(model);
                }

                // clear text
                barcodeTxt.Text = String.Empty;
                packageQtyTxt.Text = "0";
                txtReceivedBy.Text = String.Empty;
            };
        }

        private void AddNewPackage()
        {
            EditText barcodeTxt = FindViewById<EditText>(Resource.Id.txtBarcode);
            EditText packageQtyTxt = FindViewById<EditText>(Resource.Id.txtPackageQty);

            // Get barcode
            var barcodeValue = barcodeTxt.Text;

            barcodeValue = barcodeValue.Trim();

            if (String.IsNullOrEmpty(barcodeValue))
            {
                Android.Widget.Toast.MakeText(this, "Empty Package Barcode!", Android.Widget.ToastLength.Short).Show();
                return;
            }

            if (GetConnectionStatus())
            {
                // call api
                var model = _transactionService.CheckPackageOrder(barcodeValue);

                if (model == null)
                {
                    Android.Widget.Toast.MakeText(this, "Package does not exist!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (model.Status != (int)StatusTypeEnums.UNLOADED)
                {
                    Android.Widget.Toast.MakeText(this, "Invalid Package Status!", Android.Widget.ToastLength.Short).Show();
                    barcodeTxt.Text = string.Empty;
                    return;
                }

                var packageCount = packageQtyTxt.Text;

                if (String.IsNullOrEmpty(packageCount))
                {
                    packageQtyTxt.Text = "1";
                }
                else
                {
                    var tempCount = Convert.ToInt16(packageCount);
                    tempCount++;
                    packageQtyTxt.Text = Convert.ToString(tempCount);
                }


                Android.Widget.Toast.MakeText(this, "Package Found!", Android.Widget.ToastLength.Short).Show();
            }
        }

        void ProcessDeliver(string deliveredTo)
        {
            foreach (var item in tableItems)
            {
                ModelOrder modelOrder = new ModelOrder
                {
                    OrderNumber = item.Heading,
                    PackageQty = item.PackageQty,
                    DeliveredTS = DateTime.UtcNow,
                    Status = (int)StatusTypeEnums.DELIVERED,
                    DeliveredTo = deliveredTo,
                    GUID = item.GUID
                };

                DeliverPackageModel model = new DeliverPackageModel
                {
                    ModelOrder = modelOrder,
                    UpdateTS = true
                };

                if (GetConnectionStatus())
                {
                    var result = _transactionService.DeliverPackage(model);

                    if (!result)
                    {
                        AddToDeliverTable(model);
                        continue;
                    }
                }
                else
                {
                    // save to local db
                    AddToDeliverTable(model);
                }
            }
        }

        void ClearScreen()
        {
            tableItems = new List<TableItem>();
        }

        void AddToDeliverTable(DeliverPackageModel model)
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                // Create Table
                db.CreateTable<DeliverTable>();

                // insert new item
                DeliverTable tbl = new DeliverTable
                {
                    OrderNumber = model.ModelOrder.OrderNumber,
                    Status = model.ModelOrder.Status,
                    PackageQty = model.ModelOrder.PackageQty,
                    DeliveredTS = model.ModelOrder.DeliveredTS,
                    DeliveredTo = model.ModelOrder.DeliveredTo,
                    GUID  = model.ModelOrder.GUID
                };

                db.Insert(tbl);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Short).Show();
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