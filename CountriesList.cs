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
using CountryWpfApp;
using CountryWpfApp.Models;
using System.Text.Json.Nodes;

public partial class CountriesList
{
    public static List<AllCountries> GetAllCountryData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, MyModel.my);
        }

        try
        {
            var jsonText = File.ReadAllText(filePath);
            var countryData = JsonSerializer.Deserialize<List<AllCountries>>(jsonText);
            IEnumerable<AllCountries> query = countryData!.OrderBy(x => x?.name?.common);

            return query.ToList();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static List<string> GetAllCountryNameList(List<AllCountries> allCountries)
    {
        var allCountryList = allCountries
            .Select(x => x.name?.common)
            .ToList();

        return allCountryList!;
    }

    public static AllCountries GetCurrentCountry(List<AllCountries> allCountries, int id)
    {
        return allCountries[id];
    }

      // Regex + Serialized IgnoreNull
    public static string GetCurrencies(Currencies currency)
    {
        if (currency == null) { return "N/A"; }

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonCurrencies = JsonSerializer.Serialize(currency, jsonOptions);

        string rxAddCodeFindPattern = "(\\\"[\\w]{3}\\\"):\\{";
        string rxAddCodeReplacePattern = "{\"code\":$1,";

        var _temp = Regex.Replace(jsonCurrencies, rxAddCodeFindPattern, rxAddCodeReplacePattern);
        var jsonCurrenciesText = $"[{_temp[1..^1]}]";

        var currencyData = JsonSerializer.Deserialize<List<CurrencyRecord>>(jsonCurrenciesText!);
        var currencyDataList = currencyData?
            .Select(x => $"{x.code} {x.name} ({(x.symbol ?? "N/A")})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataList!);

        return currencies;
    }

    //Using .Net new JsonNode method
    public static string GetCurrenciesNodes(Currencies currency)
    {
        if (currency == null) { return "N/A"; }

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonCurrencies = JsonSerializer.Serialize(currency, jsonOptions);

        if (jsonCurrencies == null) { return "N/A"; }

        List<CurrencyRecord> currencyData = new();

        IEnumerable<KeyValuePair<string, JsonNode?>> currencyJsonObject = JsonNode.Parse(jsonCurrencies)!.AsObject().AsEnumerable();
        foreach (KeyValuePair<string, JsonNode?> key in currencyJsonObject)
        {
            dynamic _tmp = JsonNode.Parse(key.Value!.ToJsonString())!;
            dynamic _name = _tmp?["name"]!.ToString() ?? "N/A";
            dynamic _symbol = _tmp?["symbol"]!.ToString() ?? "N/A";
            currencyData.Add(new CurrencyRecord() { code = key.Key, name = _name, symbol = _symbol });
        }

        dynamic _currencyDataList = currencyData?
            .Select(x => $"{x.code} {x.name} ({x.symbol})")
            .ToList()!;
        var _currencies = string.Join(",\n", _currencyDataList);

        return _currencies;
    }

    // Hybrid Reflection + Serialized (slow)
    public static string GetCurrenciesMod(Currencies currency)
    {
        if (currency == null) { return "N/A"; }

        List<string> currencyList = new();
        PropertyInfo[] properties = typeof(Currencies).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (!property.CanRead) continue;
            var _currency = property.GetValue(currency, null);
            if (_currency != null)
            {
                string _jsonCurrency = JsonSerializer.Serialize(_currency, property.PropertyType);
                var _tmp = RegexReplacePropertyName().Replace(_jsonCurrency, $"$1\"code\":\"{property.Name}\",");
                currencyList.Add(_tmp);
            }
        }
        var jsonCurrencyText = $"[{string.Join(",", currencyList)}]";
        var currencyData = JsonSerializer.Deserialize<List<CurrencyRecord>>(jsonCurrencyText!);
        var currencyDataList = currencyData?
            .Select(x => $"{x.code} {x.name} ({(x.symbol ?? "N/A")})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataList!);

        return currencies;
    }

    // Reflection 1 (two loops, very slow)
    public static string GetCurrenciesModified(Currencies currency)
    {
        if (currency == null)
        {
            return "N/A";
        }

        List<CurrencyRecord> currencyData = new();
        List<string> _strings = new();

        foreach (PropertyInfo property in currency.GetType().GetProperties())
        {
            if (!property.CanRead) continue;
            var _currency = property.GetValue(currency, null);
            if (_currency != null)
            {
                foreach (PropertyInfo property2 in _currency.GetType().GetProperties())
                {
                    if (!property2.CanRead) continue;
                    _strings.Add(property2.GetValue(_currency, null)?.ToString()!);
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

        var currencyDataList = currencyData?
            .Select(x => $"{x.code} {x.name} ({x.symbol})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataList!);

        return currencies;
    }

    // Reflection 2 (1 loop, slow)
    public static string GetCurrenciesReflection(Currencies currency)
    {
        if (currency == null)
        {
            return "N/A";
        }

        List<CurrencyRecord> _currencyData = new();

        foreach (PropertyInfo property in currency.GetType().GetProperties())
        {
            if (!property.CanRead) continue;
            var _currency = property.GetValue(currency, null);
            if (_currency != null)
            {
                var _name = _currency.GetType().GetProperty("name")!.GetValue(_currency, null)!.ToString();
                var _symbol = _currency.GetType().GetProperty("symbol") != null ? _currency.GetType().GetProperty("symbol")!.GetValue(_currency, null)!.ToString() : "N/A";
                _currencyData.Add(new CurrencyRecord() { code = property.Name, name = _name, symbol = _symbol });
            }
        }

        var _currencyDataSorted = _currencyData?
            .Select(x => $"{x.code} {x.name} ({x.symbol})")
            .ToList();
        var currencies = string.Join(",\n", _currencyDataSorted!);

        return currencies;
    }

    public static string GetLanguages(Languages languages)
    {
        if (languages == null) { return "N/A"; }

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var languagesText = JsonSerializer.Serialize(languages, jsonOptions);
        var countryLanguage = RegexGetLanguage().Matches(languagesText)
            .ToList();
        var languageList = string.Join(", ", countryLanguage);

        return languageList;
    }

    public static string GetLanguagesNodes(Languages languages)
    {
        if (languages == null) { return "N/A"; }

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var languagesText = JsonSerializer.Serialize(languages, jsonOptions);
        var languagesJsonObject = JsonNode.Parse(languagesText)!.AsObject().AsEnumerable();

        List<string> languagesMod = new();

        foreach (var key in languagesJsonObject)
        {
            languagesMod.Add(key.Value!.ToString());
        }

        var languageList = string.Join(", ", languagesMod);
        return languageList;
    }

    public static string GetBorders(AllCountries allCountries)
    {
        if (allCountries?.borders != null)
        {
            return string.Join(", ", allCountries?.borders!);
        }
        else
        { return "None"; }
    }

    public static string GetContinents(AllCountries allCountries)
    {
        if (allCountries.continents != null)
        {
            return string.Join(", ", allCountries?.continents!);
        }
        else
        { return "None"; }
    }

    public static string GetDemonyms(AllCountries allCountries)
    {
        var demonyms = $"{allCountries?.demonyms?.eng?.m} (M)\n{allCountries?.demonyms?.eng?.f} (F)";
        return demonyms;
    }

    public static string GetJsonFileDate(string filePath)
    {
        //return File.GetCreationTime(filePath).ToString();
        return File.GetLastWriteTime(filePath).ToString();
    }

    public static async void UpdateJsonDataFile(string filePath)
    {
        string allCountryUrl = MainWindow.AllCountryUrl;
        string backupFilePath = MainWindow.JsonBackupFilePath;

        if (File.Exists(filePath))
        {
            try
            {
                File.Copy(filePath, backupFilePath, true);
                //string successMassage = $"File backup done: {backupFilePath} @ " + getJsonFileDate(backupFilePath);
                //MessageBox.Show(successMassage, "File Backup");
            }
            catch (HttpRequestException e)
            {
                string errorMassage = "File backup error: " + e.Message;
                MessageBox.Show(errorMassage, GetJsonFileDate(filePath));

                //throw;
            }
        }

        using var client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(allCountryUrl);
        var content = await response.Content.ReadAsStringAsync();

        try
        {
            File.WriteAllText(filePath, content);
            MainWindow.updateComplete = true;
            string successMassage = $"File update SUCCESS: {filePath} @ " + GetJsonFileDate(filePath);
            MessageBox.Show(successMassage, "JSON Data File Update");
        }
        catch (Exception e)
        {
            string errorMassage = "File update error: " + e.Message;
            MessageBox.Show(errorMassage, GetJsonFileDate(filePath));

            //throw;
        }
    }

    [GeneratedRegex("^(\\{)")]
    private static partial Regex RegexReplacePropertyName();
    [GeneratedRegex("(?<=:\\\")\\w*(?=\\\")")]
    private static partial Regex RegexGetLanguage();
}

public record CurrencyRecord
{
    public string? code { get; set; }
    public string? name { get; set; }
    public string? symbol { get; set; }
}


