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
using StarKargoService.GenericServices;
using StarKargoCommon.Models;
using StarKargoService.TransactionService;
using SQLite;
using StarKargo.Table;

namespace StarKargo
{
    [Activity(Label = "", Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class ReceivePackageActivity : Activity
    {
        IGenericService _genericService = new GenericService();
        ITransactionService _transactionService = new TransactionService();

        List<TableItem> tableItems = new List<TableItem>();
        ListView listView;

        int totalCount = 0;
        string selectedBarcode = String.Empty;
        int selectedPackageCount = 0;
        Guid SELECTED_AGENT = Guid.Empty;
        string SELECTED_AGENTSTR = "None";

        bool executeOnce = false;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ReceivePackage);

            InitializeListView();

            // Agent ID
            Spinner spinnerAgent = FindViewById<Spinner>(Resource.Id.spinAgentID);
            spinnerAgent.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinnerAgent_ItemSelected);

            //var adapterRole = ArrayAdapter.CreateFromResource(
            //        this, Resource.Array.agent_array, Resource.Layout.spinner_item);
            // Add NONE
            var adapterRole = new ArrayAdapter(this, Resource.Layout.spinner_item, UserSession.AgentsStringList);

            adapterRole.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerAgent.Adapter = adapterRole;


            // Cancel Action
            Button cancelAction = FindViewById<Button>(Resource.Id.btnCancel);

            // Barcode Action
            ImageButton barcodeAction = FindViewById<ImageButton>(Resource.Id.ImgBtnScan);

            // Complete Action
            Button completeAction = FindViewById<Button>(Resource.Id.btnComplete);

            TextView scanPackageCountTxt = FindViewById<TextView>(Resource.Id.txtViewPackageCount);

            EditText barcodeEditText = FindViewById<EditText>(Resource.Id.txtBarcode);

            barcodeEditText.KeyPress += (object sender, View.KeyEventArgs e) => {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    //add your logic here
                    AddNewPackage(scanPackageCountTxt);
                    e.Handled = true;
                }
            };

