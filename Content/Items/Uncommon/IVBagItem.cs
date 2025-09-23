using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class IVBagItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<int> Target_Count;
    public static ConfigEntry<int> Target_Count_Stack;
    public static ConfigEntry<float> Heal_Percent;
    public static ConfigEntry<float> Heal_Percent_Stack;
    public static ConfigEntry<float> Flat_Armor;
    public static ConfigEntry<float> Flat_Armor_Stack;
    public static ConfigEntry<float> Radius;
    public static ConfigEntry<float> Radius_Stack;

    protected override string Name => "IVBag";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier2;
    protected override ItemTag[] Tags => [ItemTag.Healing, ItemTag.Utility];
    protected override bool IsRemovable => true;

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("IVBagModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("IVBagRender");

    protected override string DisplayName => "IV Bag";
    protected override string Description => FuseText([
        string.Format("Tether ".Style(FontColor.cIsHealing) + "to the nearest {0} " + "({1} per stack) ".Style(FontColor.cStack).OptText(Target_Count_Stack.Value != 0) + "allies ".OptText("ally ", Target_Count_Stack.Value != 0 || Target_Count.Value > 1),
        Target_Count.Value, Target_Count_Stack.Value.SignVal()),

        string.Format("within " + "{0}m".Style(FontColor.cIsUtility) + " ({1}m per stack)".Style(FontColor.cStack).OptText(Radius_Stack.Value != 0) + ", ",
        RoundVal(Radius.Value), RoundVal(Radius_Stack.Value).SignVal()),

        string.Format("sharing " + "{0}% ".Style(FontColor.cIsHealing) + "({1}% per stack) ".Style(FontColor.cStack).OptText(Heal_Percent_Stack.Value != 0) + "of " + "all healing".Style(FontColor.cIsHealing) + ". ",
        RoundVal(Heal_Percent.Value), RoundVal(Heal_Percent_Stack.Value).SignVal()),

        string.Format("Increase armor ".Style(FontColor.cIsHealing) + "by " + "{0} ".Style(FontColor.cIsHealing) + "({1} per stack) ".Style(FontColor.cStack).OptText(Flat_Armor_Stack.Value != 0) + "while " + "untethered".Style(FontColor.cIsHealing) + ".",
        RoundVal(Flat_Armor.Value), RoundVal(Flat_Armor_Stack.Value).SignVal())
    ]);
    protected override string PickupText => "Tether to " + "a nearby ally".OptText("nearby allies", Target_Count.Value > 1) + ", share health and gaining armor." ;

    protected override bool IsEnabled()
    {
        Target_Count = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Tether Count", 1,
            "[ 1 = 1 | Max Tether to Ally ]"
        );
        Target_Count_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Tether Count Stack", 1,
            "[ 1 = +1 | Max Tether to Ally per Item Stack | 0 to Disable ]"
        );

        Heal_Percent = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Healing Shared", 50f,
            "[ 50 = 50% | Healing Shared to Tethered Ally ]"
        );
        Heal_Percent_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Healing Shared Stack", 0f,
            "[ 50 = +50% | Healing Shared per Item Stack | 0 to Disable ]"
        );

        Flat_Armor = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Armor Gain", 25f,
            "[ 25 = 25 | Armor Gained ]"
        );
        Flat_Armor_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Armor Gain Stack", 10f,
            "[ 10 = +10 | Armor Gained per Item Stack | 0 to Disable ]"
        );

        Radius = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Radius", 30f,
            "[ 30 = 30 | Meter Radius ]"
        );
        Radius_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Radius Stack", 0f,
            "[ 10 = +10 | Meter Radius per Item Stack | 0 to Disable ]"
        );

        Item_Enabled = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        Target_Count.Value = Mathf.Max(Target_Count.Value, 0);
        Target_Count_Stack.Value = Mathf.Max(Target_Count_Stack.Value, 0);

        Heal_Percent.Value = Mathf.Max(Heal_Percent.Value, 0);
        Heal_Percent_Stack.Value = Mathf.Max(Heal_Percent_Stack.Value, 0);

        Flat_Armor.Value = Mathf.Max(Flat_Armor.Value, 0);
        Flat_Armor_Stack.Value = Mathf.Max(Flat_Armor_Stack.Value, 0);

        Radius.Value = Mathf.Max(Radius.Value, 0);
        Radius_Stack.Value = Mathf.Max(Radius_Stack.Value, 0);

        return Item_Enabled.Value;
    }

    protected override void Initialize()
    {
        ItemDef = Value;
        ItemDef.requiredExpansion = SotAPlugin.ScalesAsclepiusExp;
    }
    protected override void LogDisplay()
    {
        ModelPanelParameters modelParam = PickupModelPrefab.AddComponent<ModelPanelParameters>();
        var foundMesh = PickupModelPrefab.transform.GetChild(0);

        if (!foundMesh) return;

        modelParam.focusPointTransform = foundMesh;
        modelParam.cameraPositionTransform = foundMesh;
        modelParam.minDistance = 0.05f * 25f;
        modelParam.maxDistance = 0.25f * 25f;
        modelParam.modelRotation = new Quaternion(0.0115291597f, -0.587752283f, 0.0455321521f, -0.807676435f);

        FloatingPointFix modelScale = PickupModelPrefab.AddComponent<FloatingPointFix>();
        modelScale.sizeModifier = 1f;
    }
}