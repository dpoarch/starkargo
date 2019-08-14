using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using StarKargo;
using StarKargoCommon.Models;
using StarKargoCommon.Enumerations;
using StarKargoCommon.Helpers;
using StarKargo.Model;
using SQLite;
using System.IO;
using StarKargoService.UserService;
using System.Threading.Tasks;
using System.Linq;
using StarKargoService.GenericServices;
using Android.Net;
using StarKargo.Table;
using Android.Graphics;

namespace LogIn
{
    [Activity(Label = "StarKargo", MainLauncher = true,  Theme = "@style/android:Theme.Holo.Light.NoActionBar")]
    public class LoginActivity : Activity
    {
        IUserService _userService = new UserService();
        IGenericService _genericService = new GenericService();

        private string USERNAME = string.Empty;
        private string PASSWORD = string.Empty;
        private int ROLE = -1;

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Login);

            // Create DB
            CreateDB();

            RetrieveUserCredentials();

            if(!String.IsNullOrEmpty(USERNAME) && !String.IsNullOrEmpty(PASSWORD))
            {
                // check if its still valid
                var pwd = Utils.GeneratePassword(USERNAME, PASSWORD);

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);
                db.CreateTable<UserTable>();
                var data = db.Table<UserTable>(); 

                var userDataEntity = data.Where(x => x.Email == USERNAME && x.Password == pwd).FirstOrDefault(); //Linq Query  

