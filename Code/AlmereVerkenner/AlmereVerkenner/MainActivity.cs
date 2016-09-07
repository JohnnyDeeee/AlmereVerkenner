using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Hardware;
using Android.Graphics;
using System.IO;
using ZXing.Mobile;
using System.Threading.Tasks;
using Android.Locations;

namespace AlmereVerkenner
{
    [Activity(Label = "Almere Verkenner", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Android.Hardware.Camera camera;
        private Button captureButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set view to home screen
            SetContentView(Resource.Layout.Main);

            // Set the camer view
            //textureView = this.FindViewById<TextureView>(Resource.Id.cameraView);
            //textureView.SurfaceTextureListener = this;

            // Add the capture button event
            captureButton = this.FindViewById<Button>(Resource.Id.captureButton);
            captureButton.Click += CaptureButton_Click;

            // Initialize scanner (for android only)
            MobileBarcodeScanner.Initialize(Application);
        }

        private async void CaptureButton_Click(object sender, EventArgs e)
        {
            // Capture/Read QR code
            object result = await ReadQRCode();
            Toast.MakeText(this, "Result: " + result.ToString(), ToastLength.Short).Show();            
                       
            // Check QR result with current GPS position
            
            
            // Add point if OK
        }

        private async Task<object> ReadQRCode()
        {
            MobileBarcodeScanner scanner = new MobileBarcodeScanner();
            scanner.UseCustomOverlay = false;
            scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
            scanner.BottomText = "Wait for the barcode to automatically scan!";
            return await scanner.Scan();
        }

        private bool CompareLocation(string longitude, string latitude)
        {
            LocationManager locationManager = (LocationManager)GetSystemService(Context.LocationService);
            Criteria crit = new Criteria();
            // TODO: set criteria to acc_fine

            // Check if GPS is enabled
            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider) ||
                locationManager.IsProviderEnabled(LocationManager.NetworkProvider))
            {
                locationManager.RequestSingleUpdate(locationManager.GetBestProvider(), 11, null);
            }
        }
    }
}

