# 📦 Charms

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for adding custom Charms to CloverPit using CloverAPI.
</link-summary>

## Overview

Adding custom Charms to CloverPit is made easy with CloverAPI.  
To get started, use `CharmManager.Builder()` to create a new Charm.  
Then, use the builder methods to set the properties of the Charm.  
Finally, call `BuildAndRegister()` to register the Charm with the game.  
The builder returns a `PowerupScript.Identifier` that can be used to reference the Charm in your code.  
Be careful! The Identifier may be different between game sessions if mods are added or removed. To store a reference to
a Charm outside of single sessions, use the GUID (`GetGUID()`) and `CharmManager`'s `GetCharmDataByGUID()` method.

## Example

```C#
using CloverAPI.Content.Charms;
using static PowerupScript; // For Category, Archetype, and constants

internal static PowerupScript.Identifier MyCharm;

public static void RegisterCharms()
{
    MyCharm = CharmManager.Builder("mymod", "my_charm") // Namespace, Name
        .WithName("My Charm") // Display name
        .WithDescription("This is my custom charm.") // Description
        .WithTextureModel(Path.Combine(Plugin.ImagePath, "my_charm.png")) // Texture path (for basic texture Charms)
        .BuildAndRegister(); // Register the Charm
}
```

## Charm Properties

The following properties can be set using the builder methods:

### Special Properties

#### GUID

A unique identifier for the Charm. Used to reference the Charm in a way that is stable between game sessions.  
Cannot be set manually, is generated from the namespace and name.

- Method: `GetGUID()`
- Default: {namespace}.{name} (e.g. "mymod.my_charm")

### General Properties

#### Name

The display name of the Charm.

- Method: `WithName(string name)` or `WithName(StringSource name)`
- Default: `Unnamed Lucky Charm`

#### Description

The description of the Charm.

- Method: `WithDescription(string description)` or `WithDescription(StringSource description)`
- Default: `No description set.`

#### Unlock Mission

The description of the unlock condition for the Charm.

- Method: `WithUnlockMission(string unlockMission)` or `WithUnlockMission(StringSource unlockMission)`
- Default: `VanillaLocalizedString("POWERUP_UNLOCK_MISSION_NONE")`

#### Category

The category of the Charm (normal or skeleton).

- Method: `WithCategory(PowerupScript.Category category)`
- Default: `PowerupScript.Category.normal`
  Settings this is not recommended, as skeleton Charms are special and things may break if used by something other than
  the originals

#### Archetype

The archetype of the Charm (e.g. generic, spicyPeppers, religious, etc.).

- Method: `WithArchetype(PowerupScript.Archetype archetype)`
- Default: `PowerupScript.Archetype.generic`

#### Is Instant Powerup

Whether the Charm acts as an instant powerup, activating immediately and not going in one of the Charm slots.

- Method: `WithIsInstantPowerup(bool isInstantPowerup = true)`
- Default: `false`

#### Max Buy Times

The maximum number of times the Charm can be bought from shops. Set to -1 for infinite.

- Method: `WithMaxBuyTimes(int maxBuyTimes)`
- Default: `-1`

#### Store Reroll Chance

The chance the Charm will be rerolled when selected to appear in the shop.  
Effectively the rarity of the Charm, higher values mean the Charm is less likely to appear.

- Method: `WithStoreRerollChance(float storeRerollChance)`
- Default: `PowerupScript.STORE_REROLL_CHANCE_COMMON` (0f)
  The PowerupScript class has predefined constants for the rarities used by vanilla Charms (
  `STORE_REROLL_CHANCE_COMMON` (0f) to STORE_REROLL_CHANCE_LEGENDARY_ULTRA (0.8f)), but you can use any float value
  between 0 and 1.

#### Starting Price

The default price of the Charm without any Traits or Charm effects modifying it.

- Method: `WithStartingPrice(int startingPrice)`
- Default: `PowerupScript.PRICE_NORMAL` (3)
  The PowerupScript class has predefined constants for the prices used by vanilla Charms (`PRICE_FREE` (0) to
  `PRICE_UNREACHABLE_HIGH_HIGH` (5000)), but you can use any integer value.

#### Unlock Price

An unused property. Presumably used for Charms that are unlocked by buying them. May work, but untested.

- Method: `WithUnlockPrice(BigInteger unlockPrice)` or `WithUnlockPrice(long unlockPrice)`
- Default: -1

### Visual Properties

Charms can be represented visually in three different ways:

- A flat texture (rendered on a basic painting-like model)
- Retextured vanilla Charm models
- A custom 3D model

If none are set, it will use the "flat texture" model with no texture (magenta and black checkerboard)

#### Texture Model (easiest)

Texture models are the easiest. You just need to provide a path to a texture image file (any common format like PNG or
JPG should work) or Texture2D object. The texture will be rendered on a simple flat model, similar to a painting.

