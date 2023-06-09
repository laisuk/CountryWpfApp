﻿using System;
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
        public const string countryFilePath = @".\Data\all.json";
        public const string AllCountryUrl = @"https://restcountries.com/v3.1/all";
        public const string JsonBackupFilePath = @".\Data\all.json.bak";
        private static List<AllCountries> AllCountryData = CountriesList.GetAllCountryData(countryFilePath);
        public static bool updateComplete;        

        public MainWindow()
        {
            InitializeComponent();

            var countryNameList = CountriesList.GetAllCountryNameList(AllCountryData);
            cbCountryCode.ItemsSource = countryNameList;
            cbCountryCode.SelectedIndex = -1;
            lblFileDate.Content = CountriesList.GetJsonFileDate(countryFilePath);
        }

        private void cbCountryCode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //string message = $"{cbCountryCode.SelectedIndex}: [{cbCountryCode.SelectedItem}] selected.";
            //string title = "Country Selected";
            //MessageBox.Show(message, title);
            //var currentCountry = countryList.getCurrentCountry(allCountryData, cbCountryCode.SelectedIndex);
            //fillCountryForm(currentCountry);
            return;
        }

        private void btnGetInfo_Click(object sender, RoutedEventArgs e)
        {
            var currentCountry = CountriesList.GetCurrentCountry(AllCountryData, cbCountryCode.SelectedIndex);
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

                tbLanguages.Text = CountriesList.GetLanguages(countryModel!);
                //tbLanguages.Text = countryList.getLanguagesMod(countryModel!);

                tbArea.Text = countryModel?.area + " km²";
                tbLatLng.Text = $"{countryModel?.latlng?[0]}° : {countryModel?.latlng?[1]}°";
                tbPopulation.Text = countryModel?.population.ToString();

                tbBorder.Text = CountriesList.GetBorders(countryModel!);

                tbCar.Text = countryModel?.car?.side?.ToUpper();
                tbTimeZone.Text = countryModel?.timezones?[0];
                tbContinent.Text = CountriesList.GetContinents(countryModel!);
                tbDemonyms.Text = CountriesList.GetDemonyms(countryModel!);
                tbStartOfWeek.Text = countryModel?.startOfWeek?.ToUpper();

                //string currencies = countryList.getCurrencies(allCountryData[cbCountryCode.SelectedIndex]); // Regex
                string currencies = CountriesList.GetCurrenciesRx(AllCountryData[cbCountryCode.SelectedIndex]); // Regex + Serialized IgnoreNull
                //string currencies = countryList.getCurrenciesMod(allCountryData[cbCountryCode.SelectedIndex]); // Hybrid Reflection + Serialized
                //string currencies = countryList.getCurrenciesModified(allCountryData[cbCountryCode.SelectedIndex]); // Reflection
                tbCurrency.Text = currencies;

                Uri uri = new(countryModel?.flags?.png!);
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
            updateComplete = false;
            CountriesList.UpdateJsonDataFile(countryFilePath);
            var length = 100;

            Task.Run(() =>
            {
                for (int i = 0; i <= length; i++)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        btnUpdateData.IsEnabled = false;
                        lblFileDate.Content = $"Updating...({i})";
                    }), DispatcherPriority.Render);

                    if (updateComplete)
                    {
                        break;
                    }
                    Thread.Sleep(30);
                }
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lblFileDate.Content = CountriesList.GetJsonFileDate(countryFilePath);
                    btnUpdateData.IsEnabled = true;
                }), DispatcherPriority.Render);
                updateComplete = false;
            });

            AllCountryData = CountriesList.GetAllCountryData(countryFilePath);            
        }
    }
}
