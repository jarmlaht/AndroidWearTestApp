using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Content;
using Android.Gms.Wearable;
using Android.Gms.Common.Apis;

namespace WearTestApp
{
    [Activity(Label = "MobileTestApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MobileMainActivity : Activity
    {
        TextView txtMsg;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource  
            SetContentView(Resource.Layout.Main);

            // Get our TextBox from the layout resource,  
            txtMsg = FindViewById<TextView>(Resource.Id.txtMessage);
            txtMsg.Text = "Waiting messages...";

            IntentFilter filter = new IntentFilter(Intent.ActionSend);
            MessageReceiver receiver = new MessageReceiver(this);
            LocalBroadcastManager.GetInstance(this).RegisterReceiver(receiver, filter);

            Button button = FindViewById<Button>(Resource.Id.buttonSend);
            button.Click += delegate {
                WearService wearService = new WearService();
                wearService.SendData();
            };
        }

        public void ProcessMessage(Intent intent)
        {
            txtMsg.Text = intent.GetStringExtra("WearMessage");
        }

        internal class MessageReceiver : BroadcastReceiver
        {
            MobileMainActivity main;
            public MessageReceiver(MobileMainActivity owner) { this.main = owner; }
            public override void OnReceive(Context context, Intent intent)
            {
                main.ProcessMessage(intent);
            }
        }
    }
}

