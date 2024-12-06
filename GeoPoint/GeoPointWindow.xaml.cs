using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace GeoPoint
{
    /// <summary>
    /// Логика взаимодействия для GeoPointWindow.xaml
    /// </summary>
    public partial class GeoPointWindow : Window
    {
        public List<ListViewItem> Items = new();
        private readonly GeoService _service = new();
        public GeoPointWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillListView();
        }

        private async void FillListView()
        {
            var kmls = await _service.GetKmlsAsync();
            foreach (var kml in kmls)
            {
                ListViewItem item = new() { Date = kml.DateTime, KmlPath = kml.KmlData, ImagePath = kml.ImagePath };
                Items.Add(item);
            }

            KmlsListView.ItemsSource = Items;
        }

        private void GE3DButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as ListViewItem;

            // Здесь вы можете выполнить необходимое действие с item
            try
            {
                // Укажите приложение для открытия KML файла, например, Google Earth
                string applicationPath = @"C:\Program Files\Google\Google Earth Pro\client\googleearth.exe"; // Путь к Google Earth

                Process.Start(applicationPath, item.KmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
