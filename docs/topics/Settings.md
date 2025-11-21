# ⚙️ Settings & Configuration

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for adding settings to the in-game settings menu using CloverAPI.
</link-summary>

**Code Reference**  
`namespace: CloverAPI.Content.Settings`  
`class: ModSettingsManager`

You can add custom settings to the in-game settings menu using `ModSettingsManager`. This allows players to configure your mod's behavior directly from the game's settings interface.

## Adding Settings
It is recommended to use BepInEx's built-in configuration system to manage your mod's settings. You can use `ConfigEntry<T>` directly in the `ModSettingsManager` methods to create settings that are automatically synchronized with BepInEx's configuration files.  
You can register settings either manually or automatically. Manual registration gives you more control over the setting properties (min, max, step, etc.) and page layout, while automatic registration is quicker and easier to implement if you don't need custom behavior.

1. Manual settings registration
`ModSettingsManager.RegisterPage(BaseUnityPlugin owner, string name, Action<PageBuilder>? configure = null)`
Registers a new empty settings page. You can then add settings to it either in the optional `configure` callback or later using the returned `PageBuilder` instance.

2. Automatic settings registration
`ModSettingsManager.RegisterPageFromConfig(BaseUnityPlugin owner, string name, string[]? ignoredKeys = null, string[]? ignoredCategories = null, Action<PageBuilder>? configure = null)`
Registers a new settings page and automatically adds settings based on the `Config` entries defined in your mod (the passed `owner` parameter).  
This creates a page from the *current* state of the `Config` entries, so make sure to call this method after all your `Config.Bind()` calls that you want to include.

There are also deprecated overloads without the `owner` parameter. These should be avoided, as they rely on auto-detection of the calling mod, which may not always work correctly.

### Parameters
The `owner` parameter is your mod's main class that inherits from `BaseUnityPlugin`.  
The `name` parameter is the name of the settings page.
The optional `configure` parameter is a callback that allows you edit the page inline. This is not required, as you can also add settings later using the returned `PageBuilder` instance.
For automatic registration, the optional `ignoredKeys` and `ignoredCategories` parameters allow you to exclude specific settings from being added to the page. These are the `key` and `section` as defined in the `ConfigFile.Bind()` method (case-insensitive). As an alternative for excluding specific settings, you can simply call `ModSettingsManager.RegisterPageFromConfig()` before defining those settings in your mod with `Config.Bind()`.

### Automatic Generation Details
When using automatic settings registration, the following `ConfigEntry<T>` types are supported and mapped to corresponding setting types in the settings menu:

| ConfigEntry Type | Setting Type    | Additional Info                                                                                                                                                                                                                                                                                                                                                                  |
|------------------|-----------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `bool`           | On/Off Switch   |                                                                                                                                                                                                                                                                                                                                                                                  |
| `int`            | Integer Stepper | If `AcceptableValuesRange<int>` is set, min and max are taken from it. Otherwise, no limits are applied. All other parameters are default.                                                                                                                                                                                                                                       |
| `float`          | Percent         | If `AcceptableValuesRange<float>` is set, min and max are taken from it. If min and max are in range [0-0.5]-[0.5-5], the setting is shown as a percentage and a scale factor of 100 is applied. In all other cases, the setting is shown as a normal float. Step is based on the difference between min and max with the last significant digit being rounded to the nearest 5. |
| `string`         | Cycle           | If `AcceptableValuesList<string>` is set, the setting is shown as a cycle with the provided values. Otherwise, it is ignored and not added to the page. Text input is currently not supported, but planned for a future update.                                                                                                                                                  |
| `Enum` (any)     | Cycle           | The setting is shown as a cycle with all enum values.                                                                                                                                                                                                                                                                                                                            |
| `KeybindBinding` | Keybind         | Unified keybind picker for keyboard or controller input. Stored as `Device:Element` (e.g. `Keyboard:Space`, `Joystick:LeftTrigger`).                                                                                                                                                                                                                                         |

