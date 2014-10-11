using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MessagingToolkit.QRCode.Codec;
using MessagingToolkit.QRCode.Codec.Data;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace qrcode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new BitmapImage();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".png";
            ofd.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                Image1.Source = new BitmapImage(new Uri(ofd.FileName));
                Image1.Tag = ofd.FileName;
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                QRCodeDecoder dec = new QRCodeDecoder();
                TextBlock1.Text = dec.Decode(new QRCodeBitmapImage(new Bitmap(Image1.Tag.ToString())));

            }
            catch
            {
                TextBlock1.Text = "Wrong image!";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            string yoururl = TextBlock1.Text.ToString();
            MessagingToolkit.QRCode.Codec.QRCodeEncoder qe = new MessagingToolkit.QRCode.Codec.QRCodeEncoder();
            System.Drawing.Bitmap bm = qe.Encode(yoururl);
            using (MemoryStream memory = new MemoryStream())
            {
                bm.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                ImageSource imageSource = bitmapImage;

                Image1.Source = imageSource;
                DataContext = bitmapImage;
            }



        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Filter = "Text documents (.jpg)|*.jpg"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                var encoder = new JpegBitmapEncoder(); // Or PngBitmapEncoder, or whichever encoder you want
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)DataContext));
                using (var stream = dlg.OpenFile())
                {
                    encoder.Save(stream);
                }
            }

        }
    }
}