# 🗒️ Strings & Translations

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for adding custom strings and translations to CloverPit using CloverAPI.
</link-summary>

**Code Reference**  
`namespace: CloverAPI.Content.Strings`  
`classes: StringManager | LocalizationManager | LanguageManager`

## Overview

Sometimes, you may want dynamic text in your mod, or you may want to support multiple languages. CloverAPI provides an
interface for this.

## Adding Custom Strings

Strings are dynamic text that essentially works as a `Replace()` function on all of its occurrences in the game.
You can add global strings using `StringManager.Register(string key, StringSource value, bool late = false)`.  
The `key` parameter is the text to be replaced.  
The `value` parameter is the text to replace it with. It can be a simple string, a function that returns a string (
`StringFromCallable`), or a localized string (`LocalizedString`).  
The `late` parameter determines whether the replacement should be applied before or after the vanilla game's own string
replacements. Usually early is better, as it'll allow common symbols like `€` for coins and `[ROUNDS_LEFT_N]` to apply
to the custom string as well.

```C#
using CloverAPI;
using System;
public static void RegisterStrings()
{
    StringManager.RegisterString("[EXAMPLE_STRING]", "Hello, World!");
    StringManager.RegisterString("[TODAY]", new StringFromCallable(() => DateTime.Now.ToString("D")));
}
```

## Localized Strings

If you want to support multiple languages, you can use `LocalizedString` to provide translations for different
languages.  
Only languages supported by the game can be used.  
To register a localized string, use `LocalizationManager.Register(string key, StringSource localizedString)`.  
`StringSource` can support mostly the same as `StringSource`, including all previously mentioned types (string,
`StringFromCallable`, and `LocalizedString`). You'll usually want to either use a simple string or a `LocalizedString`
though.

```C#
using CloverAPI;
using static Panik.Translation;

public static void RegisterLocalizedStrings()
{
    LocalizationManager.RegisterTranslation("[EXAMPLE_TRANSLATED]", new LocalizedString(new ()
    {
        [Language.English] = "Hello, World!",
        [Language.Italian] = "Ciao, Mondo!",
        [Language.French] = "Bonjour, le monde!",
        ...
    }));
}
```

## Custom Languages
The game supports multiple languages, but what if you want to add your own? You can do that using the `LanguageManager`.  
To register a custom language, use `LanguageManager.RegisterLanguage(Dictionary<string, string> terms)`.
The `terms` parameter is a dictionary that maps term keys to their translations in the custom language.
```C#
using CloverAPI;
using System.Collections.Generic;
public static void RegisterCustomLanguage()
{
    var pirateTerms = new Dictionary<string, string>
    {
        { "ABILITY_DESCR_CHANCES_BELL", "Increases yer chance o' findin' [S_BELL] bells (+1)" },
        { "ABILITY_DESCR_CHANCES_CHERRY", "Increases yer chance o' findin' [S_CHERRY] cherries (+1)" },
        { "ABILITY_DESCR_CHANCES_CLOVER", "Increases yer chance o' findin' [S_CLOVER] clovers (+1)" },
        { "YOUR_LANGUAGE_TRANSLATED", "Pirate Speak" },
        // Add more terms as needed
    };
    
    LanguageManager.RegisterLanguage(pirateTerms);
}
```
The 'YOUR_LANGUAGE_TRANSLATED' key is used to display the name of the language in the settings menu. You must define it for your custom language, otherwise it'll show up as "UnnamedLanguage#".  
Any missing keys will fall back to English.
You can also load languages from files. This is done through `LanguageManager.LoadFromFile(string filePath)`. This automatically
registers the language as well. The format for these files is explained in the non-coding documentation.
See the [Languages](NonCodeLanguages.md) documentation for more information.