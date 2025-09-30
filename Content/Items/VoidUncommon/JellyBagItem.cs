using RoR2;
using UnityEngine;
using BepInEx.Configuration;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
public class JellyBagItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<float> Heal_Percent;
    public static ConfigEntry<float> Heal_Percent_Stack;
    public static ConfigEntry<float> Threshold;
    public static ConfigEntry<float> Threshold_Stack;
    public static ConfigEntry<float> Radius;
    public static ConfigEntry<float> Radius_Stack;

    private static readonly string VoidCorruptText = "Corrupts all IV Bags".Style(FontColor.cIsVoid) + ".";

    protected override string Name => "JellyBag";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.VoidTier2;
    protected override ItemTag[] Tags => [ItemTag.Healing, ItemTag.Utility];

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("jellyBagModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("jellyBagRender");

    protected override string DisplayName => "Jelly Bag";
    protected override string Description => FuseText([
        string.Format("Heal ".Style(FontColor.cIsHealing) + "for " + "{0}% ".Style(FontColor.cIsHealing) + "({1} per stack) ".Style(FontColor.cStack).OptText(Heal_Percent_Stack.Value != 0) + "of damage when nearby allies ",
        RoundVal(Heal_Percent.Value), RoundVal(Heal_Percent_Stack.Value).SignVal()),

        string.Format("within " + "{0}m ".Style(FontColor.cIsUtility) + "({1}m per stack) ".Style(FontColor.cStack).OptText(Radius_Stack.Value != 0) + "take damage for ",
        RoundVal(Radius.Value), RoundVal(Radius_Stack.Value).SignVal()),

        string.Format("at least " + "{0}% ".Style(FontColor.cIsHealth) + "({1}% per stack) ".Style(FontColor.cStack).OptText(Threshold_Stack.Value != 0) + "health".Style(FontColor.cIsHealth) + ". ",
        RoundVal(Threshold.Value), RoundVal(Threshold_Stack.Value).SignVal()),

        VoidCorruptText
    ]);
    protected override string PickupText => "Heal when nearby allies take damage. " + VoidCorruptText;

    protected override bool IsEnabled()
    {
        Heal_Percent = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Percent Heal", 50f,
            "[ 50 = 50% | Damage to Healing ]"
        );
        Heal_Percent_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Percent Heal Stack", 0f,
            "[ 25 = +25% | Damage to Healing per Item Stack | 0 to Disable ]"
        );

        Threshold = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Damage Threshold", 50f,
            "[ 50 = 50% | Damaged Required to Proc ]"
        );
        Threshold_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Damage Threshold Stack", -50f,
            "[ -50 = -50% | Damaged Required to Proc per Item Stack | RECIPROCAL | 0 to Disable ]"
        );

        Radius = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Radius", 30f,
            "[ 30 = 30 | Meter Radius ]"
        );
        Radius_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Radius Stack", 0f,
            "[ 5 = +5 | Meter Radius per Item Stack | 0 to Disable ]"
        );

        Heal_Percent.Value = Mathf.Max(Heal_Percent.Value, 0);
        Heal_Percent_Stack.Value = Mathf.Max(Heal_Percent_Stack.Value, 0);

        Threshold.Value = Mathf.Max(Threshold.Value, 0);
        Threshold_Stack.Value = Mathf.Min(Threshold_Stack.Value, 0);

        Radius.Value = Mathf.Max(Radius.Value, 0);
        Radius_Stack.Value = Mathf.Max(Radius_Stack.Value, 0);

        Item_Enabled = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        return Item_Enabled.Value;
    }

    protected override void Initialize()
    {
        ItemDef = Value;
        ItemDef.requiredExpansion = SotAPlugin.ScalesAsclepiusExp;

        GameObject displayPrefab    = SotAPlugin.Bundle.LoadAsset<GameObject>("jellyBagDisplay");
        DynamicBone dynamic         = displayPrefab.AddComponent<DynamicBone>();

        dynamic.m_Root = displayPrefab.transform.root;
    }
    protected override void LogDisplay()
    {
        ModelPanelParameters modelParam = PickupModelPrefab.AddComponent<ModelPanelParameters>();
        var foundMesh = PickupModelPrefab.transform.Find("mdlJellyBag");

        if (!foundMesh) return;

        modelParam.focusPointTransform = foundMesh;
        modelParam.cameraPositionTransform = foundMesh;
        modelParam.minDistance = 1f;
        modelParam.maxDistance = 0.25f * 25f;
        modelParam.modelRotation = new Quaternion(0.0115291597f, -0.587752283f, 0.0455321521f, -0.807676435f);
    }
}