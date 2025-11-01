namespace Warslammer.Core
{
    /// <summary>
    /// Represents the overall state of the game
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Battle,
        ArmyBuilder,
        Campaign,
        Paused
    }

    /// <summary>
    /// Represents the current phase within a player's turn
    /// </summary>
    public enum GamePhase
    {
        TurnStart,
        Movement,
        Action,
        TurnEnd
    }

    /// <summary>
    /// Types of units in the game
    /// </summary>
    public enum UnitType
    {
        Infantry,
        Cavalry,
        Monster,
        Hero,
        Vehicle,
        Artillery
    }

    /// <summary>
    /// Faction types for armies and units
    /// </summary>
    public enum FactionType
    {
        None,           // Neutral/no faction
        Empire,         // Human empire (placeholder)
        Chaos,          // Chaos forces (placeholder)
        Eldar,          // Space elves (placeholder)
        Orks,           // Greenskins (placeholder)
        Necrons,        // Undead robots (placeholder)
        Tau             // Tech aliens (placeholder)
    }

    /// <summary>
    /// Types of abilities that units can have
    /// </summary>
    public enum AbilityType
    {
        Passive,
        Active,
        Reaction,
        Aura
    }

    /// <summary>
    /// When an ability can be triggered
    /// </summary>
    public enum AbilityTiming
    {
        Always,
        TurnStart,
        MovementPhase,
        ActionPhase,
        TurnEnd,
        OnDamaged,
        OnAttack,
        OnDeath,
        OnActivation
    }

    /// <summary>
    /// Size of unit bases for measurement and positioning
    /// </summary>
    public enum BaseSize
    {
        Small_25mm,
        Medium_40mm,
        Large_60mm,
        Huge_80mm,
        Gargantuan_100mm
    }

    /// <summary>
    /// Source of damage for tracking resistances and vulnerabilities
    /// </summary>
    public enum DamageSource
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
        Holy,
        Dark,
        Psychic
    }

    /// <summary>
    /// Types of terrain features on the battlefield
    /// </summary>
    public enum TerrainType
    {
        Open,
        Forest,
        Hill,
        Building,
        Water,
        Obstacle,
        Impassable
    }

    /// <summary>
    /// Range categories for attacks and abilities
    /// </summary>
    public enum RangeType
    {
        Melee,
        Ranged,
        Self,
        Aura
    }

    /// <summary>
    /// Types of equipment units can have
    /// </summary>
    public enum EquipmentType
    {
        Weapon,
        Armor,
        Accessory,
        Mount
    }

    /// <summary>
    /// Weapon categories
    /// </summary>
    public enum WeaponType
    {
        Sword,
        Axe,
        Spear,
        Bow,
        Crossbow,
        Staff,
        Wand,
        Gun,
        Artillery
    }

    /// <summary>
    /// Player types in the game
    /// </summary>
    public enum PlayerType
    {
        Human,
        AI,
        NetworkRemote
    }

    /// <summary>
    /// AI difficulty levels
    /// </summary>
    public enum AIDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    /// <summary>
    /// Campaign mission types
    /// </summary>
    public enum MissionType
    {
        Battle,
        Story,
        Tutorial,
        Challenge
    }

    /// <summary>
    /// Victory conditions for battles
    /// </summary>
    public enum VictoryCondition
    {
        Elimination,
        ObjectiveControl,
        Survival,
        Assassination,
        Custom
    }
}