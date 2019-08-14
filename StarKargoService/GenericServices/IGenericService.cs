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

namespace StarKargoService.GenericServices
{
    public interface IGenericService
    {
        List<DataModel> GetReferences();

        List<LotModel> GetOpenLotList();

        List<LotModel> GetShippedLotList();

        List<AgentModel> GetAgentList();

        List<LocationModel> GetLocationList();
    }
}