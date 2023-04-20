using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

public class CountriesList
{
    public List<AllCountries> getAllCountryData(string filePath)
    {
        try
        {
            var jsonText = File.ReadAllText(filePath);
            var countryData = JsonSerializer.Deserialize<List<AllCountries>>(jsonText);
            IEnumerable<AllCountries> query = countryData!.OrderBy(AllCountries => AllCountries?.name?.common);

            return query.ToList();
            //return countryData!;

        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public List<string> getAllCountryList(List<AllCountries> allCountries)
    {
        var allCountryList = allCountries
            .Select(x => x.name?.common)
            .ToList();

        return allCountryList!;

    }

    public AllCountries getCurrentCountry(List<AllCountries> allCountries, int id)
    {
        return allCountries[id];
    }

    public string getCurrencies(AllCountries allCountries)
    {
        var jsonAllCurency = JsonSerializer.Serialize(allCountries.currencies);
        var jsonCurrency = Regex.Matches(jsonAllCurency, "\\{\\\"name\\\":\\\"([\\w\\W]*?)\\\",\\\"symbol\\\":\\\"(.*?)\\\"\\}");
        var jsonCurrency2 = jsonCurrency
            .Select(x => $"{x.Value}");
        var jsonCurrencyText = "[" + string.Join(",", jsonCurrency2) + "]";
        var currencyData = JsonSerializer.Deserialize<List<MYR>>(jsonCurrencyText!);
        var currencyDataSorted = currencyData?.Select(x => $"{x.name} ({x.symbol})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
        //return jsonAllCurency;
    }

    public string getLanguages(AllCountries allCountries)
    {
        var languages = JsonSerializer.Serialize(allCountries.languages);
        var countryLanguage = Regex.Matches(languages, "(?<=:\\\")\\w*(?=\\\")").ToList();
        var languageList = string.Join(", ", countryLanguage);
        return languageList;
    }

    public string getJsonFileDate(string filePath)
    {
        //return File.GetCreationTime(filePath).ToString();
        return File.GetLastWriteTime(filePath).ToString();
    }

    public async void updateJsonDataFile(string filePath)
    {
        string allCountryUrl = @"https://restcountries.com/v3.1/all";
        string backupFilePath = @".\Data\all.json.bak";

        if (File.Exists(filePath))
        {
            try
            {
                File.Copy(filePath, backupFilePath, true);
                //string sucessMassage = $"File backup done: {backupFilePath} @ " + getJsonFileDate(backupFilePath);
                //MessageBox.Show(sucessMassage, "File Backup");
            }
            catch (Exception ex)
            {
                string errorMassage = "File backup error: " + ex.Message;
                MessageBox.Show(errorMassage, getJsonFileDate(filePath));

                throw;
            }
        }

        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(allCountryUrl);
            var content = await response.Content.ReadAsStringAsync();        

            try
            {
                File.WriteAllText(filePath, content);
                string sucessMassage = $"File update SUCCESS: {filePath} @ " + getJsonFileDate(filePath);
                MessageBox.Show(sucessMassage, "JSON Data File Update");
            }
            catch (Exception ex)
            {
                string errorMassage = "File update error: " + ex.Message;
                MessageBox.Show(errorMassage, getJsonFileDate(filePath));

                throw;
            }
        }
    }
}

