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
using System.Collections.Generic;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Common;

namespace AlmereVerkenner
{
    [Activity(Label = "Almere Verkenner", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, Android.Gms.Location.ILocationListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        private Button captureButton;
        private Location currentLocation;
        private object qrResult;
        private static GoogleApiClient googleApiClient;

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

            // Create Google API instance
            if(googleApiClient == null)
                googleApiClient = new GoogleApiClient.Builder(this).AddConnectionCallbacks(this).AddOnConnectionFailedListener(this).AddApi(LocationServices.API).Build();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnStop()
        {
            if (googleApiClient.IsConnected || googleApiClient.IsConnecting)
                googleApiClient.Disconnect();
            base.OnStop();
        }

        private async void CaptureButton_Click(object sender, EventArgs e)
        {
            // Capture/Read QR code
            qrResult = await ReadQRCode();
            //Toast.MakeText(this, "Result: " + qrResult.ToString(), ToastLength.Short).Show();

            // Check QR result with current GPS position
            Toast.MakeText(this, "Connecting to GooglePlayServices...", ToastLength.Long).Show();
            googleApiClient.Connect();

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

        private void StartLocationUpdate()
        {
            // Use google location api
            LocationRequest request = new LocationRequest();
            request.SetPriority(LocationRequest.PriorityHighAccuracy);
            request.SetInterval(5000);
            LocationServices.FusedLocationApi.RequestLocationUpdates(googleApiClient, request, this);
        }

        #region Android GPS
        public void OnLocationChanged(Location location)
        {
            // TODO: Check GPS location with QR result
            currentLocation = location;
            //Toast.MakeText(this, "currentLocation = " + location, ToastLength.Long).Show(); // DEBUG
            LocationServices.FusedLocationApi.RemoveLocationUpdates(googleApiClient, this);

            string[] qr_array = qrResult.ToString().Split(',');
            double qr_long = Convert.ToDouble(qr_array[0]);
            double qr_lat = Convert.ToDouble(qr_array[1]);
            double distance = CalcDistance(qr_lat, qr_long, location.Latitude, location.Longitude, 'K');

            if (true /*distance < 1000*/) // Not sure if this is a good radius (in meters)
            {
                // User is within ?1? km radius with the qr sticker
                // Add point to account
                Toast.MakeText(this, "Added point!", ToastLength.Short).Show();
                string pointsText = this.FindViewById<TextView>(Resource.Id.pointsLabel).Text;
                string[] array = pointsText.Split(':');
                int points = Convert.ToInt32(array[1].TrimStart(' '));
                points++;
                pointsText = "Points: " + points;
                this.FindViewById<TextView>(Resource.Id.pointsLabel).Text = pointsText;
            }
            else
            {
                Toast.MakeText(this, "Hacker! Distance = " + distance, ToastLength.Short).Show();
            }
        }

        private static Double rad2deg(Double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        private static Double deg2rad(Double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private double CalcDistance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            //double theta = lon1 - lon2;
            //double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            //dist = Math.Acos(dist);
            //dist = rad2deg(dist);
            //dist = dist * 60 * 1.1515;
            //if (unit == 'K')
            //{
            //    dist = dist * 1.609344;
            //}
            //else if (unit == 'N')
            //{
            //    dist = dist * 0.8684;
            //}
            //return (dist);

            int R = 6371000; // metres
            var φ1 = deg2rad(lat1);
            var φ2 = deg2rad(lat2);
            var Δφ = deg2rad(lat2 - lat1);
            var Δλ = deg2rad(lon2 - lon1);

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var d = R * c;
            return d;
        }

        public void OnProviderDisabled(string provider)
        {
            Console.WriteLine("Provider: " + provider + " = disabled!");
            Toast.MakeText(this, "Please enable your Location service in you phone Settings!", ToastLength.Long).Show();
        }

        public void OnProviderEnabled(string provider)
        {
            Console.WriteLine("Provider: " + provider + " = enabled!");
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Google Play Service: Location API
        public void OnConnected(Bundle connectionHint)
        {
            //Toast.MakeText(this, "Connected to GooglePlayServices! (" + googleApiClient.GetConnectionResult(LocationServices.API).ToString() + ")", ToastLength.Short).Show();
            StartLocationUpdate();
        }

        public void OnConnectionSuspended(int cause)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Toast.MakeText(this, "Could not connect to GoogelPlayerServices! Result: " + result.ErrorMessage, ToastLength.Short);
        }
        #endregion
    }
}

