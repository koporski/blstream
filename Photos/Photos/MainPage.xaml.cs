using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Photos
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            getPicture();       
        }

        private async void getPicture()
        {
            // grtting pictures library and file list
            StorageFolder pictures = KnownFolders.PicturesLibrary;
            IReadOnlyList<StorageFile> fileList = await pictures.GetFilesAsync(CommonFileQuery.OrderByDate);
            if (fileList.Count > 0)
            {
                StorageFile file = fileList[0];
                using (IRandomAccessStream fileStream =
                    await file.OpenAsync(FileAccessMode.Read))
                {
                    
                    // setting the image source to the selected bitmap
                    Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
                    new Windows.UI.Xaml.Media.Imaging.BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    image.Source = bitmapImage;
                }
            }
           
         }
     }
}

