using CloverAPI.Classes;
using CloverAPI.Content.Charms;
using CloverAPI.Content.Strings;
using System;
using System.Numerics;
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

    private Archetype _archetype = Archetype.generic;

    private Category _category = Category.normal;

    private string _customModelAssetBundlePath;

    private string _customModelName;

    private StringSource _description = "No description set.";

    private bool _isInstantPowerup;

    private int _maxBuyTimes = -1;

    private ModelMode _modelMode = ModelMode.GenericModel;

    private StringSource _name = "Unnamed Lucky Charm";

    private PowerupEvent _onEquip;

    private PowerupEvent _onPutInDrawer;

    private PowerupEvent _onThrowAway;

    private PowerupEvent _onUnequip;

    private Identifier _sourceModel = Identifier.undefined;

    private int _startingPrice = PRICE_NORMAL;

    private float _storeRerollChance = STORE_REROLL_CHANCE_COMMON;

    private TextureSource _texture;

    private StringSource _unlockMission = new VanillaLocalizedString("POWERUP_UNLOCK_MISSION_NONE");

    private BigInteger _unlockPrice = -1L;

    internal string guid;

    internal Identifier id = Identifier.undefined;

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

        this.guid = nameSpace + "." + identifier;
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
        this._category = category;
        return this;
    }

    public CharmBuilder WithArchetype(Archetype archetype)
    {
        this._archetype = archetype;
        return this;
    }

    public CharmBuilder WithIsInstantPowerup(bool isInstantPowerup = true)
    {
        this._isInstantPowerup = isInstantPowerup;
        return this;
    }

    public CharmBuilder WithMaxBuyTimes(int maxBuyTimes)
    {
        this._maxBuyTimes = maxBuyTimes;
        return this;
    }

    public CharmBuilder WithStoreRerollChance(float storeRerollChance)
    {
        this._storeRerollChance = storeRerollChance;
        return this;
    }

    public CharmBuilder WithStartingPrice(int startingPrice)
    {
        this._startingPrice = startingPrice;
        return this;
    }

    public CharmBuilder WithUnlockPrice(BigInteger unlockPrice)
    {
        this._unlockPrice = unlockPrice;
        return this;
    }

    public CharmBuilder WithUnlockPrice(long unlockPrice)
    {
        this._unlockPrice = unlockPrice;
        return this;
    }

    public CharmBuilder WithName(StringSource name)
    {
        this._name = name;
        return this;
    }

    public CharmBuilder WithName(string name)
    {
        this._name = name;
        return this;
    }

    public CharmBuilder WithDescription(StringSource description)
    {
        this._description = description;
        return this;
    }

    public CharmBuilder WithDescription(string description)
    {
        this._description = description;
        return this;
    }

    public CharmBuilder WithUnlockMission(StringSource unlockMission)
    {
        this._unlockMission = unlockMission;
        return this;
    }

    public CharmBuilder WithUnlockMission(string unlockMission)
    {
        this._unlockMission = unlockMission;
        return this;
    }

    public CharmBuilder WithOnEquipEvent(PowerupEvent onEquip)
    {
        this._onEquip = onEquip;
        return this;
    }

    public CharmBuilder WithOnUnequipEvent(PowerupEvent onUnequip)
    {
        this._onUnequip = onUnequip;
        return this;
    }

    public CharmBuilder WithOnPutInDrawerEvent(PowerupEvent onPutInDrawer)
    {
        this._onPutInDrawer = onPutInDrawer;
        return this;
    }

    public CharmBuilder WithOnThrowAwayEvent(PowerupEvent onThrowAway)
    {
        this._onThrowAway = onThrowAway;
        return this;
    }

    public CharmBuilder WithScript(CharmScript script)
    {
        this._onEquip = script.OnEquip;
        this._onUnequip = script.OnUnequip;
        this._onPutInDrawer = script.OnPutInDrawer;
        this._onThrowAway = script.OnThrowAway;
        script.SetCharmReference(this);
        return this;
    }

    public CharmBuilder WithScript<T>() where T : CharmScript, new()
    {
        return WithScript(new T());
    }

    public CharmBuilder WithTextureModel(TextureSource texture)
    {
        this._modelMode = ModelMode.GenericModel;
        this._texture = texture;
        return this;
    }

    public CharmBuilder WithExistingModel(Identifier sourceModel, TextureSource texture = null)
    {
        this._modelMode = ModelMode.ExistingModel;
        this._sourceModel = sourceModel;
        this._texture = texture;
        return this;
    }

    public CharmBuilder WithCustomModel(string assetBundlePath, string modelName = null, TextureSource texture = null)
    {
        this._modelMode = ModelMode.CustomModel;
        this._customModelAssetBundlePath = assetBundlePath;
        this._customModelName = modelName;
        this._texture = texture;
        return this;
    }

    public string GetName()
    {
        return this._name;
    }

    public string GetDescription()
    {
        return this._description;
    }

    public Category GetCategory()
    {
        return this._category;
    }

    public Archetype GetArchetype()
    {
        return this._archetype;
    }

    public bool IsInstantPowerup()
    {
        return this._isInstantPowerup;
    }

    public int GetMaxBuyTimes()
    {
        return this._maxBuyTimes;
    }

    public float GetStoreRerollChance()
    {
        return this._storeRerollChance;
    }

    public int GetStartingPrice()
    {
        return this._startingPrice;
    }

    public BigInteger GetUnlockPrice()
    {
        return this._unlockPrice;
    }

    public string GetUnlockMission()
    {
        return this._unlockMission;
    }

    public PowerupEvent GetOnEquipEvent()
    {
        return this._onEquip;
    }

    public PowerupEvent GetOnUnequipEvent()
    {
        return this._onUnequip;
    }

    public PowerupEvent GetOnPutInDrawerEvent()
    {
        return this._onPutInDrawer;
    }

    public PowerupEvent GetOnThrowAwayEvent()
    {
        return this._onThrowAway;
    }

    public ModelMode GetModelMode()
    {
        return this._modelMode;
    }

    public string GetCustomModelAssetBundlePath()
    {
        return this._customModelAssetBundlePath;
    }

    public string GetCustomModelName()
    {
        return this._customModelName;
    }

    public Identifier GetSourceModel()
    {
        return this._sourceModel;
    }

    public Texture2D GetTexture()
    {
        return this._texture?.GetTexture();
    }

    public Identifier GetID()
    {
        return this.id;
    }

    public string GetGUID()
    {
        return this.guid;
    }

    public string GetNamespace()
    {
        int lastDotIndex = this.guid.LastIndexOf('.');
        return this.guid.Substring(0, lastDotIndex);
    }

    public string GetIdentifier()
    {
        int lastDotIndex = this.guid.LastIndexOf('.');
        return this.guid.Substring(lastDotIndex + 1);
    }

    internal void _Initialize(PowerupScript obj, bool isNewGame)
    {
        if (this.id == Identifier.undefined)
        {
            throw new InvalidOperationException("Charm " + this._name +
                                                " is not registered. Call BuildAndRegister() before using the charm.");
        }

        obj.Initialize(isNewGame, this._category, this.id, this._archetype, this._isInstantPowerup, this._maxBuyTimes,
            this._storeRerollChance, this._startingPrice, this._unlockPrice,
            LocalizationManager.RegisterTranslation("MODDED_CHARM_" + this.guid + "_NAME", this._name),
            LocalizationManager.RegisterTranslation("MODDED_CHARM_" + this.guid + "_DESC", this._description),
            LocalizationManager.RegisterTranslation("MODDED_CHARM_" + this.guid + "_MISSION", this._unlockMission), this._onEquip,
            this._onUnequip, this._onPutInDrawer, this._onThrowAway);
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