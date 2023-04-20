﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;

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

            //IEnumerable<AllCountries> query = allCountryData.OrderBy(AllCountries => AllCountries?.name?.common);
            var countryNameList = countryList.getAllCountryList(allCountryData);
            cbCountryCode.ItemsSource = countryNameList;
            cbCountryCode.SelectedIndex = -1;
        }

        private void cbCountryCode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string message = $"{cbCountryCode.SelectedIndex}: [{cbCountryCode.SelectedItem}] selected.";
            //string title = "Country Selected";
            //MessageBox.Show(message, title);
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

                tbLanguages.Text = countryList.getLanguages(countryModel!);

                tbArea.Text = countryModel?.area.ToString();
                tbLatLng.Text = $"{countryModel?.latlng?[0]} : {countryModel?.latlng?[1]}";
                tbPopulation.Text = countryModel?.population.ToString();

                if (countryModel?.borders != null)
                {
                    tbBorder.Text = String.Join(", ", countryModel?.borders?.Select(b => b)!);
                }
                else
                {
                    tbBorder.Text = "None";
                }
                
                tbCar.Text = countryModel?.car?.side;
                tbTimeZone.Text = countryModel?.timezones?[0];
                tbStartOfWeek.Text = countryModel?.startOfWeek;

                string currencies = countryList.getCurrencies(allCountryData[cbCountryCode.SelectedIndex]);
                tbCurrency.Text = currencies;
                
                Uri uri = new Uri(countryModel?.flags?.png!);               
                imgFlag.Source = new BitmapImage(uri);


            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();    
        }
    }

    
}