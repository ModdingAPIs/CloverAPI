# 🏠 Introduction

<show-structure for="chapter" depth="2"/>

<link-summary>
Main page for Clover API documentation
</link-summary>

## Word of Caution

CloverAPI is still in early development, and while it is functional, there may be breaking changes in future updates.  
The documentation may also be incomplete or inaccurate in some areas.

## Getting Started
CloverAPI is a modding framework for CloverPit that allows you to create and manage custom content for the game.  
You'll need some familiarity with C# and preferably some experience with Unity and BepInEx to be able to use it effectively. See the [README of the GitHub Repository](https://github.com/ModdingAPIs/CloverAPI?tab=readme-ov-file#cloverpit-api) for more information on BepInEx and Harmony. The [Panik Games Discord](https://discord.gg/ytgv) server also has a modding channel where you can ask for help.
The best way to get started is to get the [Example Mod](https://github.com/IngoHHacks/CloverPitExampleMod), which demonstrates some of the features of CloverAPI. It also serves as a basic mod structure that you can use as a starting point for your own mods.

## Features

### Code-Based API
- [Charms](Charms.md)
- [Strings & Translations](Strings.md)
- [Persistent Data Storage](SaveData.md)
- [Settings & Configuration](Settings.md)
- [Audio Overrides](AudioOverrides.md)
- [Texture Overrides](TextureOverrides.md)

### Non-Code-Based API
- [Languages](NonCodeLanguages.md)

## Feature Stability
| Feature                  | Stability       | Notes                                                                                         |
|--------------------------|-----------------|-----------------------------------------------------------------------------------------------|
| Charms                   | 🟩 Good         | Core mechanics are stable, but testing is ongoing. New features also are being added.         |
| Strings & Translations   | 🟨 Decent       |                                                                                               |
| Languages                | 🟨 Decent       |                                                                                               |
| Persistent Data Storage  | 🟨 Decent       |                                                                                               |
| Settings & Configuration | 🟧 Moderate     | Recently added feature adapted from pharmacomaniac's ModSettingsExtender. Needs more testing. |
| Audio Overrides          | 🟧 Moderate     | Simple implementation that should work in most cases. More testing is needed.                 |
| Texture Overrides        | 🟥 Experimental | New feature, likely to have issues. Needs more testing.                                       |
| Memory Cards             | 📄 Planned      | Not yet started.                                                                              |
| Phone Choices            | 📄 Planned      | Not yet started.                                                                              |
| Symbols                  | 📄 Planned      | Not yet started.                                                                              |
| Patterns                 | 📄 Planned      | Not yet started.                                                                              | 

Legend:
- ⭐ Excellent: Stable and fully functional.
- 🟩 Good: Mostly stable, minor issues possible.
- 🟨 Decent: Functional but may have some issues.
- 🟧 Moderate: New feature, needs more testing.
- 🟥 Experimental: Very new, likely to have issues.
- ⚠️ Unstable: Known major issues, use with caution.
- ❌ Broken: Not functional due to game updates or other issues.
- 📄 Planned: Not yet started.