- Method: `WithTextureModel(TextureSource texturePath)`

#### Retextured Model (intermediate)

Retextured models use the vanilla Charm models, but with a custom texture. You need to provide the Identifier of a
vanilla Charm to use, and a path to a texture image file or Texture2D object. The texture will be applied to the model
of the specified vanilla Charm.

- Method: `WithExistingModel(PowerupScript.Identifier sourceModel, TextureSource texture)`
  Vanilla Charms' textures take some effort to replace, as you need to extract them from the game's assets first. You
  can't just use any PNG file because the model's UVs are designed for the original texture. You can use a tool like
  AssetStudio to extract the textures from the game's asset files.

#### Custom Model (hardest)

Custom models allow you to use any 3D model you want. You need to provide a path to an AssetBundle containing the model.
If you have an AssetBundle with multiple models, you also need to provide the name of the model inside the AssetBundle.
You can also provide a texture to override the model's texture, but this is optional.

- Method: `WithCustomModel(string assetBundlePath, string modelName = null, TextureSource texture = null)`
  Creating AssetBundles is outside the scope of this documentation, but there are many tutorials available online. Make
  sure to create the AssetBundle using the same version of Unity that the game uses (Unity 6000.0.37f1 at the time of
  writing). The model needs to contain all the stuff vanilla Charm models have, like outline, particle, modifier
  components, etc. You'll need to use a tool like AssetRipper to recompile the Unity project, then copy any of the
  existing Charm's prefabs and modify them to use your model.

### Event Properties

Charms have four events that can be hooked into: `OnEquip`, `OnUnequip`, `OnPutInDrawer`, and `OnThrowAway`.  
You may want your charms to do something for other events, like after scoring on the slot machine or at the start of a
deadline.
To do this, subscribe to the relevant event(s) on `OnEquip` and unsubscribe on `OnUnequip`.
<warning>
<code>OnEquip</code> also gets called when the charm is loaded by the game on startup if it's in your charm slots, so make sure your code can handle that case if necessary.
</warning>

To set the event handlers, you could use the basic builder methods
`WithOnEquipEvent(PowerupScript.PowerupEvent onEquip)`, `WithOnUnequipEvent(...)`, `WithOnPutInDrawerEvent(...)`, and
`WithOnThrowAwayEvent(...)` if the charm's effect is simple. However, in most cases, you'll want to use a class
extending `CharmScript` to implement the charm's behavior and pass it to the builder using `WithScript<T>()` (where T is
your class extending `CharmScript`). This is the recommended way to implement charm behavior.

#### CharmScript

To implement custom behavior for your Charm, create a class that extends `CharmScript`. The overrides are the four
previously mentioned events, and all of them are optional to implement.

```C#
using CloverAPI.Content.Charms;

internal class MyCharmScript : CharmScript
{
    public override void OnEquip()
    {
        SlotMachineScript.instance.OnScoreEvaluationEnd += MyCharmEffect;
    }

    public override void OnUnequip() 
    {
        SlotMachineScript.instance.OnScoreEvaluationEnd -= MyCharmEffect;
    }
    
    private void MyCharmEffect()
    {
        // Example effect: Do something if the player hit the jackpot
        if (SlotMachineScript.HasJackpot())
        {
            // Do stuff here
        }
    }
}
```

Then use the builder method `WithScript<MyCharmScript>()` to set the script for your Charm.  
Alternatively, you can use `new MyCharmScript().ToBuilder()` to get a builder with the script set or pass an instance of
your script to the `CharmManager.Builder()` method as extra argument.

### Persistent Data

If your Charm needs to store persistent data (e.g. number of times the effect has triggered), you can use CloverAPI's
persistent data storage system.  
Charms have a dedicated storage that you can access using the `Data` property of the `CharmScript` instance, or the
`CharmManager.GetCharmDataByID()` method (or the `ByGUID` variant). This returns a `CharmData` object that you can use
to store and retrieve data using key-value pairs.

```C#
using CloverAPI.Content.Charms;

internal class MyCharmScript : CharmScript
{
    private int TriggerCount
    {
        get => Data.Get("triggerCount", 0);
        set => Data.Set("triggerCount", value);
    }
    
    public override void OnEquip()
    {
        SlotMachineScript.instance.OnScoreEvaluationEnd += OnTrigger;
    }
       
    public override void OnUnequip()
    {
        SlotMachineScript.instance.OnScoreEvaluationEnd -= OnTrigger;
    }
    
    private void OnTrigger()
    {
        if (SlotMachineScript.HasJackpot())
        {
            TriggerCount++;
            if (TriggerCount % 7 == 0)
            {
                // Do stuff here
            }
        }
    }
}
```

For more about persistent data storage, see the [Persistent Data Storage](SaveData.md) documentation.