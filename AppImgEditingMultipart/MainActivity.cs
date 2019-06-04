using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp;

namespace AppImgEditingMultipart
{
    [Activity(Label = "AppImgEditingMultipart", MainLauncher = true)]
    public class MainActivity : Activity
    {
        FingerPaintCanvasView fingerPaintCanvasView;
        Bitmap bitmapOriginal;
        EditText txtNote;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.Main);
                // Get a reference to the FingerPaintCanvasView from the Main.axml file
                fingerPaintCanvasView = FindViewById<FingerPaintCanvasView>(Resource.Id.canvasView);

                // Set up the Spinner to select stroke color
                Spinner colorSpinner = FindViewById<Spinner>(Resource.Id.colorSpinner);
                colorSpinner.ItemSelected += OnColorSpinnerItemSelected;

                var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.colors_array, Android.Resource.Layout.SimpleSpinnerItem);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                colorSpinner.Adapter = adapter;

                // Set up the Clear button
                Button btnClear = FindViewById<Button>(Resource.Id.btnClear);
                btnClear.Click += BtnClear_Click;

                //Open camera and click picture and save background
                Button btnCamera = FindViewById<Button>(Resource.Id.btnCamera);
                btnCamera.Click += BtnCamera_Click;

                //Save Image To Local
                Button btnSave = FindViewById<Button>(Resource.Id.btnSave);
                btnSave.Click += BtnSave_Click;

                // Go To Next Activity
                Button btnNext = FindViewById<Button>(Resource.Id.btnNext);
                btnNext.Click += BtnNext_Click;

                txtNote = FindViewById<EditText>(Resource.Id.txtNote);
            }
            catch (Exception ex) { Toast.MakeText(this, ex.Message, ToastLength.Long).Show(); }
        }
        private void BtnNext_Click(object sender, EventArgs e)
        {


        }

        //Conver image to base 64 and save
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap imgbitmap = Bitmap.CreateBitmap(fingerPaintCanvasView.Width, fingerPaintCanvasView.Height, Bitmap.Config.Rgb565);
                Canvas canvas = new Canvas(imgbitmap);
                fingerPaintCanvasView.Draw(canvas);
                string s = await SaveAsync(imgbitmap);
                //Convert to bitmap to base64string
                using (var stream = new MemoryStream())
                {
                    imgbitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);

                    var bytes = stream.ToArray();
                    //imageEditBase64.EditImgString = Convert.ToBase64String(bytes);
                }
                //Add both string to list
               // _ListImgBase64.Add(imageEditBase64);

                Toast.MakeText(this, "Image saved", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
        private const string UPLOAD_IMAGE = "http://10.91.6.23/testapi/api/products/PatientImg";
      

        public object MediaTypeHeaderValue { get; private set; }

        private async Task<String> SaveAsync(Bitmap bitmap)
        {
            var progressDialog = ProgressDialog.Show(this, "", "Connecting...", true);
            try
            {
                byte[] bitmapData;
                var stream = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                bitmapData = stream.ToArray();
                var fileContent = new ByteArrayContent(bitmapData);

                fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/octet-stream");
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = "my_uploaded_image.png"
                };

                string boundary = "---8d0f01e6b3b5dafaaadaad";
                MultipartFormDataContent multipartContent = new MultipartFormDataContent(boundary);
                multipartContent.Add(fileContent);

                HttpClient httpClient = new HttpClient();
                var response = await httpClient.PostAsync(UPLOAD_IMAGE, multipartContent);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                   // return content;
                }
                progressDialog.Hide();
                return null;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                return null;
            }
        }
        private void BtnCamera_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            StartActivityForResult(intent, 0);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (resultCode == Result.Ok)
                {
                    bitmapOriginal = (Bitmap)data.Extras.Get("data");
                    fingerPaintCanvasView.SetBitMap(bitmapOriginal);
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
        private void BtnClear_Click(object sender, EventArgs e)
        {
            try
            {
                fingerPaintCanvasView.ClearAll();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        void OnColorSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs args)
        {
            try
            {
                Spinner spinner = (Spinner)sender;
                string strColor = (string)spinner.GetItemAtPosition(args.Position);
                Color strokeColor = (Color)(typeof(Color).GetProperty(strColor).GetValue(null));
                fingerPaintCanvasView.StrokeColor = strokeColor;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }
    }
}

