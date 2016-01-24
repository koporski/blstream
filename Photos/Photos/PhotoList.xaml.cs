using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Photos
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoList : Page
    {
        public ObservableCollection<SimpleImageInfo> _imageList; 
        public PhotoList()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _imageList = (ObservableCollection<SimpleImageInfo>)e.Parameter;
            FileListItemsControl.ItemsSource = _imageList;
            base.OnNavigatedTo(e);
        }

        //przycisk powrotu
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        //wybór obrazka
        private void FileListItemsControl_ItemClick(object sender, ItemClickEventArgs e)
        {
            SimpleImageInfo selectedItem = (SimpleImageInfo)e.ClickedItem;
            this.Frame.Navigate(typeof(MainPage), selectedItem);

        }
    }
}