            // Cancel Action
            cancelAction.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };

            // Image barcode
            barcodeAction.Click += (object sender, EventArgs e) =>
            {
                AddNewPackage(scanPackageCountTxt);
            };

            // Complete Action
            completeAction.Click += (object sender, EventArgs e) =>
            {
                if (totalCount > 0)
                {
                    showCompleteDialog();
                }
                else
                {
                    Android.Widget.Toast.MakeText(this, "No Package!", Android.Widget.ToastLength.Short).Show();
                }
            };

            // get the total Count
            // 0 Packages Scanned
            totalCount = tableItems.Sum(x => x.PackageQty);
            scanPackageCountTxt.Text = string.Format("{0} Package(s) Scanned", totalCount);
            executeOnce = false;
        }

        private void AddNewPackage(TextView scanPackageCountTxt)
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
                    SubHeading = "1 item(s)",
                    PackageQty = 1
                });
            }


            InitializeListView();

            // get the total Count
            // 0 Packages Scanned
            totalCount = tableItems.Sum(x => x.PackageQty);
            scanPackageCountTxt.Text = string.Format("{0} Package(s) Scanned", totalCount);

            barcodeTxt.Text = String.Empty;
            Android.Widget.Toast.MakeText(this, "Package is Added!", Android.Widget.ToastLength.Short).Show();
        }

        private void spinnerAgent_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            var agentStr = Convert.ToString(spinner.GetItemAtPosition(e.Position));
            var agentModel = UserSession.Agents.Where(x => x.Value == agentStr).FirstOrDefault();
            if (agentModel != null)
            {
                SELECTED_AGENT = agentModel.GUID;
                SELECTED_AGENTSTR = agentModel.Value;
            }
        }

        void ClearScreen()
        {
            tableItems = new List<TableItem>();
            TextView scanPackageCountTxt = FindViewById<TextView>(Resource.Id.txtViewPackageCount);
            totalCount = 0;
            scanPackageCountTxt.Text = string.Format("{0} Package(s) Scanned", totalCount);
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

                if(String.IsNullOrEmpty(tempTxtPackageCount))
                {
                    return;
                }

                selectedPackageCount = Convert.ToInt16(txtPackageCount.Text);

                if (selectedPackageCount <= 0)
                {
                     RemoveItemFromList();
                }else
                {
                    UpdateItemFromList(selectedPackageCount);
                }

                executeOnce = false;
                builder.Dismiss();

                //Toast.MakeText(this, "Alert dialog dismissed!", ToastLength.Short).Show();
            };
            builder.Show();
        }

        void showCompleteDialog()
        {
            //Inflate layout
            View view = LayoutInflater.Inflate(Resource.Layout.ReceiveOKDialog, null);
            AlertDialog builder = new AlertDialog.Builder(this).Create();
            builder.SetView(view);
            builder.SetCanceledOnTouchOutside(false);

            Button buttonYesAction = view.FindViewById<Button>(Resource.Id.btnYes);
            Button buttonNoAction = view.FindViewById<Button>(Resource.Id.btnNo);


            buttonYesAction.Click += delegate
            {

                try
                {
                    //send packages
                    foreach (var item in tableItems)
                    {
                        ModelOrder modelOrder = new ModelOrder
                        {
                            Agent = SELECTED_AGENT,
                            AgentStr = SELECTED_AGENTSTR,
                            OrderNumber = item.Heading,
                            PackageQty = item.PackageQty,
                            ReceivedTS = DateTime.UtcNow,
                            Status = (int)StatusTypeEnums.RECEIVED
                        };

                        ReceivePackageModel model = new ReceivePackageModel
                        {
                            ModelOrder = modelOrder,
                            UpdateTS = false
                        };

                        if (GetConnectionStatus())
                        {
                            var result = _transactionService.ReceivePackage(model);

                            if(!result)
                            {
                                // TBD: If api fails to process a package
                                AddToLocalReceiveTable(model);
                                continue;
                            }
                        }
                        else
                        {
                            // save to local db
                            AddToLocalReceiveTable(model);
                        }
                    }

                    UserSession.SendingAllPendingTransactions();

                    // clear all info
                    ClearScreen();

                    // Reset ListView
                    InitializeListView();

                    // Display Dialog
                    Android.Widget.Toast.MakeText(this, "Transaction Completed!", Android.Widget.ToastLength.Short).Show();

                    builder.Dismiss();

                    Finish();
                }
                catch
                {
                    Android.Widget.Toast.MakeText(this, "Error : Cannot Process Transaction!", Android.Widget.ToastLength.Short).Show();
                }
            };

            buttonNoAction.Click += delegate
            {
                builder.Dismiss();
            };

            builder.Show();
        }


        void AddToLocalReceiveTable(ReceivePackageModel model)
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                // Create Table
                db.CreateTable<ReceivedOrderTable>();

                // insert new item
                ReceivedOrderTable tbl = new ReceivedOrderTable
                {
                    OrderNumber = model.ModelOrder.OrderNumber,
                    Status = model.ModelOrder.Status,
                    Agent = model.ModelOrder.Agent,
                    AgentStr = model.ModelOrder.AgentStr,
                    PackageQty = model.ModelOrder.PackageQty,
                    ReceivedTS = model.ModelOrder.ReceivedTS
                };

                db.Insert(tbl);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Short).Show();
            }
        }

        void RemoveItemFromList()
        {
            if(!String.IsNullOrEmpty(selectedBarcode))
            {
                // Check current data list
                var item = tableItems.Where(x => x.Heading == selectedBarcode).FirstOrDefault();

                if (item != null)
                {
                    // add count
                    tableItems.Remove(item);

                    InitializeListView();

                    TextView scanPackageCountTxt = FindViewById<TextView>(Resource.Id.txtViewPackageCount);

                    totalCount = tableItems.Sum(x => x.PackageQty);
                    scanPackageCountTxt.Text = string.Format("{0} Packages Scanned", totalCount);
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
                    scanPackageCountTxt.Text = string.Format("{0} Packages Scanned", totalCount);
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