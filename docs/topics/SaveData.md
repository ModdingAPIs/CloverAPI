# 💾 Persistent Data Storage

<show-structure for="chapter" depth="2"/>

<link-summary>
Documentation for storing persistent data to CloverPit using CloverAPI.
</link-summary>

CloverAPI provides a simple way to store persistent data using key-value pairs. This is useful for saving player progress, settings, or any other data that needs to persist between game sessions.

## Saving Data
You can use the `PersistentData` class as an interface to save and load data. In most cases, the `JsonPersistentData` class that automatically serializes and deserializes data to and from JSON format is sufficient. If not, you can manually override the `ToString()` and `FromString(string data)` methods to implement your own serialization format.
To register the persistent data to save and load automatically alongside the game's own save data, use the `PersistentDataManager.Register(string key, PersistentData data)` method. The `key` parameter is a unique identifier for the data and used as the filename for saving the data.  
You can edit the values however you want, but if you only want to update it when the game is saved, you can override the `BeforeSave()`. Likewise, you can use `AfterLoad()`, `AfterSave()`, and `BeforeLoad()` to run code at those times.  
If your data applies to the current run only, you should clear/reset it on `OnReset()`.
```C#
public class SomePersistentData : JsonPersistentData
{
    public int someValue = 0;
    public string someString = "Hello, World!";
    
    public override void BeforeSave()
    {
        someValue++;
    }
    
    public override void OnReset()
    {
        someValue = 0;
    }
}
```

## Charm Data
Charms have their dedicated persistent data class. You can access data of individual charms using `CharmManager.GetCharmDataByID(PowerupScript.Identifier id)`, its `ByGUID()` counterpart, or the `Data` property inside a `CharmScript` instance.  
Read more about Charms in the [Charms](Charms.md) documentation.