using CloverAPI.Content.Builders;
using CloverAPI.SaveData;

namespace CloverAPI.Content.Charms;

public class CharmScript
{
    protected CharmBuilder charmReference;
    
    public CharmData Data
    {
        get
        {
            if (charmReference == null) return null;
            return CharmManager.GetCharmDataByID(charmReference.id);
        }
    }
    
	public virtual void OnEquip(PowerupScript powerup) { }
	public virtual void OnUnequip(PowerupScript powerup) { }
	public virtual void OnPutInDrawer(PowerupScript powerup) { }
    public virtual void OnThrowAway(PowerupScript powerup) { }

    public CharmBuilder ToBuilder(string nameSpace, string name) => CharmBuilder.Create(nameSpace, name, this);

    internal void SetCharmReference(CharmBuilder charmBuilder)
    {
        charmReference = charmBuilder;
    }
}
