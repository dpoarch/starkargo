using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Net;
using StarKargoCommon.Enumerations;
using System.Linq;
using Android.Views;
using StarKargo.Model;
using StarKargoService.TransactionService;
using StarKargoCommon.Models;
using SQLite;
using StarKargo.Table;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class LoadContainerActivity : Activity
    {
        ITransactionService _transactionService = new TransactionService();

        List<TableItem> tableItems = new List<TableItem>();
        ListView listView;

        int totalCount = 0;
        string selectedBarcode = String.Empty;
        int selectedPackageCount = 0;
        bool executeOnce = false;

        Guid SELECTED_LOTNO = Guid.Empty;
        string SELECTED_LOTNOSTR = String.Empty;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.LoadContainer);

            InitializeListView();

            // Open Lot ID
            Spinner spinnnerLot = FindViewById<Spinner>(Resource.Id.spinLotID);
            var adapterLot = new ArrayAdapter(this, Resource.Layout.spinner_item, UserSession.OpenLotsStringList);
            spinnnerLot.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinnerLot_ItemSelected);
            adapterLot.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnnerLot.Adapter = adapterLot;

            // Cancel Action
            Button cancelAction = FindViewById<Button>(Resource.Id.btnCancel);

            // Barcode Action
            ImageButton barcodeAction = FindViewById<ImageButton>(Resource.Id.ImgBtnScan);

            // Complete Action
            Button completeAction = FindViewById<Button>(Resource.Id.btnComplete);

            // Cancel
            cancelAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };

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

            // Image barcode
            barcodeAction.Click += (object sender, EventArgs e) =>
            {
                AddNewPackage();
            };

            // Complete
            completeAction.Click += (object sender, EventArgs e) =>
            {
                try
                {
                    if (totalCount <= 0)
                    {
                        Android.Widget.Toast.MakeText(this, "No Package!", Android.Widget.ToastLength.Short).Show();
                        return;
                    }

                    // Check if Lot No is empty
                    if (String.IsNullOrEmpty(SELECTED_LOTNOSTR))
                    {
                        Android.Widget.Toast.MakeText(this, "No Selected Lot No!", Android.Widget.ToastLength.Short).Show();
                        return;
                    }

                    // store info
                    ProcessLoadContainer();

                    UserSession.SendingAllPendingTransactions();

                    // clear all info
                    ClearScreen();

                    // Reset ListView
                    InitializeListView();

                    // Display Dialog
                    Android.Widget.Toast.MakeText(this, "Transaction Completed!", Android.Widget.ToastLength.Short).Show();

                    Finish();
                }
                catch
                {
                    Android.Widget.Toast.MakeText(this, "Error : Cannot Process Transaction!", Android.Widget.ToastLength.Short).Show();
                }
            };

            // get the total Count
            // 0 Packages Scanned
            totalCount = tableItems.Sum(x => x.PackageQty);
           // scanPackageCountTxt.Text = string.Format("{0} Packages Scanned", totalCount);
        }

        private void AddNewPackage()
        {
            EditText barcodeTxt = FindViewById<EditText>(Resource.Id.txtBarcode);

            // Get barcode
            var barcodeValue = barcodeTxt.Text;

            barcodeValue = barcodeValue.Trim();

            if (String.IsNullOrEmpty(barcodeValue))
            {
                Android.Widget.Toast.MakeText(this, "Empty Package Barcode!", Android.Widget.ToastLength.Short).Show();
                return;
            }

            CheckPackageModel model = null;
            if (GetConnectionStatus())
            {
                try
                {
                    // call api
                    model = _transactionService.CheckPackageOrder(barcodeValue);


                    if (model == null)
                    {
                        Android.Widget.Toast.MakeText(this, "Package does not exists!", Android.Widget.ToastLength.Short).Show();
                        return;
                    }

                    if (model.Status != (int)StatusTypeEnums.RECEIVED)
                    {
                        Android.Widget.Toast.MakeText(this, "Invalid Package Status!", Android.Widget.ToastLength.Short).Show();
                        barcodeTxt.Text = string.Empty;

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                }

            }

            // Check current data list
            var item = tableItems.Where(x => x.Heading == barcodeValue).FirstOrDefault();

            if (item != null)
            {
                // add count
                var totalCount = item.PackageQty + 1;
                tableItems.Where(x => x.Heading == barcodeValue).FirstOrDefault().PackageQty = totalCount;
                tableItems.Where(x => x.Heading == barcodeValue).FirstOrDefault().SubHeading = string.Format("{0} item(s)", totalCount);
            }
            else
            {
                tableItems.Add(new TableItem
                {
                    Heading = barcodeValue,
                    // SubHeading = string.Format("{0} item(s)", model != null ? model.PackageQty : 1),
                    // PackageQty = model != null ? model.PackageQty : 1,
                    SubHeading = string.Format("{0} item(s)", 1),
                    PackageQty = 1,
                    GUID = model != null ? model.GUID : Guid.Empty
                });
            }

            InitializeListView();

            // get the total Count
            // 0 Packages Scanned
            totalCount = tableItems.Sum(x => x.PackageQty);
            //  scanPackageCountTxt.Text = string.Format("{0} Packages Scanned", totalCount);

            barcodeTxt.Text = String.Empty;
            Android.Widget.Toast.MakeText(this, "Package is Added!", Android.Widget.ToastLength.Short).Show();
            executeOnce = false;
        }

        private void ProcessLoadContainer()
        {
            try
            {
                foreach (var item in tableItems)
                {
                    // check if item exists in database and item has RECEIVED status
                    // if yes : process
                    // if no  : skip
                    // check connection
                    if (GetConnectionStatus())
                    {
                        try
                        {
                            // call api
                            var tempModel = _transactionService.CheckPackageOrder(item.Heading);


                            if (tempModel == null)
                            {
                                continue;
                            }

                            if (tempModel.Status != (int)StatusTypeEnums.RECEIVED)
                            {
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            //Android.Widget.Toast.MakeText(this, "Error!", Android.Widget.ToastLength.Short).Show();
                        }
                    }

                    // PROCESS ORDER
                    ModelOrder modelOrder = new ModelOrder
                    {
                        LotNo = SELECTED_LOTNO,
                        LotNoStr = SELECTED_LOTNOSTR,
                        OrderNumber = item.Heading,
                        PackageQty = item.PackageQty,
                        LoadedTS = DateTime.UtcNow,
                        Status = (int)StatusTypeEnums.LOADED,
                        GUID = item.GUID
                    };

                    LoadContainerModel model = new LoadContainerModel
                    {
                        ModelOrder = modelOrder,
                        UpdateTS = true
                    };

                    if (GetConnectionStatus())
                    {
                        // check if valid package


                        var result = _transactionService.LoadContainer(model);

                        if (!result)
                        {
                            AddToLocalLoadContainerTable(model);
                            continue;
                        }
                    }
                    else
                    {
                        // save to local db
                        AddToLocalLoadContainerTable(model);
                    }
                }
            }
            catch(Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Short).Show();
            }
        }

        private void AddToLocalLoadContainerTable(LoadContainerModel model)
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                // Create Table
                db.CreateTable<LoadContainerTable>();

                // insert new item
                LoadContainerTable tbl = new LoadContainerTable
                {
                    OrderNumber = model.ModelOrder.OrderNumber,
                    Status = model.ModelOrder.Status,
                    LotNo = model.ModelOrder.Agent,
                    LotNoStr = model.ModelOrder.AgentStr,
                    PackageQty = model.ModelOrder.PackageQty,
                    LoadedTS = model.ModelOrder.LoadedTS,
                    GUID = model.ModelOrder.GUID
                };

                db.Insert(tbl);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Short).Show();
            }
        }

        private void spinnerLot_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            var lotStr = Convert.ToString(spinner.GetItemAtPosition(e.Position));
            var lotModel = UserSession.OpenLots.Where(x => x.Value == lotStr).FirstOrDefault();
            if (lotModel != null)
            {
                SELECTED_LOTNO = lotModel.GUID;
                SELECTED_LOTNOSTR = lotModel.Value;
            }
        }

        void ClearScreen()
        {
            tableItems = new List<TableItem>();
        }


        void showPackageCountDialog(int count)
        {
            //Inflate layout
            View view = LayoutInflater.Inflate(Resource.Layout.DialogLayout, null);
            AlertDialog builder = new AlertDialog.Builder(this).Create();
            builder.SetView(view);
            builder.SetCanceledOnTouchOutside(false);

            Button button = view.FindViewById<Button>(Resource.Id.btnOK);

            // Assign Package Count
            EditText txtPackageCount = view.FindViewById<EditText>(Resource.Id.txtPackageCount);
            txtPackageCount.Text = Convert.ToString(count);

            button.Click += delegate
            {
                txtPackageCount = view.FindViewById<EditText>(Resource.Id.txtPackageCount);

                var tempTxtPackageCount = txtPackageCount.Text;

                tempTxtPackageCount = tempTxtPackageCount.Trim();

                if (String.IsNullOrEmpty(tempTxtPackageCount))
                {
                    return;
                }

                selectedPackageCount = Convert.ToInt16(txtPackageCount.Text);

                if (selectedPackageCount <= 0)
                {
                     RemoveItemFromList();
                }
                else
                {
                    UpdateItemFromList(selectedPackageCount);
                }

                executeOnce = false;
                builder.Dismiss();
            };
            builder.Show();
        }

        void RemoveItemFromList()
        {
            if (!String.IsNullOrEmpty(selectedBarcode))
            {
                // Check current data list
                var item = tableItems.Where(x => x.Heading == selectedBarcode).FirstOrDefault();

                if (item != null)
                {
                    // add count
                    tableItems.Remove(item);

                    InitializeListView();
                }
            }
        }


        void UpdateItemFromList(int selectedPackageCount)
        {
            if (!String.IsNullOrEmpty(selectedBarcode))
            {
                // Check current data list
                var item = tableItems.Where(x => x.Heading == selectedBarcode).FirstOrDefault();

                if (item != null)
                {
                    item.PackageQty = selectedPackageCount;
                    item.SubHeading = string.Format("{0} item(s)", selectedPackageCount);

                    InitializeListView();

                    TextView scanPackageCountTxt = FindViewById<TextView>(Resource.Id.txtViewPackageCount);

                    totalCount = tableItems.Sum(x => x.PackageQty);
                    //scanPackageCountTxt.Text = string.Format("{0} Packages Scanned", totalCount);
                }
            }
        }


        void InitializeListView()
        {
            // Initialize the ListView with the data first.
            listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter = new HomeScreenAdapter(this, tableItems);

            // Important - Set the ChoiceMode
            listView.ChoiceMode = ChoiceMode.Single;

            listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {

                if (!executeOnce)
                {
                    var t = tableItems[e.Position];

                    var count = t.PackageQty;
                    selectedBarcode = t.Heading;
                    selectedPackageCount = t.PackageQty;
                    executeOnce = true;
                    showPackageCountDialog(count);
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
    }
}