using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CountryWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string countryFilePath = @".\Data\all.json";
        public static CountriesList countryList = new();
        List<AllCountries> allCountryData = countryList.getAllCountryData(countryFilePath);

        public MainWindow()
        {
            InitializeComponent();

            var countryNameList = countryList.getAllCountryList(allCountryData);
            cbCountryCode.ItemsSource = countryNameList;
            cbCountryCode.SelectedIndex = -1;
            lblFileDate.Content = countryList.getJsonFileDate(countryFilePath);
        }

        private void cbCountryCode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //string message = $"{cbCountryCode.SelectedIndex}: [{cbCountryCode.SelectedItem}] selected.";
            //string title = "Country Selected";
            //MessageBox.Show(message, title);
            return;
        }

        private void btnGetInfo_Click(object sender, RoutedEventArgs e)
        {
            var currentCountry = countryList.getCurrentCountry(allCountryData, cbCountryCode.SelectedIndex);
            fillCountryForm(currentCountry);
        }

        private void fillCountryForm(AllCountries countryModel)
        {
            if (countryModel != null)
            {
                tbName.Text = countryModel?.name?.common;
                tbCapital.Text = countryModel?.capital?[0];
                tbCode.Text = countryModel?.cca2 + " " + countryModel?.flag;
                tbIDD.Text = countryModel?.idd?.root + countryModel?.idd?.suffixes?[0];
                tbRegion.Text = countryModel?.region;
                tbSubRegion.Text = countryModel?.subregion;

                tbLanguages.Text = countryList.getLanguagesMod(countryModel!);

                tbArea.Text = countryModel?.area + " km²";
                tbLatLng.Text = $"{countryModel?.latlng?[0]}° : {countryModel?.latlng?[1]}°";
                tbPopulation.Text = countryModel?.population.ToString();

                tbBorder.Text = countryList.getBorders(countryModel!);

                tbCar.Text = countryModel?.car?.side?.ToUpper();
                tbTimeZone.Text = countryModel?.timezones?[0];
                tbStartOfWeek.Text = countryModel?.startOfWeek?.ToUpper();

                string currencies = countryList.getCurrenciesModified(allCountryData[cbCountryCode.SelectedIndex]);
                tbCurrency.Text = currencies;

                Uri uri = new Uri(countryModel?.flags?.png!);
                imgFlag.Source = new BitmapImage(uri);
                imgFlag.ToolTip = countryModel?.flags?.alt!;

            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnUpdateData_Click(object sender, RoutedEventArgs e)
        {
            countryList.updateJsonDataFile(countryFilePath);

            var length = 100;

            Task.Run(() =>
            {
                for (int i = 0; i <= length; i++)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lblFileDate.Content = $"Updating...({i})";
                    }), DispatcherPriority.Render);
                    Thread.Sleep(20);
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lblFileDate.Content = countryList.getJsonFileDate(countryFilePath);
                }), DispatcherPriority.Render);
            });

            allCountryData = countryList.getAllCountryData(countryFilePath);

        }
    }
}
