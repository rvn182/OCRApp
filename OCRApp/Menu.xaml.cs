using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using Windows.UI.Popups;
using System.Text.RegularExpressions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OCRApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static string price="";
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void buttonAddPrice_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            //captureUI.PhotoSettings.CroppedSizeInPixels = new Size(300, 300);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }

            SoftwareBitmap bitmapa = null;
            OcrEngine silnik = OcrEngine.TryCreateFromUserProfileLanguages();

            using (var stream = await photo.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                bitmapa = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            }
            OcrResult rezultat = await silnik.RecognizeAsync(bitmapa);
            try
            {
                price = ParsePrice(rezultat.Text);
                this.Frame.Navigate(typeof(AddPrice));
            }
            catch(Exception exception)
            {
                MessageDialog message = new MessageDialog(exception.ToString()+"\n Sparsowny text: "+rezultat.Text);
                await message.ShowAsync();
            }
            //textBlock.Text = rezultat.Text;
            
        }

        static string ParsePrice(string price)
        {
            price = Regex.Replace(price, @"\s+", " ");
            //price.Replace(@"\s+", " ");
            string[] words = price.Split(' ');
            int length = words.Length;
            if (length == 1)
            {
                if (price.Contains("."))
                {
                    string[] table = price.Split('.');
                    if (table.Length != 2)
                        throw new Exception("Bad format of data.");
                    if (!CheckNumbers(table[0], table[1]))
                        throw new Exception("Bad format of data.");
                    return price;
                }
                else if (price.Contains(","))
                {
                    string[] table = price.Split(',');
                    if (table.Length != 2)
                        throw new Exception("Bad format of data.");
                    if (!CheckNumbers(table[0], table[1]))
                        throw new Exception("Bad format of data.");
                    string priceToReturn = table[0] + "." + table[1];
                    return priceToReturn;
                }
                else
                {
                    if (price.Length < 1)
                        throw new Exception("Bad format of data.");
                    else if (price.Length == 1)
                    {
                        int grosze = int.Parse(price);
                        if (grosze < 1)
                            throw new Exception("Bad format of data.");
                        return "0.0" + grosze;
                    }
                    else if (price.Length == 2)
                    {
                        int grosze = int.Parse(price);
                        if (grosze < 0)
                            throw new Exception("Bad format of data.");
                        return "0." + price;
                    }
                    else
                    {
                        int grosze = int.Parse(words[0].Substring(words[0].Length - 2));
                        int zlotowki = int.Parse(words[0].Substring(0, words[0].Length - 2));
                        return zlotowki + "." + grosze;
                    }
                }
            }
            else if (words.Length == 2)
            {
                if (words[1].Contains("zł"))
                {
                    int zlotowki = int.Parse(words[0]);
                    if (zlotowki < 1)
                        throw new Exception("Bad format of data.");
                    return words[0];
                }
                else if (words[1].Contains("gr"))
                {
                    int grosze = int.Parse(words[0]);
                    if (grosze < 1 && grosze > 99)
                        throw new Exception("Bad format of data.");
                    if (grosze < 10)
                        return "0.0" + words[0];
                    else
                        return "0." + words[0];
                }
                else
                {
                    if (!CheckNumbers(words[0], words[1]))
                        throw new Exception("Bad format of data.");
                    return words[0] + "." + words[1];
                }
            }
            else if (words.Length == 4)
                if (words[1].Contains("zł") && words[3].Contains("gr"))
                {
                    if (!CheckNumbers(words[0], words[2]))
                        throw new Exception("Bad format of data.");
                    return words[0] + "." + words[2];
                }

            throw new Exception("Bad format of data.");
        }

        static bool CheckNumbers(string zlotowkiString, string groszeString)
        {
            int grosze = int.Parse(groszeString);
            if (grosze > 99 && grosze < 0)
                return false;
            int zlotowki = int.Parse(zlotowkiString);
            if (zlotowki < 0)
                return false;
            return true;
        }

        private async void buttonResearch_Click(object sender, RoutedEventArgs e)
        {
            //var dialog = new MessageDialog("Your message here");
            //await dialog.ShowAsync();
            this.Frame.Navigate(typeof(Search));
        }
    }
}
