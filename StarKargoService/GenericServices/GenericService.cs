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
using System.Net;
using Newtonsoft.Json;
using StarKargoCommon.Helpers;
using StarKargoCommon.Enumerations;

namespace StarKargoService.GenericServices
{
    public class GenericService : IGenericService
    {
        public List<LotModel> GetOpenLotList()
        {

            List<LotModel> filterResults = new List<LotModel>();

            // Add default 
            filterResults.Add(new LotModel
            {
                GUID = Guid.Empty,
                Type = (int)ReferenceTypeEnums.OPEN_LOT,
                Value = ""
            });

            // TYPE = 1
            var results = GetReferences();

            if (results != null)
            {
                var entityResults = results.Where(x => x.Type == (int)ReferenceTypeEnums.OPEN_LOT).
                     Select(x => new LotModel
                     {
                         GUID = x.GUID,
                         Type = x.Type,
                         Value = x.Value
                     }).ToList();

                filterResults.AddRange(entityResults);

                return filterResults;
            }

            return null;
        }

        public List<AgentModel> GetAgentList()
        {
            List<AgentModel> filterResults = new List<AgentModel>();

            // Add default 
            filterResults.Add(new AgentModel
            {
                GUID = Guid.Empty,
                Type = (int)ReferenceTypeEnums.AGENT,
                Value = "None"
            });

            // TYPE = 0
            var results = GetReferences();

            if (results != null)
            {
                var entityResults = results.Where(x => x.Type == (int)ReferenceTypeEnums.AGENT).
                     Select(x => new AgentModel
                     {
                         GUID = x.GUID,
                         Type = x.Type,
                         Value = x.Value
                     }).ToList();

                filterResults.AddRange(entityResults);

                return filterResults;
            }

            return null;
        }

        public List<LotModel> GetShippedLotList()
        {
            List<LotModel> filterResults = new List<LotModel>();

            // Add default 
            filterResults.Add(new LotModel
            {
                GUID = Guid.Empty,
                Type = (int)ReferenceTypeEnums.SHIPPED_LOT,
                Value = ""
            });

            // TYPE = 2
            var results = GetReferences();

            if (results != null)
            {
                var entityResults = results.Where(x => x.Type == (int)ReferenceTypeEnums.SHIPPED_LOT).
                     Select(x => new LotModel
                     {
                         GUID = x.GUID,
                         Type = x.Type,
                         Value = x.Value
                     }).ToList();

                filterResults.AddRange(entityResults);

                return filterResults;
            }

            return null;
        }

        public List<LocationModel> GetLocationList()
        {
            // TYPE = 2
            var results = GetReferences();

            if (results != null)
            {
                var filterResults = results.Where(x => x.Type == (int)ReferenceTypeEnums.LOCATION_ID).
                     Select(x => new LocationModel
                     {
                         GUID = x.GUID,
                         Type = x.Type,
                         Value = x.Value
                     }).ToList();

                return filterResults;
            }

            return null;
        }

        public List<DataModel> GetReferences()
        {
            List<DataModel> retVal = null;

            using (var client = new WebClient())
            {
                try
                {
                    client.Headers.Add("Token", Utils.TOKEN);

                    string URL = string.Format("{0}{1}", Utils.BASE_PATH, Utils.GET_REFERENCES);
                    var response = client.DownloadString(URL);
                    ResponseReference account = JsonConvert.DeserializeObject<ResponseReference>(response);
                    retVal = account.Data;

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