```C#
using BepInEx;
using BepInEx.Configuration;

public class ExampleMod : BaseUnityPlugin
{
    private ConfigEntry<bool> toggleDisplay;
    private ConfigEntry<int> luckModifier;
    private ConfigEntry<float> tripleSixChance;
    private ConfigEntry<float> coinMultiplier;
    private ConfigEntry<string> difficultyMode;
    
    private void Awake()
    {
        // ...
        MakeConfig();
    }
    
    private void MakeConfig()
    {
        toggleDisplay = Config.Bind("General", "Toggle Display", true, "Turns the mod on/off");
        luckModifier = Config.Bind("General", "Luck Modifier", 0, new ConfigDescription("Adds base luck (0-15).", new AcceptableValueRange<int>(0, 15)));
        tripleSixChance = Config.Bind("General", "Triple 666 Chance", 1.5f, new ConfigDescription("Percent chance for Triple 666 to occur.", new AcceptableValueRange<float>(0f, 30f)));
        coinMultiplier = Config.Bind("General", "Coin Multiplier", 1f, new ConfigDescription("Applies a coin multiplier between 0.5x and 4x.", new AcceptableValueRange<float>(0.5f, 4f)));
        difficultyMode = Config.Bind("General", "Difficulty Mode", "Harder", new ConfigDescription("Cycle through difficulty presets.", new AcceptableValueList<string>("Harder", "Better", "Faster", "Stronger")));
    
#if MANUAL_SETTINGS
        // Registers your page using your plugin's GUID
        ModSettingsManager.RegisterPage(this, "Example Mod", page =>
        {
            page.OnOff("Toggle Display", toggleDisplay);
            page.Int("Luck Modifier", luckModifier, min: 0, max: 15, step: 1);
            page.Percent("Triple 666 Chance", tripleSixChance, minPercent: 0f, maxPercent: 30f, step: 0.5f);
            page.Multiplier("Coin Multiplier", coinMultiplier, minMultiplier: 0.5f, maxMultiplier: 4f, step: 0.5f);
            page.Cycle("Difficulty Mode", difficultyMode, "Harder", "Better", "Faster", "Stronger");
        });
#else
        // Registers your page and automatically adds all Config entries
        ModSettingsManager.RegisterPageFromConfig(this, "Example Mod");
#endif
    }
}
```

### Adding Individual Settings
Once you have a `PageBuilder` instance (either from manual or automatic registration), you can add individual settings of the following types:

#### On/Off Switch
`OnOff(string label, ConfigEntry<bool> entry, string onLabel = "On", string offLabel = "Off", Action<bool>? onChanged = null, ToggleAdjustMode adjustMode = ToggleAdjustMode.Toggle)`
Adds a toggle switch for a boolean setting.  
The `label` parameter is the text shown next to the switch.  
The `entry` parameter is the `ConfigEntry<bool>` that holds the setting value.  
The optional `onLabel` and `offLabel` parameters allow you to customize the text shown for the on and off states.  
The optional `onChanged` parameter is a callback that is invoked whenever the setting is changed, receiving the new value as a parameter.  
The optional `adjustMode` parameter determines how the switch behaves with the keyboard/gamepad. The default is `Toggle`, which toggles the value when activated. The `Directional` mode sets the value to Off when pressing left and On when pressing right.

#### Integer Stepper
`Int(string label, ConfigEntry<int> entry, int? min = null, int? max = null, int step = 1, bool wrap = false, Func<int, int>? normalizer = null, Func<int, string>? valueFormatter = null, Action<int>? onChanged = null)`
Adds a stepper for an integer setting.  
The `label` parameter is the text shown next to the stepper.  
The `entry` parameter is the `ConfigEntry<int>` that holds the setting value.  
The optional `min` and `max` parameters define the minimum and maximum values for the stepper. If not set, no limits are applied.  
The optional `step` parameter defines the increment/decrement step size. Default is 1.  
The optional `wrap` parameter determines whether the stepper wraps around when reaching the min or max value. Default is false.  
The optional `normalizer` parameter is a function that takes the current value and returns a normalized value. This can be used to adjust the value before displaying it.  
The optional `valueFormatter` parameter is a function that takes the current value and returns a formatted string for display.  
The optional `onChanged` parameter is a callback that is invoked whenever the setting is changed, receiving the new value as a parameter.

#### Percent
`Percent(string label, ConfigEntry<float> entry, float minPercent = 0f, float maxPercent = 100f, float step = 5f, bool wrap = false, int decimalPlaces = 1, bool showPercent = true, float scale = 1f, Action<float>? onChanged = null)`
Adds a stepper for a float setting displayed as a percentage.  
The `label` parameter is the text shown next to the stepper.  
The `entry` parameter is the `ConfigEntry<float>` that holds the setting value.  
The optional `minPercent` and `maxPercent` parameters define the minimum and maximum values for the stepper. Default is 0f and 100f.  
The optional `step` parameter defines the increment/decrement step size. Default is 5f.  
The optional `wrap` parameter determines whether the stepper wraps around when reaching the min or max value. Default is false.  
The optional `decimalPlaces` parameter defines the minimum number of decimal places to display. Default is 1.  
The optional `showPercent` parameter determines whether to append a percent sign (%) to the displayed value. Default is true.  
The optional `scale` parameter is a multiplier applied to the value for display purposes. Default is 1f (no scaling).  
The optional `onChanged` parameter is a callback that is invoked whenever the setting is changed, receiving the new value as a parameter.  

