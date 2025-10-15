# CloverPit API

API for CloverPit

## For Mod Users

You can find the Clover API on Thunderstore:
- [CloverAPI on Thunderstore](https://thunderstore.io/c/clover-pit/p/ModdingAPIs/CloverAPI/)

## Automatic Installation

Just search for "CloverAPI" in one of these mod managers and install it like any other mod:
- [r2modman](https://ingoh.net/r2modman.zip) (Unofficial build until the next release is out)
- [Thunderstore Mod Manager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager)

## Manual Installation

Download the latest release from Thunderstore or GitHub and place the DLL in your `BepInEx/plugins` folder like any other mod.

The API doesn't do anything on its own, so you'll need to install mods that use it to see any effect.

## For Mod Developers

If you're a mod developer looking to use CloverAPI, here are some resources to help you get started:

- See the [BepInEx](https://docs.bepinex.dev/api/index.html)
  and [Harmony](https://harmony.pardeike.net/articles/intro.html) docs for the basics of modding.
- See the [CloverAPI documentation](https://ingoh.net/cloverapi/) for more information on how to use the API.
- Ask around in the [Panik Games Discord](https://discord.gg/ytgv)'s modding channel if you need help.
- Check out the [Example Mod](https://github.com/IngoHHacks/CloverPitExampleMod) for a basic mod structure and some
  example usages of the API.

The API is currently in beta. Breaking changes may happen, but we'll try to keep them to a minimum.  
Game updates may also break the API. We'll fix it as soon as we can, but please be patient.

It's recommended to add the API as a dependency for all mods, even if you don't use it directly. The API includes various
features that are important for modding, like the save file compatibility layer and showing that mods are installed
on the screen. The developers requested to show whether mods are installed to prevent cheating and confusion.
Please respect this request.

The APIs distribution contains Newtonsoft JSON. Please do not include it in your mod as well, as it may cause conflicts.
Of course, you can still use it in your mod, just don't include it in the distribution.

## License
See [LICENSE](LICENSE) for more information.

## Credits
- IngoH - Project lead, maintainer, and developer
- pharmacomaniac (a.k.a. zapybibby) - Settings integration
- Matteo & Lorenzo - The devs of CloverPit!

Thanks to everyone who provided feedback and helped to test the API!

## Fun Fact
The name "CloverAPI" is a portmanteau of "CloverPit" and "API".  
Really creative, I know. Sometimes I amaze even myself.

Let's go gambling! 🍀
