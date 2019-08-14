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

namespace StarKargoService.TransactionService
{
    public interface ITransactionService
    {
        bool DeliverPackage(DeliverPackageModel model);

        bool ReceivePackage(ReceivePackageModel model);

        bool LoadContainer(LoadContainerModel model);

        bool UnloadContainer(UnloadContainerModel model);

        CheckPackageModel CheckPackageOrder(string barcode);
    }
}