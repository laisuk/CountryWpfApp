using CountryWpfApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        var jsonCurrency2 = jsonCurrency.Select(x => $"{x.Value}" );
        var jsonCurrency3 = "["+ string.Join(",", jsonCurrency2) + "]";
        var currencyData = JsonSerializer.Deserialize<List<MYR>>(jsonCurrency3);
        var currencyData2 = currencyData?.Select(x => $"{x.name} ({x.symbol})")
            .ToList();
        var currencies  = string.Join(",\n",currencyData2!);

        return currencies;
        //return jsonAllCurency;
    }

    public string getLanguages(AllCountries allCountries)
    {
        var languages = JsonSerializer.Serialize(allCountries.languages);
        var countryLanguage = Regex.Matches(languages , "(?<=:\\\")\\w*(?=\\\")").ToList();
        var languageList = string.Join(", " , countryLanguage);
        return languageList;
    }
}

