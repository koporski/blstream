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


namespace Photos
{
    
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        private string _info;
        private BitmapImage _photo;
        private int _counter = 0;
        private bool _loaded = false;
        private ObservableCollection<SimpleImageInfo> _imageList;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            DataContext = this;
            _imageList = new ObservableCollection<SimpleImageInfo>();
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

        //funkcja do zmiany wyświetlanego zdjęcia
        public async void ChangePhoto()
        {
            if (_imageList.Count == 0)
                _loaded = await LoadImagesAsync();
            if (_counter >= _imageList.Count)
                _counter = 0;
            SimpleImageInfo image = _imageList[_counter];
            Photo = await GetImage(image.Source);
            Info = image.Info;
        }

        //ładowanie zdjęć z biblioteki do listy
        public async Task<bool> LoadImagesAsync()
        {
            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByDate, new[] { ".jpg", ".jpeg", ".jpe",
                ".jpg", ".gif", ".tiff", ".tif", ".png", ".bmp", ".wdp", ".jxr", ".hdp"});
            queryOptions.FolderDepth = FolderDepth.Deep;
            var query = KnownFolders.PicturesLibrary.CreateFileQueryWithOptions(queryOptions);
            IReadOnlyList<StorageFile> fileList = await query.GetFilesAsync();
            foreach (var storageFile in fileList)
            {
                ImageProperties props = await storageFile.Properties.GetImagePropertiesAsync();
                _info = storageFile.Name + "\n" + props.DateTaken + "\n" + props.Latitude + "; " + props.Longitude + "\n" + props.Height + "x" + props.Width;
                _imageList.Add(new SimpleImageInfo
                {
                    FileName = storageFile.Name,
                    Info = _info,
                    Source = storageFile,
                    Thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.PicturesView),
                    Id = _imageList.Count
                });
            }
            progressRing.IsActive = false;
            return true;
        }

        //funkcja zwracająca bitmapę
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

        //obsługa aparatu
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
                _imageList.Add(new SimpleImageInfo
                {
                    FileName = name,
                    Info = _info,
                    Source = photo,
                    Thumbnail = await photo.GetThumbnailAsync(ThumbnailMode.PicturesView),
                    Id = _imageList.Count
                });
                await photo.CopyAsync(KnownFolders.CameraRoll, name);
                return true;
            }
            else
                return false;
        }

        //obsługa funkcji "share"
        private void ShareImageHandler(DataTransferManager sender,
           DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Image sharing";
            request.Data.Properties.Description = "Sharing selected image...";
            DataRequestDeferral deferral = request.GetDeferral();
            try
            {
                request.Data.Properties.Thumbnail =
                    RandomAccessStreamReference.CreateFromFile(_imageList[_counter].Source);
                StorageFile imageFile = _imageList[_counter].Source;
                request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(imageFile));
            }
            finally
            {
                deferral.Complete();
            }
        }

        //naciśnięcie na obrazek
        private void image_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _counter++;
            ChangePhoto();
        }

        //przycisk aparatu
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(_loaded)
            {
                try
                {
                    if (await CapturePhoto() == true)
                    {
                        _counter = _imageList.Count - 1;
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

        //przycisk listy zdjęć
        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            if(_loaded)
            {
                this.Frame.Navigate(typeof(PhotoList), _imageList);
            }            
        }

        //przycisk share
        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            if(_loaded && _imageList.Count > 0)
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                    DataRequestedEventArgs>(this.ShareImageHandler);
                DataTransferManager.ShowShareUI();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged = null;
        virtual protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}

