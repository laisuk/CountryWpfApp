using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Text.Json.Serialization;

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
        if (allCountries.currencies == null) { return "N/A"; }

        List<string> jsonMatchedList = new List<string>();

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonAllCurrency = JsonSerializer.Serialize(allCountries.currencies, jsonOptions);

        string rxExtractCurrencyPattern = "\\\"[\\w]{3}\\\":\\{\\\"name\\\":\\\"[\\w\\W]+?\\\"(,\\\"symbol\\\":\\\"[\\w\\W]+?\\\")?}";
        string rxAddCodeFindPattern = "(\\\"[\\w]{3}\\\"):\\{";
        string rxAddCodeReplacePattern = "{\"code\":$1,";

        var jsonCurrencyMatched = Regex.Matches(jsonAllCurrency, rxExtractCurrencyPattern).ToList();

        foreach (var _matche in jsonCurrencyMatched)
        {
            var _matcheString = Regex.Replace(_matche.ToString(), rxAddCodeFindPattern, rxAddCodeReplacePattern);
            jsonMatchedList.Add(_matcheString);
        }

        var jsonCurrencyText = $"[{string.Join(",", jsonMatchedList)}]";
        var currencyData = JsonSerializer.Deserialize<List<CurrencyRecord>>(jsonCurrencyText!);
        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({(x.symbol == null ? "N/A" : x.symbol)})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
        //return jsonAllCurrency;
    }

    public string getCurrenciesRx(AllCountries allCountries)
    {
        if (allCountries.currencies == null) { return "N/A"; }

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonAllCurrency = JsonSerializer.Serialize(allCountries.currencies, jsonOptions);

        string rxAddCodeFindPattern = "(\\\"[\\w]{3}\\\"):\\{";
        string rxAddCodeReplacePattern = "{\"code\":$1,";

        var _jsonAllCurrencyText = Regex.Replace(jsonAllCurrency, rxAddCodeFindPattern, rxAddCodeReplacePattern);
        var jsonAllCurrencyText = $"[{_jsonAllCurrencyText[1..^1]}]";

        var currencyData = JsonSerializer.Deserialize<List<CurrencyRecord>>(jsonAllCurrencyText!);
        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({(x.symbol == null ? "N/A" : x.symbol)})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
        //return jsonAllCurrency;
    }

    public string getCurrenciesMod(AllCountries allCountries)
    {
        if (allCountries.currencies == null) { return "N/A"; }

        List<string> currencyList = new List<string>();
        PropertyInfo[] propertyInfo = typeof(Currencies).GetProperties();
        foreach (PropertyInfo property in propertyInfo)
        {
            if (!property.CanRead) continue;
            var _currency = property.GetValue(allCountries.currencies, null);
            if (_currency != null)
            {
                string _jsonCurrency = JsonSerializer.Serialize(_currency, property.PropertyType);
                var _tmp = Regex.Replace(_jsonCurrency, @"^(\{)", $"$1\"code\":\"{property.Name}\",");
                currencyList.Add(_tmp);
            }
        }
        var jsonCurrencyText = $"[{string.Join(",", currencyList)}]";
        var currencyData = JsonSerializer.Deserialize<List<CurrencyRecord>>(jsonCurrencyText!);
        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({(x.symbol == null ? "N/A" : x.symbol)})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
    }

    public string getCurrenciesModified(AllCountries allCountries)
    {
        if (allCountries.currencies == null)
        {
            return "N/A";
        }

        List<CurrencyRecord> currencyData = new List<CurrencyRecord>();
        List<string> _strings = new List<string>();

        PropertyInfo[] propertyInfo = typeof(Currencies).GetProperties();

        foreach (PropertyInfo property in propertyInfo)
        {
            if (!property.CanRead) continue;
            var _currency = property.GetValue(allCountries.currencies, null);
            if (_currency != null)
            {
                foreach (PropertyInfo property1 in _currency.GetType().GetProperties())
                {
                    if (!property1.CanRead) continue;
                    _strings.Add(property1.GetValue(_currency, null)?.ToString()!);
                }
                if (_strings.Count == 2)
                {
                    currencyData.Add(new CurrencyRecord() { code = property.Name, name = _strings[0], symbol = _strings[1] });
                }
                else
                {
                    currencyData.Add(new CurrencyRecord() { code = property.Name, name = _strings[0], symbol = "N/A" });
                }
                _strings.Clear();
            }
        }

        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({x.symbol})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
    }

    public string getLanguages(AllCountries allCountries)
    {
        if (allCountries.languages == null) { return "N/A"; }

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var languages = JsonSerializer.Serialize(allCountries.languages, jsonOptions);
        var countryLanguage = Regex.Matches(languages, "(?<=:\\\")\\w*(?=\\\")")
            .ToList();
        var languageList = string.Join(", ", countryLanguage);

        return languageList;
    }

    public string getLanguagesMod(AllCountries allCountries)
    {
        if (allCountries.languages == null) { return "N/A"; }

        List<string> languagesMod = new List<string>();
        PropertyInfo[] properties = typeof(Languages).GetProperties();

        if (properties.Length == 0) { return "N/A"; }

        foreach (PropertyInfo property in properties)
        {
            if (!property.CanRead) continue;
            var _languege = property.GetValue(allCountries?.languages!, null);

            if (_languege != null)
            {
                languagesMod.Add(_languege.ToString()!);
            }
        }

        var languageList = string.Join(", ", languagesMod);

        return languageList;
    }

    public string getBorders(AllCountries allCountries)
    {
        if (allCountries?.borders != null)
        {
            return String.Join(", ", allCountries?.borders!);
        }
        else
        { return "None"; }
    }

    public string getContinents(AllCountries allCountries)
    {
        if (allCountries.continents != null)
        {
            return String.Join(", ", allCountries?.continents!);
        }
        else
        { return "None"; }
    }

    public string getDemonyms(AllCountries allCountries)
    {
        var demonyms = $"{allCountries?.demonyms?.eng?.m} (M)\n{allCountries?.demonyms?.eng?.f} (F)";
        return demonyms;
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

public record CurrencyRecord
{
    public string? code { get; set; }
    public string? name { get; set; }
    public string? symbol { get; set; }
}

