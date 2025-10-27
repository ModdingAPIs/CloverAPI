# 📖 Languages

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for loading custom languages into CloverPit with the Non-Code API.
</link-summary>

## Overview

Adding a new language to CloverPit through the API is simple. You just need to put a `.loc` file anywhere in the `plugins` folder (or a subfolder) and CloverPit will automatically load it.
The `.loc` file should be formatted as follows:

```
# This is a comment
ABILITY_DESCR_CHANCES_BELL;Increases yer chance o' findin' [S_BELL] bells (+1)
ABILITY_DESCR_CHANCES_CHERRY;Increases yer chance o' findin' [S_CHERRY] cherries (+1)
ABILITY_DESCR_CHANCES_CLOVER;Increases yer chance o' findin' [S_CLOVER] clovers (+1)
YOUR_LANGUAGE_TRANSLATED;Pirate Speak
```

Each line consists of a key and its corresponding translation, separated by a semicolon (`;`). Lines starting with `#` or `//` are treated as comments and ignored.  
For strings with multiple lines, just use regular line breaks. Any lines without key will be added to the previous key's value.  
For example:
```
POWERUP_DESCR_7_SINS_STONE;If ye find 7 "Sevens" [S_SEVEN] in a single spin, they all be turnin' into Golden Sevens [SMOD_GOLD].

[SMOD_GOLD_EXPLANATION]
```

If you need a semicolon in the translation, you can escape it with a backslash (`\;`) or use `<SEMICOLON>`.