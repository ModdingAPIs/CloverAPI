using System;
using System.Numerics;
using CloverAPI.Classes;
using CloverAPI.Content.Charms;
using CloverAPI.Content.Strings;
using static PowerupScript;

namespace CloverAPI.Content.Builders;

public class CharmBuilder
{
	public enum ModelMode
	{
		ExistingModel,
		GenericModel,
		CustomModel
	}

	internal Identifier id = Identifier.undefined;

	internal string guid;

	private Category _category = Category.normal;

	private Archetype _archetype = Archetype.generic;

	private bool _isInstantPowerup = false;

	private int _maxBuyTimes = -1;

    private float _storeRerollChance = STORE_REROLL_CHANCE_COMMON;

    private int _startingPrice = PRICE_NORMAL;

	private BigInteger _unlockPrice = -1L;

	private StringSource _name = "Unnamed Lucky Charm";

	private StringSource _description = "No description set.";

	private StringSource _unlockMission = new VanillaLocalizedString("POWERUP_UNLOCK_MISSION_NONE");

	private PowerupEvent _onEquip;

	private PowerupEvent _onUnequip;

	private PowerupEvent _onPutInDrawer;

	private PowerupEvent _onThrowAway;

    private ModelMode _modelMode = ModelMode.GenericModel;

	private string _customModelAssetBundlePath;

	private string _customModelName;

    private Identifier _sourceModel = Identifier.undefined;

	private TextureSource _texture = null;

	private CharmBuilder(string nameSpace, string identifier)
	{
		if (string.IsNullOrEmpty(nameSpace))
		{
			throw new ArgumentException("Namespace cannot be null or empty.", "nameSpace");
		}
		if (string.IsNullOrEmpty(identifier))
		{
			throw new ArgumentException("Identifier cannot be null or empty.", "identifier");
		}
		if (identifier.Contains("."))
		{
			throw new ArgumentException("Identifier cannot contain dots.", "identifier");
		}
		guid = nameSpace + "." + identifier;
	}

	public static CharmBuilder Create(string nameSpace, string identifier)
	{
		return new CharmBuilder(nameSpace, identifier);
	}

	public static CharmBuilder Create(string nameSpace, string identifier, CharmScript script)
	{
		return Create(nameSpace, identifier).WithScript(script);
	}

	public CharmBuilder WithCategory(Category category)
	{
		_category = category;
		return this;
	}

	public CharmBuilder WithArchetype(Archetype archetype)
	{
		_archetype = archetype;
		return this;
	}

	public CharmBuilder WithIsInstantPowerup(bool isInstantPowerup = true)
	{
		_isInstantPowerup = isInstantPowerup;
		return this;
	}

	public CharmBuilder WithMaxBuyTimes(int maxBuyTimes)
	{
		_maxBuyTimes = maxBuyTimes;
		return this;
	}

	public CharmBuilder WithStoreRerollChance(float storeRerollChance)
	{
		_storeRerollChance = storeRerollChance;
		return this;
	}

	public CharmBuilder WithStartingPrice(int startingPrice)
	{
		_startingPrice = startingPrice;
		return this;
	}

	public CharmBuilder WithUnlockPrice(BigInteger unlockPrice)
	{
		_unlockPrice = unlockPrice;
		return this;
	}
    
    public CharmBuilder WithUnlockPrice(long unlockPrice)
    {
        _unlockPrice = unlockPrice;
        return this;
    }
    
    public CharmBuilder WithName(StringSource name)
    {
        _name = name;
        return this;
    }

	public CharmBuilder WithName(string name)
	{
		_name = name;
		return this;
	}

    public CharmBuilder WithDescription(StringSource description)
    {
        _description = description;
        return this;
    }

	public CharmBuilder WithDescription(string description)
	{
		_description = description;
		return this;
	}
    
    public CharmBuilder WithUnlockMission(StringSource unlockMission)
    {
        _unlockMission = unlockMission;
        return this;
    }

	public CharmBuilder WithUnlockMission(string unlockMission)
	{
		_unlockMission = unlockMission;
		return this;
	}

	public CharmBuilder WithOnEquipEvent(PowerupEvent onEquip)
	{
		_onEquip = onEquip;
		return this;
	}

	public CharmBuilder WithOnUnequipEvent(PowerupEvent onUnequip)
	{
		_onUnequip = onUnequip;
		return this;
	}

