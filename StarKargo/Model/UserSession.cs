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
using System.IO;
using SQLite;
using StarKargoService.TransactionService;
using StarKargo.Table;
using System.Threading.Tasks;

namespace StarKargo.Model
{
    public static class UserSession
    {
        public static int UserType { get; set; }
        public static string Email { get; set; }
        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        public static Guid GUID { get; set; }

        public static List<LotModel> OpenLots {get; set;}

        public static List<string> OpenLotsStringList { get; set; }

        public static List<LotModel> ShippedLots { get; set; }

        public static List<string> ShippedLotsStringList { get; set; }

        public static List<AgentModel> Agents { get; set; }

        public static List<string> AgentsStringList { get; set; }

        public static List<LocationModel> Locations { get; set; }

        public static List<string> LocationsStringList { get; set; }

        public static string DB_PATH = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "StarKargoDB.db3"); //Create New Database


        public static void SaveUserCredentials(string userName, string password, int role)
        {
            //store
            var prefs = Application.Context.GetSharedPreferences("StarKargo", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString("UserName", userName);
            prefEditor.PutString("Password", password);
            prefEditor.PutString("Role", Convert.ToString(role));
            prefEditor.Commit();

        }

        public static void SendingAllPendingTransactions()
        {
            // Sending Receiving
            Task.Factory.StartNew(() =>
            {
                SendReceivedPending();
            })
             .ContinueWith((task, y) =>
             { }, null, TaskScheduler.FromCurrentSynchronizationContext());

            // Sending Loading
            Task.Factory.StartNew(() =>
            {
                SendLoadContainerPending();
            })
             .ContinueWith((task, y) =>
             { }, null, TaskScheduler.FromCurrentSynchronizationContext());

            // Sending Unloading
            Task.Factory.StartNew(() =>
            {
                SendUnLoadContainerPending();
            })
             .ContinueWith((task, y) =>
             { }, null, TaskScheduler.FromCurrentSynchronizationContext());

            // Sending Delivery
            Task.Factory.StartNew(() =>
            {
                SendDeliverPending();
            })
              .ContinueWith((task, y) =>
              { }, null, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private static void SendDeliverPending()
        {
            ITransactionService _transactionService = new TransactionService();

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
            catch (Exception ex)
            {
  
            }
        }

        private static void SendLoadContainerPending()
        {
            ITransactionService _transactionService = new TransactionService();

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
            catch (Exception ex)
            {
            }
        }

        private static void SendUnLoadContainerPending()
        {
            ITransactionService _transactionService = new TransactionService();

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
            catch (Exception ex)
            {
            }
        }

        private static void SendReceivedPending()
        {
            ITransactionService _transactionService = new TransactionService();

            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                var data = db.Table<ReceivedOrderTable>(); //Call Table  

                int TotalCount = data.Count();
                var TotalSendCount = 0;

                foreach (var d in data)
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

     
                    var result = _transactionService.ReceivePackage(model);

                    if (result)
                    {
                        db.Delete(d);
                        TotalSendCount++;
                    }
                }


            }
            catch (Exception ex)
            {
            }
        }

    }
}