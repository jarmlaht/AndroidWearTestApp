using System;

using Android.App;
//using Android.Runtime;
using Android.Widget;
using Android.OS;
using Android.Support.Wearable.Views;
using Android.Content;
using Java.Interop;
using Android.Gms.Common.Apis;
using Android.Gms.Wearable;
using Android.Gms.Common;
using System.Linq;
using Android.Support.V4.Content;

namespace WearTestApp
{
    [Activity(Label = "WearTestApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class WearMainActivity : Activity, IDataApiDataListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        private GoogleApiClient client;
        const string syncPathWatch = "/WearTestApp/Watch";
        const string syncPathPhone = "/WearTestApp/Phone";
        TextView txtMsg;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            client = new GoogleApiClient.Builder(this, this, this).AddApi(WearableClass.API).Build();
            client.Connect();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            IntentFilter filter = new IntentFilter(Intent.ActionSend);
            MessageReceiver receiver = new MessageReceiver(this);
            LocalBroadcastManager.GetInstance(this).RegisterReceiver(receiver, filter);

            var v = FindViewById<WatchViewStub>(Resource.Id.watch_view_stub);
            v.LayoutInflated += delegate
            {
                // Get our TextBox from the layout resource,  
                txtMsg = FindViewById<TextView>(Resource.Id.txtMessage);
                txtMsg.Text = "Waiting messages...";

                // Get our button from the layout resource,
                // and attach an event to it
                Button button = FindViewById<Button>(Resource.Id.myButton);

                button.Click += delegate {
                    SendData();
                };
            };
        }

        public void SendData()
        {
            Android.Util.Log.Info("INFO", "SendData");
            try
            {
                var request = PutDataMapRequest.Create(syncPathWatch);
                var map = request.DataMap;
                map.PutString("Message", "Hello from Wearable!");
                map.PutLong("UpdatedAt", DateTime.UtcNow.Ticks);
                WearableClass.DataApi.PutDataItem(client, request.AsPutDataRequest());
                Android.Util.Log.Info("INFO", "SendData: " + client.IsConnected + ", " + request.DataMap.GetString("Message"));
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

        protected override void OnStart()
        {
            base.OnStart();
            client.Connect();
        }

        protected override void OnStop()
        {
            base.OnStop();
            client.Disconnect();
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

        public void OnDataChanged(DataEventBuffer dataEvents)
        {
            var dataEvent = Enumerable.Range(0, dataEvents.Count)
                                      .Select(i => dataEvents.Get(i).JavaCast<IDataEvent>())
                                      .FirstOrDefault(x => x.Type == DataEvent.TypeChanged && x.DataItem.Uri.Path.Equals(syncPathPhone));
            if (dataEvent == null)
                return;

            //get data from mobile phone  
            var dataMapItem = DataMapItem.FromDataItem(dataEvent.DataItem);
            var map = dataMapItem.DataMap;
            string message = dataMapItem.DataMap.GetString("Message") + "\n" + dataMapItem.DataMap.GetLong("UpdatedAt");
            Android.Util.Log.Info("INFO", "OnDataChanged: " + message);

            Intent intent = new Intent();
            intent.SetAction(Intent.ActionSend);
            intent.PutExtra("WearMessage", message);
            LocalBroadcastManager.GetInstance(this).SendBroadcast(intent);
        }

        public void ProcessMessage(Intent intent)
        {
            txtMsg.Text = intent.GetStringExtra("WearMessage");
        }

        internal class MessageReceiver : BroadcastReceiver
        {
            WearMainActivity main;
            public MessageReceiver(WearMainActivity owner) { this.main = owner; }
            public override void OnReceive(Context context, Intent intent)
            {
                main.ProcessMessage(intent);
            }
        }
    }
}


