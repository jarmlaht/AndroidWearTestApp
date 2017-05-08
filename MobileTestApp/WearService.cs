using System.Linq;
using Android.App;
using Android.Content;
//using Android.Runtime;
using Android.Gms.Wearable;
using Android.Gms.Common.Apis;
using Android.Support.V4.Content;
using System;
using Java.Interop;
using Android.OS;
using Android.Gms.Common;

namespace WearTestApp
{
    [Service]
    [IntentFilter(new[] { "com.google.android.gms.wearable.BIND_LISTENER" })]
    public class WearService : WearableListenerService, IDataApiDataListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        const string syncPathWatch = "/WearTestApp/Watch";
        const string syncPathPhone = "/WearTestApp/Phone";
        static GoogleApiClient client;

        public override void OnCreate()
        {
            base.OnCreate();
            client = new GoogleApiClient.Builder(this.ApplicationContext)
                    .AddApi(WearableClass.API)
                    .Build();

            client.Connect();

            Android.Util.Log.Info("INFO", "WearIntegrationreated");
        }

        public override void OnDataChanged(DataEventBuffer dataEvents)
        {
            Android.Util.Log.Info("INFO", "OnDataChanged");
            var dataEvent = Enumerable.Range(0, dataEvents.Count)
                                      .Select(i => dataEvents.Get(i).JavaCast<IDataEvent>())
                                      .FirstOrDefault(x => x.Type == DataEvent.TypeChanged && x.DataItem.Uri.Path.Equals(syncPathWatch));
            if (dataEvent == null)
            {
                Android.Util.Log.Info("INFO", "OnDataChanged: message = null!");
                return;
            }

            //get data from wearable  
            var dataMapItem = DataMapItem.FromDataItem(dataEvent.DataItem);
            var map = dataMapItem.DataMap;
            string message = dataMapItem.DataMap.GetString("Message") + "\n" + dataMapItem.DataMap.GetLong("UpdatedAt");
            Android.Util.Log.Info("INFO", "OnDataChanged: " + message);

            Intent intent = new Intent();
            intent.SetAction(Intent.ActionSend);
            intent.PutExtra("WearMessage", message);
            LocalBroadcastManager.GetInstance(this).SendBroadcast(intent);
        }

        public void SendData()
        {
            Android.Util.Log.Info("INFO", "SendData");
            try
            {
                var request = PutDataMapRequest.Create(syncPathPhone);
                var map = request.DataMap;
                map.PutString("Message", "Hello from Phone!");
                map.PutLong("UpdatedAt", DateTime.UtcNow.Ticks);
                WearableClass.DataApi.PutDataItem(client, request.AsPutDataRequest());
                Android.Util.Log.Info("INFO", "SendData: " + client.IsConnected);
            }
            catch (Exception e)
            {
                Android.Util.Log.Error("ERROR", "SendData: " + e.Message);
            }
            /*finally
            {
                client.Disconnect();
            }*/
        }

        public void OnConnected(Bundle p0)
        {
            WearableClass.DataApi.AddListener(client, this);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Android.Util.Log.Error("ERROR", "GMSonnection failed " + result.ErrorCode);
        }

        public void OnConnectionSuspended(int reason)
        {
            Android.Util.Log.Error("ERROR", "GMSonnection suspended " + reason);
            WearableClass.DataApi.RemoveListener(client, this);
        }
    }
}