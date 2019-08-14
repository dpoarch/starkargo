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
using StarKargoCommon.Enumerations;
using SQLite;
using System.Threading.Tasks;
using StarKargoCommon.Models;
using Android.Net;
using StarKargoService.TransactionService;
using StarKargo.Table;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class PendingActivity : Activity
    {
        ITransactionService _transactionService = new TransactionService();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PendingPackage);

            // Send Received
            Button receivedAction = FindViewById<Button>(Resource.Id.btnReceiveComplete);

            // Send Load
            Button  loadAction = FindViewById<Button>(Resource.Id.btnLoadComplete);

            // Send UnLoad
            Button unloadAction = FindViewById<Button>(Resource.Id.btnUnLoadComplete);

            // Send Deliver
            Button deliverAction = FindViewById<Button>(Resource.Id.btnDeliverComplete);

            // Done
            Button doneAction = FindViewById<Button>(Resource.Id.btnDone);

            // Cancel
            doneAction.Click += (object sender, EventArgs e) =>
            {
                var userType = UserSession.UserType;

                switch (userType)
                {
                    case (int)UserTypeEnums.Administrator:
                        var adminActivity = new Intent(this, typeof(AdminActivity));
                        StartActivity(adminActivity);
                        break;
                    case (int)UserTypeEnums.Fulfillment:
                        var fullfillmentActivity = new Intent(this, typeof(FullfillmentActivity));
                        StartActivity(fullfillmentActivity);
                        break;
                    case (int)UserTypeEnums.Warehouse:
                        var wareHouseActivity = new Intent(this, typeof(WarehouseActivity));
                        StartActivity(wareHouseActivity);
                        break;
                }
            };

            // Receive Send
            receivedAction.Click += (object sender, EventArgs e) =>
            {
                TextView txtRecCount = FindViewById<TextView>(Resource.Id.txtRecCount);
                var recCount = txtRecCount.Text;

                if(Convert.ToInt16(recCount) == 0)
                {
                    Android.Widget.Toast.MakeText(this, "Empty Packages!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                SendReceivedPending();
            };

            // Load Container Send
            loadAction.Click += (object sender, EventArgs e) =>
            {
                TextView txtLoadCount = FindViewById<TextView>(Resource.Id.txtLoadCount);
                var loadCount = txtLoadCount.Text;

                if (Convert.ToInt16(loadCount) == 0)
                {
                    Android.Widget.Toast.MakeText(this, "Empty Packages!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                SendLoadContainerPending();
            };

            // Unload Container Send
            unloadAction.Click += (object sender, EventArgs e) =>
            {
                TextView txtUnLoadCount = FindViewById<TextView>(Resource.Id.txtUnLoadCount);
                var unloadCount = txtUnLoadCount.Text;

                if (Convert.ToInt16(unloadCount) == 0)
                {
                    Android.Widget.Toast.MakeText(this, "Empty Packages!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                SendUnLoadContainerPending();
            };

            // Deliver Send
            deliverAction.Click += (object sender, EventArgs e) =>
            {
                TextView txtDeliverCount = FindViewById<TextView>(Resource.Id.txtDeliverCount);
                var deliverCount = txtDeliverCount.Text;

                if (Convert.ToInt16(deliverCount) == 0)
                {
                    Android.Widget.Toast.MakeText(this, "Empty Packages!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                SendDeliverPending();
            };

            ReceivedCount();
            LoadContainerCount();
            UnLoadContainerCount();
            DeliverCount();
        }

        private void SendReceivedPending()
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                var data = db.Table<ReceivedOrderTable>(); //Call Table  

                int TotalCount = data.Count();
                var TotalSendCount = 0;

                foreach(var d in data)
                {
                    ModelOrder modelOrder = new ModelOrder
                    {
                        Agent = d.Agent,
                        AgentStr = d.AgentStr,
                        OrderNumber = d.OrderNumber,
                        PackageQty = d.PackageQty,
                        ReceivedTS = d.ReceivedTS,
                        Status = d.Status
                    };

                    ReceivePackageModel model = new ReceivePackageModel
                    {
                        ModelOrder = modelOrder,
                        UpdateTS = false
                    };

                    if (GetConnectionStatus())
                    {
                        var result = _transactionService.ReceivePackage(model);

                        if(result)
                        {
                            db.Delete(d);
                            TotalSendCount++;
                        }
                    }
                }

                // Update Count for LOAD Container
                ReceivedCount();

                if (TotalSendCount == 0)
                {
                    Android.Widget.Toast.MakeText(this, "No Packages were sent!", Android.Widget.ToastLength.Short).Show();
                    return;
                }


                if(TotalCount == TotalSendCount)
                {
                    Android.Widget.Toast.MakeText(this, "Successful sending of all packages!", Android.Widget.ToastLength.Short).Show();
                }
                else
                {
                    Android.Widget.Toast.MakeText(this, "Only " + TotalSendCount + " Package(s) were Successfully Sent!", Android.Widget.ToastLength.Short).Show();
                }

            }
            catch (Exception ex)
            {
                Android.Widget.Toast.MakeText(this, "Error !", Android.Widget.ToastLength.Short).Show();
               // throw;
            }
        }

        private void SendLoadContainerPending()
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                var data = db.Table<LoadContainerTable>(); //Call Table  

                int TotalCount = data.Count();
                var TotalSendCount = 0;
                var InvalidPackageCount = 0;


                foreach (var d in data)
                {
                    ModelOrder modelOrder = new ModelOrder
                    {
                        GUID = d.GUID,
                        LotNo = d.LotNo,
                        LotNoStr = d.LotNoStr,
                        OrderNumber = d.OrderNumber,
                        PackageQty = d.PackageQty,
                        LoadedTS = d.LoadedTS,
                        Status = d.Status,
                    };

                    LoadContainerModel model = new LoadContainerModel
                    {
                        ModelOrder = modelOrder,
                        UpdateTS = false
                    };

                    if (GetConnectionStatus())
                    {
                        // check if item exists in database and item has RECEIVED status
                        // if yes : process
                        // if no  : delete from local table
                        // check connection
                        try
                        {
                            // call api
                            var tempModel = _transactionService.CheckPackageOrder(model.ModelOrder.OrderNumber);

                            if (tempModel == null)
                            {
                                InvalidPackageCount++;
                                db.Delete(d);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            //Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                        }
            
                        var result = _transactionService.LoadContainer(model);

                        if (result)
                        {
                            db.Delete(d);
                            TotalSendCount++;
                        }
                    }
                }

                // Update Count for LOAD Container
                LoadContainerCount();

                if (TotalSendCount == 0 && InvalidPackageCount == 0)
                {
                    Android.Widget.Toast.MakeText(this, "No Packages were sent!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (InvalidPackageCount > 0)
                {
                    Android.Widget.Toast.MakeText(this, string.Format("{0} Invalid Package(s)!",InvalidPackageCount), Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (TotalCount == TotalSendCount)
                {
                    Android.Widget.Toast.MakeText(this, "Successful sending of all packages!", Android.Widget.ToastLength.Short).Show();
                }
                else if(TotalSendCount > 0)
                {
                    Android.Widget.Toast.MakeText(this, "Only " + TotalSendCount + " Package(s) were Successfully Sent!", Android.Widget.ToastLength.Short).Show();
                }

            }
            catch (Exception ex)
            {
                Android.Widget.Toast.MakeText(this, "Error !", Android.Widget.ToastLength.Short).Show();
                //throw;
            }
        }

        private void SendUnLoadContainerPending()
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                var data = db.Table<UnLoadContainerTable>(); //Call Table  

                int TotalCount = data.Count();
                var TotalSendCount = 0;
                var InvalidPackageCount = 0;

                foreach (var d in data)
                {
                    ModelOrder modelOrder = new ModelOrder
                    {
                        GUID = d.GUID,
                        LotNo = d.LotNo,
                        LotNoStr = d.LotNoStr,
                        OrderNumber = d.OrderNumber,
                        PackageQty = d.PackageQty,
                        UnloadedTS = d.LoadedTS,
                        Status = d.Status,
                    };

                    UnloadContainerModel model = new UnloadContainerModel
                    {
                        ModelOrder = modelOrder,
                        UpdateTS = true
                    };

                    if (GetConnectionStatus())
                    {

                        // check if item exists in database and item has RECEIVED status
                        // if yes : process
                        // if no  : delete from local table
                        // check connection
                        try
                        {
                            // call api
                            var tempModel = _transactionService.CheckPackageOrder(model.ModelOrder.OrderNumber);

                            if (tempModel == null)
                            {
                                InvalidPackageCount++;
                                db.Delete(d);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            //Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                        }

                        var result = _transactionService.UnloadContainer(model);

                        if (result)
                        {
                            db.Delete(d);
                            TotalSendCount++;
                        }
                    }
                }

                UnLoadContainerCount();

                if (TotalSendCount == 0 && InvalidPackageCount == 0)
                {
                    Android.Widget.Toast.MakeText(this, "No Packages were sent!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (InvalidPackageCount > 0)
                {
                    Android.Widget.Toast.MakeText(this, string.Format("{0} Invalid Package(s)!", InvalidPackageCount), Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (TotalCount == TotalSendCount)
                {
                    Android.Widget.Toast.MakeText(this, "Successful sending of all packages!", Android.Widget.ToastLength.Short).Show();
                }
                else if (TotalSendCount > 0)
                {
                    Android.Widget.Toast.MakeText(this, "Only " + TotalSendCount + " Package(s) were Successfully Sent!", Android.Widget.ToastLength.Short).Show();
                }

            }
            catch (Exception ex)
            {
                Android.Widget.Toast.MakeText(this, "Error !", Android.Widget.ToastLength.Short).Show();
                // throw;
            }
        }

        private void SendDeliverPending()
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                var data = db.Table<DeliverTable>(); //Call Table  

                int TotalCount = data.Count();
                var TotalSendCount = 0;
                var InvalidPackageCount = 0;

                foreach (var d in data)
                {
                    ModelOrder modelOrder = new ModelOrder
                    {
                        GUID = d.GUID,
                        OrderNumber = d.OrderNumber,
                        PackageQty = d.PackageQty,
                        DeliveredTo = d.DeliveredTo,
                        DeliveredTS = d.DeliveredTS,
                        Status = d.Status,
                        DeliveredDate = d.DeliveredDate
                    };

                    DeliverPackageModel model = new DeliverPackageModel
                    {
                        ModelOrder = modelOrder,
                        UpdateTS = true
                    };

                    if (GetConnectionStatus())
                    {
                        // check if item exists in database and item has RECEIVED status
                        // if yes : process
                        // if no  : delete from local table
                        // check connection
                        try
                        {
                            // call api
                            var tempModel = _transactionService.CheckPackageOrder(model.ModelOrder.OrderNumber);

                            if (tempModel == null)
                            {
                                InvalidPackageCount++;
                                db.Delete(d);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            //Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                        }

                        var result = _transactionService.DeliverPackage(model);

                        if (result)
                        {
                            db.Delete(d);
                            TotalSendCount++;
                        }
                    }
                }


                DeliverCount();

                if (TotalSendCount == 0 && InvalidPackageCount == 0)
                {
                    Android.Widget.Toast.MakeText(this, "No Packages were sent!", Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (InvalidPackageCount > 0)
                {
                    Android.Widget.Toast.MakeText(this, string.Format("{0} Invalid Package(s)!", InvalidPackageCount), Android.Widget.ToastLength.Short).Show();
                    return;
                }

                if (TotalCount == TotalSendCount)
                {
                    Android.Widget.Toast.MakeText(this, "Successful sending of all packages!", Android.Widget.ToastLength.Short).Show();
                }
                else if (TotalSendCount > 0)
                {
                    Android.Widget.Toast.MakeText(this, "Only " + TotalSendCount + " Package(s) were Successfully Sent!", Android.Widget.ToastLength.Short).Show();
                }

            }
            catch (Exception ex)
            {
                Android.Widget.Toast.MakeText(this, "Error !", Android.Widget.ToastLength.Short).Show();
                // throw;
            }
        }

        private void DeliverCount()
        {
            TextView txtDeliverCount = FindViewById<TextView>(Resource.Id.txtDeliverCount);
            int totalCount = 0;

            try
            {

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);
                db.CreateTable<DeliverTable>();
                var data = db.Table<DeliverTable>(); //Call Table  

                totalCount = data.Count();
                txtDeliverCount.Text = string.Format("{0}", totalCount);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ReceivedCount()
        {
            TextView txtRecCount = FindViewById<TextView>(Resource.Id.txtRecCount);
            int totalCount = 0;

            try
            {

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);
                db.CreateTable<ReceivedOrderTable>();
                var data = db.Table<ReceivedOrderTable>(); //Call Table  

                totalCount = data.Count();

                txtRecCount.Text = string.Format("{0}", totalCount);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void LoadContainerCount()
        {
            TextView txtLoadCount = FindViewById<TextView>(Resource.Id.txtLoadCount);

            int totalCount = 0;

            try
            {

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);
                db.CreateTable<LoadContainerTable>();
                var data = db.Table<LoadContainerTable>(); //Call Table 

                totalCount = data.Count();

                txtLoadCount.Text = string.Format("{0}", totalCount);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void UnLoadContainerCount()
        {
            TextView txtUnLoadCount = FindViewById<TextView>(Resource.Id.txtUnLoadCount);

            int totalCount = 0;

            try
            {

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);
                db.CreateTable<UnLoadContainerTable>();
                var data = db.Table<UnLoadContainerTable>(); //Call Table  

                totalCount = data.Count();

                txtUnLoadCount.Text = string.Format("{0}", totalCount);

            }
            catch (Exception ex)
            {
                throw;
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