`Percent` also has a variant for `int`, which works the same way but doesn't have redundant parameter `decimalPlaces`.

#### Multiplier
`Multiplier(string label, ConfigEntry<float> entry, float minMultiplier, float maxMultiplier, float step, bool wrap = false, int decimalPlaces = 2, Func<float, string>? valueFormatter = null, Action<float>? onChanged = null)`
Adds a stepper backed by common 0× to 4× multipliers.  
The `label` parameter is the text shown next to the stepper.  
The `entry` parameter is the `ConfigEntry<float>` that holds the setting value.  
The `minMultiplier` and `maxMultiplier` parameters define the minimum and maximum values for the stepper.  
The `step` parameter defines the increment/decrement step size.  
The optional `wrap` parameter determines whether the stepper wraps around when reaching the min or max value. Default is false.  
The optional `decimalPlaces` parameter defines the minimum number of decimal places to display. Default is 2.  
The optional `valueFormatter` parameter is a function that takes the current value and returns a formatted string for display.  
The optional `onChanged` parameter is a callback that is invoked whenever the setting is changed, receiving the new value as a parameter.  
This stepper automatically snaps to common multipliers like 0.5×, 1×, 1.5×, 2×, etc., based on the provided min, max, and step values.

#### Cycle
`Cycle(string label, ConfigEntry<T> entry, params T[] options)`  
`Cycle(string label, ConfigEntry<T> entry, IReadOnlyList<T> values, Func<T, string>? valueFormatter = null, Action<T>? onChanged = null, IEqualityComparer<T>? comparer = null)`
Adds a cycle selector for a setting with multiple discrete options. Typically used for `string` or `Enum` types.  
The `label` parameter is the text shown next to the cycle selector.  
The `entry` parameter is the `ConfigEntry<string>` that holds the setting value.  
The `options` or `values` parameter is an array or list of possible values for the cycle.  
The optional `valueFormatter` parameter is a function that takes the current value and returns a formatted string for display.  
The optional `onChanged` parameter is a callback that is invoked whenever the setting is changed, receiving the new value as a parameter.  
The optional `comparer` parameter allows you to provide a custom equality comparer for the values. Default is `EqualityComparer<T>.Default`.

#### Keybind
`Keybind(string label, ConfigEntry<KeybindBinding> entry, bool allowKeyboard = true, bool allowJoystickButtons = true, bool allowJoystickAxes = true, Func<KeybindBinding, string>? valueFormatter = null, Action<KeybindBinding>? onChanged = null)`  
Adds a keybind field that listens to keyboard or controller and saves the first input it sees (click/Enter to start “Listening…”). Mouse bindings are not captured by the picker.  
The value is stored as a `KeybindBinding`, serialized as `Device:Element` (e.g. `Keyboard:Space`, `Joystick:LeftTrigger`). If you prefer to seed defaults by hand, you can pass a string to `Config.Bind` in that format.  
Use the `allow*` flags to disable keyboard input or joystick buttons/axes if you need tighter control.  
Keyboard `Esc`, `Backspace`, `Return`, and OS keys plus controller `Select`/`Start`/`Home` are filtered out to avoid trapping menu navigation buttons.  
Simple Keybind Example:
```csharp
ConfigEntry<KeybindBinding> dodgeKey = Config.Bind(
    "Controls",
    "Dodge",
    new KeybindBinding(Controls.InputKind.Keyboard, nameof(Controls.KeyboardElement.Space)),
    "Bind the dodge action (Keyboard/Controller).");

ModSettingsManager.RegisterPage(this, "Example Mod", page =>
{
    page.Keybind("Dodge", dodgeKey);
});
```
Optional `valueFormatter` lets you customize how the current binding is displayed.  
Optional `onChanged` lets you provide a callback that runs whenever the player saves a new binding (e.g., update UI or sync state). Example with `onChanged`:
```csharp
page.Keybind(
    "Dodge",
    dodgeKey,
    onChanged: binding => Logger.LogInfo($"Dodge rebound to {binding}"));
```

### Non-Config Settings
If you want to add settings that are not backed by `ConfigEntry<T>`, you can use the getter/setter-based overloads of the above methods. These require you to manage the storage and retrieval of the setting values yourself.  
These use `Func<T>` for getting the current value and `Action<T>` for setting a new value instead of using a `ConfigEntry<T>` reference for doing both.
