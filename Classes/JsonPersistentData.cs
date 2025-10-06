using Newtonsoft.Json;

namespace CloverAPI.Classes;

public class JsonPersistentData : PersistentData
{
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public override void FromString(string data)
    {
        JsonConvert.PopulateObject(data, this);
    }
}