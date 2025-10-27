using Panik;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Panik.Translation;

namespace CloverAPI.Content.Strings;

public static class LanguageManager
{
    internal static List<string> CustomLanguages = new();
    internal static Dictionary<Language, string> LangMap = new();
    internal static Dictionary<string, Language> LangMapT = new();
    internal static Dictionary<string, Dictionary<string, string>> TermsByLanguage = new();
    private static int NextId => Enum.GetValues(typeof(Language)).Length + CustomLanguages.Count - 1;
    public static bool IsCustomLanguage => (int) Panik.Data.settings.language >= Enum.GetValues(typeof(Language)).Length - 1;

    public static Language LoadFromFile(string filePath)
    {
        var lines = File.ReadAllText(filePath).Replace("\\;", "<SEMICOLON>")
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var terms = new Dictionary<string, string>();
        var curTerm = "";
        foreach (var line in lines)
        {
            if (line.StartsWith("#") || line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            var parts = line.Split([';'], 2);
            if (parts.Length == 2)
            {
                curTerm = parts[0].Trim();
                terms[curTerm] = parts[1].Replace("<SEMICOLON>", ";").Trim();
            }
            else
            {
                terms[curTerm] += "\n" + line.Replace("<SEMICOLON>", ";");
            }
        }
        if (!terms.ContainsKey("YOUR_LANGUAGE_TRANSLATED"))
        {
            terms["YOUR_LANGUAGE_TRANSLATED"] = Path.GetFileNameWithoutExtension(filePath);
        }
        return RegisterLanguage(terms);
    }
    
    public static Language RegisterLanguage(params (string termKey, string termValue)[] terms)
    {
        var termDict = new Dictionary<string, string>();
        foreach (var (termKey, termValue) in terms)
        {
            termDict[termKey] = termValue;
        }

        var languageName = $"UnnamedLanguage{CustomLanguages.Count + 1}";
        if (termDict.TryGetValue("YOUR_LANGUAGE_TRANSLATED", out var translatedName))
        {
            languageName = translatedName;
        }
        return RegisterLanguage(languageName, termDict);
    }

    public static Language RegisterLanguage(Dictionary<string, string> terms)
    {
        var languageName = $"UnnamedLanguage{CustomLanguages.Count + 1}";
        if (terms.TryGetValue("YOUR_LANGUAGE_TRANSLATED", out var translatedName))
        {
            languageName = translatedName;
        }
        return RegisterLanguage(languageName, terms);
    }
    
    public static Language RegisterLanguage(string languageName, params (string termKey, string termValue)[] terms)
    {
        var termDict = new Dictionary<string, string>();
        foreach (var (termKey, termValue) in terms)
        {
            termDict[termKey] = termValue;
        }
        return RegisterLanguage(languageName, termDict);
    }

    public static Language RegisterLanguage(string languageName, Dictionary<string, string> terms)
    {
        if (!CustomLanguages.Contains(languageName))
        {
            Language langId = (Language)NextId;
            LangMap[langId] = languageName;
            LangMapT[languageName] = langId;
            TermsByLanguage[languageName] = new Dictionary<string, string>(terms);
            CustomLanguages.Add(languageName);
            _ = LanguagesInOrder; // Make sure the array is initialized
            _languagesInOrder = _languagesInOrder.Append(langId).ToArray();
            languageNamesTranslated = languageNamesTranslated.Append(languageName).ToArray();
            languagesI2Names = languagesI2Names.Append("English").ToArray();
            return langId;
        }
        throw new Exception("Language " + languageName + " is already registered.");
    }
    
    public static bool TryGetTerm(string termKey, out string termValue)
    {
        var language = Panik.Data.settings.language;
        if (LangMap.TryGetValue(language, out string languageName))
        {
            if (TermsByLanguage.TryGetValue(languageName, out var terms))
            {
                return terms.TryGetValue(termKey, out termValue);
            }
        }
        termValue = null;
        return false;
    }

    public static Language GetLanguageByName(string languageName)
    {
        if (LangMapT.TryGetValue(languageName, out var lang))
        {
            return lang;
        }
        throw new Exception("Language " + languageName + " is not registered.");
    }
    
    private static bool TryGetLanguageByName(string languageName, out Language lang)
    {
        return LangMapT.TryGetValue(languageName, out lang);
    }
}