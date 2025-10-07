using CloverAPI.Content.Builders;
using CloverAPI.SaveData;

namespace CloverAPI.Content.Charms;

public class CharmScript
{
    private CharmBuilder charmReference;

    public CharmData Data
    {
        get
        {
            if (this.charmReference == null)
            {
                return null;
            }

            return CharmManager.GetCharmDataByID(this.charmReference.id);
        }
    }

    public virtual void OnEquip(PowerupScript powerup) { }
    public virtual void OnUnequip(PowerupScript powerup) { }
    public virtual void OnPutInDrawer(PowerupScript powerup) { }
    public virtual void OnThrowAway(PowerupScript powerup) { }

    public CharmBuilder ToBuilder(string nameSpace, string name)
    {
        return CharmBuilder.Create(nameSpace, name, this);
    }

    internal void SetCharmReference(CharmBuilder charmBuilder)
    {
        this.charmReference = charmBuilder;
    }
}