using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Photos
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<SimpleImageInfo> fileList;
        private string _info;
        private BitmapImage _photo;
        private int _counter = 0;
        private bool _loaded = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            fileList = new ObservableCollection<SimpleImageInfo>();
            DataContext = this;
            ChangePhoto();     
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var obj = e.Parameter as SimpleImageInfo;
            if (obj != null)
            {
                _counter = obj.Id;
                ChangePhoto();
            }
            base.OnNavigatedTo(e);
        }

        public string Info
        {
            get
            {
                return _info;
            }
            set
            {
                _info = value;
                OnPropertyChanged("Info");
            }
        }

        public BitmapImage Photo
        {
            get
            {
                return _photo;
            }
            set
            {
                _photo = value;
                OnPropertyChanged("Photo");
            }
        }

        public async void ChangePhoto()
        {
            if (fileList.Count == 0)
                _loaded = await LoadImagesAsync();
            if (_counter >= fileList.Count)
                _counter = 0;
            SimpleImageInfo image = fileList[_counter];
            Photo = await GetImage(image.Source);
            Info = image.Info;
        }

        public async Task<bool> LoadImagesAsync()
        {
            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByDate, new[] { ".jpg", ".jpeg", ".jpe",
                ".jpg", ".gif", ".tiff", ".tif", ".png", ".bmp", ".wdp", ".jxr", ".hdp"});
            queryOptions.FolderDepth = FolderDepth.Deep;
            var query = KnownFolders.PicturesLibrary.CreateFileQueryWithOptions(queryOptions);
            IReadOnlyList<StorageFile> imageList = await query.GetFilesAsync();
            //foreach (var storageFile in imageList)
            //{
            //    ImageProperties props = await storageFile.Properties.GetImagePropertiesAsync();
            //    _info = storageFile.Name + "\n" + props.DateTaken + "\n" + props.Latitude + "; " + props.Longitude + "\n" + props.Height + "x" + props.Width;
            //    fileList.Add(new SimpleImageInfo
            //    {
            //        FileName = storageFile.Name,
            //        Info = _info,
            //        Source = storageFile,
            //        Thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.PicturesView),
            //        Id = fileList.Count
            //    });
            //}
            for (int i = 0; i <= 30; i++)
            {
                ImageProperties props = await imageList[i].Properties.GetImagePropertiesAsync();
                _info = imageList[i].Name + "\n" + props.DateTaken + "\n" + props.Latitude + "; " + props.Longitude + "\n" + props.Height + "x" + props.Width;
                fileList.Add(new SimpleImageInfo
                {
                    FileName = imageList[i].Name,
                    Info = _info,
                    Source = imageList[i],
                    Thumbnail = await imageList[i].GetThumbnailAsync(ThumbnailMode.PicturesView),
                    Id = fileList.Count
                });
            }
            progressRing.IsActive = false;
            return true;
        }

        public static async Task<BitmapImage> GetImage(StorageFile file)
        {
            BitmapImage bitmapImage;
            using (IRandomAccessStream fileStream =
                await file.OpenAsync(FileAccessMode.Read))
            {
                bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
            }
            return bitmapImage;
        }

        private async Task<bool> CapturePhoto()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if(allVideoDevices.Count < 1)
            {
                var dialog = new MessageDialog("No camera device found !");
                await dialog.ShowAsync();
                return false;
            }

            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.AllowCropping = false;
            captureUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo != null)
            {
                ImageProperties props = await photo.Properties.GetImagePropertiesAsync();
                string name = "Image " + DateTime.Now.ToString("yyyyMMdd Hmmss") + ".jpg";
                _info = name + "\n" + props.DateTaken + "\n" + props.Latitude + "; " + props.Longitude + "\n" + props.Height + "x" + props.Width;
                fileList.Add(new SimpleImageInfo
                {
                    FileName = name,
                    Info = _info,
                    Source = photo,
                    Thumbnail = await photo.GetThumbnailAsync(ThumbnailMode.PicturesView),
                    Id = fileList.Count
                });
                await photo.CopyAsync(KnownFolders.CameraRoll, name);
                return true;
            }
            else
                return false;
        }

        public event PropertyChangedEventHandler PropertyChanged = null;
        virtual protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _counter++;
            ChangePhoto();
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(_loaded)
            {
                try
                {
                    if (await CapturePhoto() == true)
                    {
                        _counter = fileList.Count - 1;
                        ChangePhoto();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    var dialog = new MessageDialog("The app was denied access to the camera");
                    await dialog.ShowAsync();
                }
            }       
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            if(_loaded)
            {
                this.Frame.Navigate(typeof(PhotoList), fileList);
            }            
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            if(_loaded && fileList.Count > 0)
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                    DataRequestedEventArgs>(this.ShareImageHandler);
                DataTransferManager.ShowShareUI();
            }
        }

        private void ShareImageHandler(DataTransferManager sender,
            DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Share Image Example";
            request.Data.Properties.Description = "Demonstrates how to share an image.";

            // Because we are making async calls in the DataRequested event handler,
            //  we need to get the deferral first.
            DataRequestDeferral deferral = request.GetDeferral();

            // Make sure we always call Complete on the deferral.
            try
            {
                request.Data.Properties.Thumbnail =
                    RandomAccessStreamReference.CreateFromFile(fileList[_counter].Source);
                StorageFile imageFile = fileList[_counter].Source;
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
            }
            finally
            {
                deferral.Complete();
            }
        }

    }
}