	public CharmBuilder WithOnPutInDrawerEvent(PowerupEvent onPutInDrawer)
	{
		_onPutInDrawer = onPutInDrawer;
		return this;
	}

	public CharmBuilder WithOnThrowAwayEvent(PowerupEvent onThrowAway)
	{
		_onThrowAway = onThrowAway;
		return this;
	}

	public CharmBuilder WithScript(CharmScript script)
	{
		_onEquip = script.OnEquip;
		_onUnequip = script.OnUnequip;
		_onPutInDrawer = script.OnPutInDrawer;
		_onThrowAway = script.OnThrowAway;
        script.SetCharmReference(this);
		return this;
	}

	public CharmBuilder WithScript<T>() where T : CharmScript, new()
	{
		return WithScript(new T());
	}

    public CharmBuilder WithTextureModel(TextureSource texture)
    {
        _modelMode = ModelMode.GenericModel;
        _texture = texture;
        return this;
    }
    
	public CharmBuilder WithExistingModel(Identifier sourceModel, TextureSource texture = null)
	{
		_modelMode = ModelMode.ExistingModel;
		_sourceModel = sourceModel;
		_texture = texture;
		return this;
	}

	public CharmBuilder WithCustomModel(string assetBundlePath, string modelName = null, TextureSource texture = null)
	{
		_modelMode = ModelMode.CustomModel;
		_customModelAssetBundlePath = assetBundlePath;
		_customModelName = modelName;
		_texture = texture;
		return this;
	}

	public string GetName()
	{
		return _name;
	}

	public string GetDescription()
	{
		return _description;
	}

	public Category GetCategory()
	{
		return _category;
	}

	public Archetype GetArchetype()
	{
		return _archetype;
	}

	public bool IsInstantPowerup()
	{
		return _isInstantPowerup;
	}

	public int GetMaxBuyTimes()
	{
		return _maxBuyTimes;
	}

	public float GetStoreRerollChance()
	{
		return _storeRerollChance;
	}

	public int GetStartingPrice()
	{
		return _startingPrice;
	}

	public BigInteger GetUnlockPrice()
	{
		return _unlockPrice;
	}

	public string GetUnlockMission()
	{
		return _unlockMission;
	}

	public PowerupEvent GetOnEquipEvent()
	{
		return _onEquip;
	}

	public PowerupEvent GetOnUnequipEvent()
	{
		return _onUnequip;
	}

	public PowerupEvent GetOnPutInDrawerEvent()
	{
		return _onPutInDrawer;
	}

	public PowerupEvent GetOnThrowAwayEvent()
	{
		return _onThrowAway;
	}

	public ModelMode GetModelMode()
	{
		return _modelMode;
	}

	public string GetCustomModelAssetBundlePath()
	{
		return _customModelAssetBundlePath;
	}

	public string GetCustomModelName()
	{
		return _customModelName;
	}

	public Identifier GetSourceModel()
	{
		return _sourceModel;
	}

	public Texture2D GetTexture()
	{
		return _texture?.GetTexture();
	}

	public Identifier GetID()
	{
		return id;
	}

	public string GetGUID()
	{
		return guid;
	}

	public string GetNamespace()
	{
		int lastDotIndex = guid.LastIndexOf('.');
		return guid.Substring(0, lastDotIndex);
	}

	public string GetIdentifier()
	{
		int lastDotIndex = guid.LastIndexOf('.');
		return guid.Substring(lastDotIndex + 1);
	}

	internal void _Initialize(PowerupScript obj, bool isNewGame)
	{
		if (id == Identifier.undefined)
		{
			throw new InvalidOperationException("Charm " + _name + " is not registered. Call BuildAndRegister() before using the charm.");
		}
		obj.Initialize(isNewGame, _category, id, _archetype, _isInstantPowerup, _maxBuyTimes, _storeRerollChance, _startingPrice, _unlockPrice, LocalizationManager.Add("MODDED_CHARM_" + guid + "_NAME", _name), LocalizationManager.Add("MODDED_CHARM_" + guid + "_DESC", _description), LocalizationManager.Add("MODDED_CHARM_" + guid + "_MISSION", _unlockMission), _onEquip, _onUnequip, _onPutInDrawer, _onThrowAway);
	}

	[Obsolete("Use BuildAndRegister() for clarity.")]
	public Identifier Build()
	{
		return BuildAndRegister();
	}

	public Identifier BuildAndRegister()
	{
		return CharmManager.RegisterCharm(this);
	}
}
