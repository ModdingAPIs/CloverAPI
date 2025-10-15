# 🗒️ Strings & Translations

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for adding custom strings and translations to CloverPit using CloverAPI.
</link-summary>

**Code Reference**  
`namespace: CloverAPI.Content.Strings`  
`classes: StringManager | LocalizationManager`

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
    StringManager.Register("[EXAMPLE_STRING]", "Hello, World!");
    StringManager.Register("[TODAY]", new StringFromCallable(() => DateTime.Now.ToString("D")));
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
    LocalizationManager.Register("[EXAMPLE_TRANSLATED]", new LocalizedString(new ()
    {
        [Language.English] = "Hello, World!",
        [Language.Italian] = "Ciao, Mondo!",
        [Language.French] = "Bonjour, le monde!",
        ...
    }));
}
```