                if (userDataEntity != null)
                {
                    UserSession.FirstName = userDataEntity.FirstName;
                    UserSession.LastName = userDataEntity.LastName;
                    UserSession.Email = userDataEntity.Email;
                    UserSession.GUID = userDataEntity.GUID;
                    UserSession.UserType = userDataEntity.Role;

                    switch (userDataEntity.Role)
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


                    Task.Factory.StartNew(() =>
                    {
                        InitializeUserTable();
                    })
                    .ContinueWith((task, y) =>
                    {
                        //!! Your code to add news to some control
                    }, null, TaskScheduler.FromCurrentSynchronizationContext());

                    Task.Factory.StartNew(() =>
                    {
                        InitializeReferences();
                    })
                    .ContinueWith((task, y) =>
                    {
                        //!! Your code to add news to some control
                    }, null, TaskScheduler.FromCurrentSynchronizationContext());


                    //InitializeReferences();
                    //Parallel.Invoke(() => DropAndReCreateTable());
                    //Parallel.Invoke(() => InitializeReferences());

                    return;
                }
            }


            //Initializing button from layout
            Button login = FindViewById<Button>(Resource.Id.login);

            //Login button click action
            login.Click += (object sender, EventArgs e) => 
            {
                try
                {
                    EditText txtUserName = FindViewById<EditText>(Resource.Id.userName);
                    EditText txtPwd = FindViewById<EditText>(Resource.Id.password);
                    CheckBox chkRememberMe = FindViewById<CheckBox>(Resource.Id.RememberMe);


                    var userName = txtUserName.Text;
                    var password = txtPwd.Text;
                    var rememberMe = chkRememberMe.Checked;

                    if (String.IsNullOrEmpty(userName))
                    {
                        Android.Widget.Toast.MakeText(this, " Missing  Username!", Android.Widget.ToastLength.Short).Show();
                        return;
                    }

                    if (String.IsNullOrEmpty(password))
                    {
                        Android.Widget.Toast.MakeText(this, " Missing  Password!", Android.Widget.ToastLength.Short).Show();
                        return;
                    }

                    var pwd = Utils.GeneratePassword(userName, password);

                    var db = new SQLiteConnection(UserSession.DB_PATH);
                    var userDataTable = db.Table<UserTable>(); //Call Table  

                    // int count = data.Count();
                    //anuja_kc@abs-cbn.com
                    var userDataEntity = userDataTable.Where(x => x.Email == userName && x.Password == pwd).FirstOrDefault(); //Linq Query  
                   // var userDataEntity = userDataTable.Where(x => x.Email == "anuja_kc@abs-cbn.com").FirstOrDefault(); //Linq Query  

                    if (userDataEntity != null)
                    {
                        Toast.MakeText(this, "Login Success", ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(this, "Username or Password invalid", ToastLength.Short).Show();
                        return;
                    }

                    Utils.TOKEN = userDataEntity.GUID.ToString();
                    UserSession.UserType = userDataEntity.Role;
                    UserSession.FirstName = userDataEntity.FirstName;
                    UserSession.LastName = userDataEntity.LastName;
                    UserSession.Email = userDataEntity.Email;
                    UserSession.GUID = userDataEntity.GUID;

                    if (rememberMe)
                    {
                        UserSession.SaveUserCredentials(userName, password, UserSession.UserType);
                    }
                    else
                    {
                        UserSession.SaveUserCredentials("", "", 0);
                    }

                    // ReCreate Table
                    //Parallel.Invoke(() => DropAndReCreateTable());
                    //Parallel.Invoke(() => InitializeReferences());

                    switch (UserSession.UserType)
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
                }
                catch(Exception ex)
                {
                    Toast.MakeText(this, "Error" + ex.Message, ToastLength.Long).Show();
                }

                Task.Factory.StartNew(() =>
                {
                    InitializeReferences();
                })
                .ContinueWith((task, y) =>
                { }, null, TaskScheduler.FromCurrentSynchronizationContext());
            };


            //Initializing button from layout
            Button forgotPwdBtn = FindViewById<Button>(Resource.Id.btnForgotPwd);

            forgotPwdBtn.Click += (object sender, EventArgs e) =>
            {
                var forgotPwdActivity = new Intent(this, typeof(ForgotPasswordActivity));
                StartActivity(forgotPwdActivity);
            };



            Task.Factory.StartNew(() =>
            {
                InitializeUserTable();
              })
              .ContinueWith((task, y) =>
              { }, null, TaskScheduler.FromCurrentSynchronizationContext());

            Task.Factory.StartNew(() =>
            {
                InitializerReferenceTable();
            })
            .ContinueWith((task, y) =>
            { }, null, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // Function called from OnCreate
        protected void RetrieveUserCredentials()
        {
            //retreive 
            var prefs = Application.Context.GetSharedPreferences("StarKargo", FileCreationMode.Private);
            USERNAME = prefs.GetString("UserName", null);
            PASSWORD = prefs.GetString("Password", null);
            ROLE = Convert.ToInt16(prefs.GetString("Role", null));           
        }

        public void RemoveUnknownLocalUserFromDB()
        {
            try
            {

                if (!GetConnectionStatus())
                    return;

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                // Create Table
                db.CreateTable<UserTable>();

                var usersLocalTable = db.Table<UserTable>(); //Call Table  

                var usersDBTable = _userService.GetUsers();

                Parallel.ForEach(usersLocalTable, d =>
                {
                    // check if data on local intersect data on db
                    var entity = usersDBTable.Where(x => x.GUID == d.GUID).FirstOrDefault();

                    if (entity == null)
                    {
                        // data is deleted on db
                        db.Delete(d);
                    }
                });
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error!", ToastLength.Short).Show();
            }
        }

        public void InitializeReferences()
        {

            if (!GetConnectionStatus())
            {
                GetLocalReferences();

                //UserSession.Agents = new System.Collections.Generic.List<AgentModel>();
                //UserSession.AgentsStringList = new System.Collections.Generic.List<string>();

                //UserSession.OpenLots = new System.Collections.Generic.List<LotModel>();
                //UserSession.OpenLotsStringList = new System.Collections.Generic.List<string>();

                //UserSession.ShippedLots = new System.Collections.Generic.List<LotModel>();
                //UserSession.ShippedLotsStringList = new System.Collections.Generic.List<string>();

                //UserSession.Locations = new System.Collections.Generic.List<LocationModel>();
                //UserSession.LocationsStringList = new System.Collections.Generic.List<string>();

                return;
            }

            // Agents
            UserSession.Agents =  _genericService.GetAgentList();
            UserSession.AgentsStringList = UserSession.Agents.Select(x => x.Value).ToList();

            // Open Lots
            UserSession.OpenLots = _genericService.GetOpenLotList();
            UserSession.OpenLotsStringList = UserSession.OpenLots.Select(x => x.Value).ToList();

            // Open Shipped Lots
            UserSession.ShippedLots = _genericService.GetShippedLotList();
            UserSession.ShippedLotsStringList = UserSession.ShippedLots.Select(x => x.Value).ToList();

            // Location
            UserSession.Locations = _genericService.GetLocationList();
            UserSession.LocationsStringList = UserSession.Locations.Select(x => x.Value).ToList();
        }

        public void GetLocalReferences()
        {
            try
            {
                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                db.CreateTable<ReferenceTable>();

                var data = db.Table<ReferenceTable>(); //Call Reference Table 

                // Agents
                UserSession.Agents = data.Where(x => x.Type == (int)ReferenceTypeEnums.AGENT).Select(x => new AgentModel
                {
                    GUID = x.Guid,
                    Type = x.Type,
                    Value = x.Value
                }).ToList();

                UserSession.AgentsStringList = UserSession.Agents.Select(x => x.Value).ToList();

                // Lots
                UserSession.OpenLots = data.Where(x => x.Type == (int)ReferenceTypeEnums.OPEN_LOT).Select(x => new LotModel
                {
                    GUID = x.Guid,
                    Type = x.Type,
                    Value = x.Value
                }).ToList();
                UserSession.OpenLotsStringList = UserSession.OpenLots.Select(x => x.Value).ToList();

                // Open Shipped Lots
                UserSession.ShippedLots = data.Where(x => x.Type == (int)ReferenceTypeEnums.SHIPPED_LOT).Select(x => new LotModel
                {
                    GUID = x.Guid,
                    Type = x.Type,
                    Value = x.Value
                }).ToList();
                UserSession.ShippedLotsStringList = UserSession.ShippedLots.Select(x => x.Value).ToList();

                // Location       
                UserSession.Locations = data.Where(x => x.Type == (int)ReferenceTypeEnums.LOCATION_ID).Select(x => new LocationModel
                {
                    GUID = x.Guid,
                    Type = x.Type,
                    Value = x.Value
                }).ToList();
                UserSession.LocationsStringList = UserSession.Locations.Select(x => x.Value).ToList();

            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error", ToastLength.Short).Show();
            }
        }

        public void InitializerReferenceTable()
        {
            try
            {
                if (!GetConnectionStatus())
                    return;

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

               // db.DropTable<ReferenceTable>();
                db.CreateTable<ReferenceTable>();

                var data = db.Table<ReferenceTable>(); //Call Reference Table 


                // Get All References
                var references = _genericService.GetReferences();

                foreach (var reference in references)
                {
                    var entity = data.Where(x => x.Guid == reference.GUID).FirstOrDefault(); //Linq Query  

                    if (entity == null)
                    {
                        // insert new item
                        ReferenceTable tbl = new ReferenceTable
                        {
                            Guid = reference.GUID,
                            Value = reference.Value,
                            Type = reference.Type
                        };

                        db.Insert(tbl);
                    }
                }

                // ADD NONE  - AGENT
                ReferenceTable tblAgentTemp = new ReferenceTable
                {
                    Guid = Guid.Empty,
                    Value = "None",
                    Type = (int)ReferenceTypeEnums.AGENT
                };

                var entity1 = data.Where(x => x.Value == tblAgentTemp.Value && x.Type == (int)ReferenceTypeEnums.AGENT).FirstOrDefault(); //Linq Query  

                if (entity1 == null)
                {
                    // insert new item
                    db.Insert(tblAgentTemp);
                }

                // ADD ""  - OPEN LOT
                ReferenceTable tblLotTemp = new ReferenceTable
                {
                    Guid = Guid.Empty,
                    Value = "",
                    Type = (int)ReferenceTypeEnums.OPEN_LOT
                };

                entity1 = data.Where(x => x.Value == tblLotTemp.Value && x.Type == (int)ReferenceTypeEnums.OPEN_LOT).FirstOrDefault(); //Linq Query  

                if (entity1 == null)
                {
                    // insert new item
                    db.Insert(tblLotTemp);
                }

                // ADD ""  - SHIPPED LOT
                ReferenceTable tblShipLotTemp = new ReferenceTable
                {
                    Guid = Guid.Empty,
                    Value = "",
                    Type = (int)ReferenceTypeEnums.SHIPPED_LOT
                };

                entity1 = data.Where(x => x.Value == tblShipLotTemp.Value && x.Type == (int)ReferenceTypeEnums.SHIPPED_LOT).FirstOrDefault(); //Linq Query  

                if (entity1 == null)
                {
                    // insert new item
                    db.Insert(tblShipLotTemp);
                }

            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error", ToastLength.Short).Show();
            }
        }

        public void InitializeUserTable()
        {
            try
            {
                if (!GetConnectionStatus())
                    return;

                string dpPath = UserSession.DB_PATH;
                var db = new SQLiteConnection(dpPath);

                // Drop Table
                // db.DropTable<UserTable>();
                // Create Table
                db.CreateTable<UserTable>();

                var localUserTable = db.Table<UserTable>(); 

                var usersDB = _userService.GetUsers();

               foreach(var userDB in usersDB)
               {
                    var localUser = localUserTable.Where(x => x.GUID == userDB.GUID).FirstOrDefault(); //Linq Query  

                    // user not found in localUserTable
                    if (localUser == null)
                    {
                        // insert user in local db
                        UserTable tbl = new UserTable
                        {
                            FirstName = userDB.Name_First,
                            LastName = userDB.Name_Last,
                            Email = userDB.Email,
                            Role = userDB.Role,
                            Location = userDB.Location,
                            LocationStr = userDB.LocationStr,
                            GUID = userDB.GUID,
                            Password = userDB.Password
                        };

                        db.Insert(tbl);
                    }
                    else
                    {
                        localUser.Password = userDB.Password;
                        localUser.Email = userDB.Email;
                        localUser.FirstName = userDB.Name_First;
                        localUser.LastName = userDB.Name_Last;
                        localUser.Role = userDB.Role;
                        localUser.Location = userDB.Location;
                        localUser.LocationStr = userDB.LocationStr;

                        db.Update(localUser);
                    }
                }

                RemoveUnknownLocalUserFromDB();

               // Toast.MakeText(this, "Record Added Successfully...,", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
              //  throw;
                Toast.MakeText(this, "Error", ToastLength.Short).Show();
            }
        }

        public string CreateDB()
        {
            var output = "";
            output += "Creating Databse if it doesnt exists";
            string dpPath = UserSession.DB_PATH; 
            var db = new SQLiteConnection(dpPath);
            output += "\n Database Created....";
            return output;
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

