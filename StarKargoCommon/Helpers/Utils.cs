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
using System.Reflection;
using StarKargoCommon.Enumerations;
using System.Security.Cryptography;

namespace StarKargoCommon.Helpers
{

    public class StringValue : System.Attribute
    {
        private readonly string _value;

        public StringValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }


    public static class Utils
    {
        public static string TOKEN = "33C861DD-95A0-4E59-AAD5-1F5E11D769C1";
      //  public static string BASE_PATH = "http://adensol.selfip.com/ABSServer/";
        public static string BASE_PATH = "http://63.68.150.63/"; 
        public static string GET_REFERENCES = "mobile/v1/reference";
        public static string GET_USERS = "mobile/v1/users";
        public static string POST_USERS = "mobile/v1/users";
        public static string POST_ORDERS = "mobile/v1/order";
        public static string GET_ORDERS = "mobile/v1/order";

        public static string GetRoleTypeValue(int value)
        {
            string retVal = String.Empty;

            switch (value)
            {
                case 0:
                    retVal = GetStringValue(RoleTypeEnums.Administrator);
                    break;
                case 1:
                    retVal = GetStringValue(RoleTypeEnums.Warehouse);
                    break;
                case 2:
                    retVal = GetStringValue(RoleTypeEnums.Manila);
                    break;
            }

            return retVal;
        }

        public static string GetStatusTypeValue(int value)
        {
            string retVal = String.Empty;

            switch (value)
            {
                case 0:
                    retVal = GetStringValue(StatusTypeEnums.RECEIVED);
                    break;
                case 1:
                    retVal = GetStringValue(StatusTypeEnums.LOADED);
                    break;
                case 2:
                    retVal = GetStringValue(StatusTypeEnums.SHIPPED);
                    break;
                case 3:
                    retVal = GetStringValue(StatusTypeEnums.UNLOADED);
                    break;
                case 4:
                    retVal = GetStringValue(StatusTypeEnums.DELIVERED);
                    break;
            }

            return retVal;
        }


        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            //Check first in our cached results...

            //Look for our 'StringValueAttribute' 

            //in the field's custom attributes

            FieldInfo fi = type.GetField(value.ToString());
            StringValue[] attrs =
               fi.GetCustomAttributes(typeof(StringValue),
                                       false) as StringValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }

        public static byte[] SHA256Hash(byte[] bytesToHash, uint salt)
        {
            var b = SHA256Hash(bytesToHash);
            b[(byte)(salt & 15)] = (byte)((b[(byte)(salt & 15)] | 0x80) & (salt / 0x1000000));
            return b;
        }

        public static byte[] SHA256Hash(byte[] bytesToHash)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] secretKey = encoding.GetBytes("12345678901234567890");
            using (HMACSHA1 hashAlgorithm = new HMACSHA1(secretKey))
            {
                return hashAlgorithm.ComputeHash(bytesToHash);
            }
            //using (SHA256 csp = new SHA256CryptoServiceProvider())
            //{
            //    return csp.ComputeHash(bytesToHash);
            //}
        }

        public static string GeneratePassword(string userName, string password)
        {
            var xors = new byte[] { 10, 44, 134, 18, 2, 210, 80, 3 };
            var bytesToHash =
                Encoding.Unicode.GetBytes(password + userName.ToUpper()).Select(b => (byte)(b ^ xors[b % 8])).ToArray();
            byte[] retVal =  SHA256Hash(bytesToHash, 4278190080);
            return Convert.ToBase64String(retVal);
        }

    }
}