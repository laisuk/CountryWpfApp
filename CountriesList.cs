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
using System.Collections;

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

    // Get currency with Regex
    public static string GetCurrencies(AllCountries allCountries)
    {
        if (allCountries.currencies == null) { return "N/A"; }

        List<string> jsonMatchedList = new();

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonAllCurrency = JsonSerializer.Serialize(allCountries.currencies, jsonOptions);

        string rxExtractCurrencyPattern = "\\\"[\\w]{3}\\\":\\{\\\"name\\\":\\\"[\\w\\W]+?\\\"(,\\\"symbol\\\":\\\"[\\w\\W]+?\\\")?}";
        string rxAddCodeFindPattern = "(\\\"[\\w]{3}\\\"):\\{";
        string rxAddCodeReplacePattern = "{\"code\":$1,";

        var jsonCurrencyMatched = Regex.Matches(jsonAllCurrency, rxExtractCurrencyPattern).ToList();

        foreach (var _matched in jsonCurrencyMatched)
        {
            var _matchedString = Regex.Replace(_matched.ToString(), rxAddCodeFindPattern, rxAddCodeReplacePattern);
            jsonMatchedList.Add(_matchedString);
        }

        var jsonCurrencyText = $"[{string.Join(",", jsonMatchedList)}]";
        var currencyData = JsonSerializer.Deserialize<List<CurrencyRecord>>(jsonCurrencyText!);
        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({x.symbol ?? "N/A"})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
        //return jsonAllCurrency;
    }

    // Regex + Serialized IgnoreNull
    public static string GetCurrenciesRx(AllCountries allCountries)
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
            .Select(x => $"{x.code} {x.name} ({(x.symbol ?? "N/A")})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
        //return jsonAllCurrency;
    }

    public static string GetCurrenciesNodes(Currencies currency)
    {
        if (currency == null) { return "N/A"; }

        var jsonOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonAllCurrency = JsonSerializer.Serialize(currency, jsonOptions);

        if (jsonAllCurrency == null) { return "N/A"; }

        List<CurrencyRecord> currencyData = new();

        IEnumerable<KeyValuePair<string, JsonNode?>> currencyJsonObject = JsonNode.Parse(jsonAllCurrency)!.AsObject().AsEnumerable();
        foreach (KeyValuePair<string, JsonNode?> key in currencyJsonObject)
        {
            var _tmp = JsonNode.Parse(key.Value!.ToString());
            var _name = _tmp?["name"];
            var _symbol = _tmp?["symbol"] ?? "N/A";
            currencyData.Add(new CurrencyRecord() { code = key.Key, name = (string)_name!, symbol = (string)_symbol! });
        }

        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({x.symbol})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
        //return jsonAllCurrency;
    }

    // Hybrid Reflection + Serialized
    public static string GetCurrenciesMod(AllCountries allCountries)
    {
        if (allCountries.currencies == null) { return "N/A"; }

        List<string> currencyList = new();
        PropertyInfo[] propertyInfo = typeof(Currencies).GetProperties();
        foreach (PropertyInfo property in propertyInfo)
        {
            if (!property.CanRead) continue;
            var _currency = property.GetValue(allCountries.currencies, null);
            if (_currency != null)
            {
                string _jsonCurrency = JsonSerializer.Serialize(_currency, property.PropertyType);
                var _tmp = RegexReplacePropertyName().Replace(_jsonCurrency, $"$1\"code\":\"{property.Name}\",");
                currencyList.Add(_tmp);
            }
        }
        var jsonCurrencyText = $"[{string.Join(",", currencyList)}]";
        var currencyData = JsonSerializer.Deserialize<List<CurrencyRecord>>(jsonCurrencyText!);
        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({(x.symbol ?? "N/A")})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

        return currencies;
    }

    // Reflection
    public static string GetCurrenciesModified(AllCountries allCountries)
    {
        if (allCountries.currencies == null)
        {
            return "N/A";
        }

        List<CurrencyRecord> currencyData = new();
        List<string> _strings = new();

        foreach (PropertyInfo property in allCountries.currencies.GetType().GetProperties())
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

    // Reflection 2
    public static string GetCurrenciesReflection(AllCountries allCountries)
    {
        if (allCountries.currencies == null)
        {
            return "N/A";
        }

        List<CurrencyRecord> currencyData = new();

        foreach (PropertyInfo property in allCountries.currencies.GetType().GetProperties())
        {
            if (!property.CanRead) continue;
            var _currency = property.GetValue(allCountries.currencies, null);
            if (_currency != null)
            {
                var _name = _currency.GetType().GetProperty("name")!.GetValue(_currency, null)!.ToString();
                var _symbol = _currency.GetType().GetProperty("symbol") != null ? _currency.GetType().GetProperty("symbol")!.GetValue(_currency, null)!.ToString() : "N/A";
                currencyData.Add(new CurrencyRecord() { code = property.Name, name = _name, symbol = _symbol });
            }
        }

        var currencyDataSorted = currencyData?
            .Select(x => $"{x.code} {x.name} ({x.symbol})")
            .ToList();
        var currencies = string.Join(",\n", currencyDataSorted!);

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

    public static string GetLanguagesMod(Languages languages)
    {
        if (languages == null) { return "N/A"; }

        List<string> languagesMod = new();
        PropertyInfo[] properties = languages.GetType().GetProperties();

        foreach (PropertyInfo property in properties)
        {
            if (!property.CanRead) continue;
            var _language = property.GetValue(languages!, null);

            if (_language != null)
            {
                languagesMod.Add(_language.ToString()!);
            }
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

