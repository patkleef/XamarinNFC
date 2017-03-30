using System.Text;
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;

namespace XamarinNFC
{
    [Activity(Label = "XamarinNFC", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private NfcAdapter _nfcAdapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (_nfcAdapter == null)
            {
                var alert = new AlertDialog.Builder(this).Create();
                alert.SetMessage("NFC is not supported on this device.");
                alert.SetTitle("NFC Unavailable");
                alert.Show();
            }
            else
            {
                var tagDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
                var filters = new[] { tagDetected };

                var intent = new Intent(this, this.GetType()).AddFlags(ActivityFlags.SingleTop);

                var pendingIntent = PendingIntent.GetActivity(this, 0, intent, 0);

                _nfcAdapter.EnableForegroundDispatch(this, pendingIntent, filters, null);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent.Action == NfcAdapter.ActionTagDiscovered)
            {
                var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
                if (tag != null)
                {
                    // First get all the NdefMessage
                    var rawMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
                    if (rawMessages != null)
                    {
                        var msg = (NdefMessage)rawMessages[0];

                        // Get NdefRecord which contains the actual data
                        var record = msg.GetRecords()[0];
                        if (record != null)
                        {
                            if (record.Tnf == NdefRecord.TnfWellKnown) // The data is defined by the Record Type Definition (RTD) specification available from http://members.nfc-forum.org/specs/spec_list/
                            {
                                // Get the transfered data
                                var data = Encoding.ASCII.GetString(record.GetPayload());
                            }
                        }
                    }
                }
            }
        }

        public void WriteToTag(Intent intent, string content)
        {
            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            if (tag != null)
            {
                Ndef ndef = Ndef.Get(tag);
                if (ndef != null && ndef.IsWritable)
                {
                    var payload = Encoding.ASCII.GetBytes(content);
                    var mimeBytes = Encoding.ASCII.GetBytes("text/plain");
                    var record = new NdefRecord(NdefRecord.TnfWellKnown, mimeBytes, new byte[0], payload);
                    var ndefMessage = new NdefMessage(new[] { record });

                    ndef.Connect();
                    ndef.WriteNdefMessage(ndefMessage);
                    ndef.Close();
                }
            }
        }
    }
}

