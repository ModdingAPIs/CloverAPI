namespace CloverAPI.Classes;

public abstract class PersistentData
{
    public new abstract string ToString();
    public abstract void FromString(string data);

    public virtual void BeforeLoad() { }
    public virtual void AfterLoad() { }

    public virtual void BeforeSave() { }
    public virtual void AfterSave() { }

    public virtual void OnReset() { }
}