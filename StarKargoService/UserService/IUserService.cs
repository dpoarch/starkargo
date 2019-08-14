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

namespace StarKargoService.UserService
{
    public interface IUserService
    {
        bool CreateUser(UserModel model);
        bool UpdateUser(UserModel model);
        bool DeleteUser(UserModel model);
        bool ChangePassword(ChangePasswordModel model);
        List<UserModel> GetUsers();
    }
}