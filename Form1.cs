using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.FFMPEG;
using System.IO;
using System.Net;
using System.Drawing.Imaging;

namespace StreamVideo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Camera parameters
        private VideoCaptureDevice Camera = null;
        //Encode each frame of image to video file
        private VideoFileWriter videofilewriter = new VideoFileWriter();
        //Image cache
        private Bitmap bmp;
        private Bitmap bmpRe;

        public byte[] byteImage;
        public byte[] byteImage_Back;

        FilterInfoCollection filterInfoCollection;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Camera.IsRunning == true)
                //Stop camera
                Camera.Stop();
                //Close the video file, if you forget to close it, you will get a damaged file that cannot be played
                videofilewriter.Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Camera = new VideoCaptureDevice(filterInfoCollection[cboCamera.SelectedIndex].MonikerString);
            Camera.NewFrame += VideoCaptureDevice_NewFrame;
            Camera.Start();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pic.Image = (Bitmap)eventArgs.Frame.Clone();
            bmp = (Bitmap)eventArgs.Frame.Clone();

            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, ImageFormat.Bmp);
            byteImage = new Byte[memoryStream.Length];
            byteImage = memoryStream.ToArray();

            //Required API
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("Your URL");
            //Requested http verb
            request.Method = "POST";
            //Requested content type
            request.ContentType = "video/mp4";
            //Convert the data content to be post to stream
            using (Stream streamWriter = request.GetRequestStream())
            {
                streamWriter.Write(byteImage, 0, byteImage.Length);
            }
            //Send Request
            using (WebResponse response = request.GetResponse())
            {
                using (MemoryStream getmemory = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(getmemory);
                    byteImage_Back = getmemory.ToArray();

                    using (MemoryStream ms = new MemoryStream(byteImage_Back))
                    {
                        bmpRe = new Bitmap(ms);
                        pic2.Image = (Bitmap)bmpRe.Clone();
                    }
                }//end using  
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
                cboCamera.Items.Add(filterInfo.Name);
            cboCamera.SelectedIndex = 0;
            Camera = new VideoCaptureDevice();
        }
    }
}
