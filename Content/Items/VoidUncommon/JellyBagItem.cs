using RoR2;
using UnityEngine;
using BepInEx.Configuration;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
public class JellyBagItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<float> Heal_Flat;
    public static ConfigEntry<float> Heal_Flat_Item;
    public static ConfigEntry<float> Heal_Percent;
    public static ConfigEntry<float> Heal_Percent_Item;
    public static ConfigEntry<float> Damage_Percent;
    public static ConfigEntry<float> Damage_Percent_Stack;
    public static ConfigEntry<int> Damage_Reduce;
    public static ConfigEntry<int> Damage_Reduce_Stack;
    public static ConfigEntry<int> Cooldown;
    public static ConfigEntry<int> Cooldown_Stack;

    private static readonly string VoidCorruptText = "Corrupts all IV Bags".Style(FontColor.cIsVoid) + ".";

    protected override string Name => "JellyBag";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.VoidTier2;
    protected override ItemTag[] Tags => [ItemTag.Healing, ItemTag.Utility];

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("jellyBagModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("jellyBagRender");

    protected override string DisplayName => "Jelly Bag";
    protected override string Description => FuseText([
        string.Format("Heal ".Style(FontColor.cIsHealing) + "for " + "{0} ".Style(FontColor.cIsHealing) + "({1} per stack) ".Style(FontColor.cStack),
        RoundVal(0), RoundVal(0)),

        string.Format("plus an additional " + "{0}% ".Style(FontColor.cIsHealing) + "({1}% per stack) ".Style(FontColor.cStack) + "of " + "maximum health ".Style(FontColor.cIsHealing),
        RoundVal(0), RoundVal(0)),

        string.Format("when a nearby ally takes damage for " + "{0}% health ".Style(FontColor.cIsHealth) + "({1}% per stack) ".Style(FontColor.cStack) + "or more. ",
        RoundVal(0), RoundVal(0)),

        string.Format("Reduce " + "incoming damage ".Style(FontColor.cIsDamage) + "by " + "{0}".Style(FontColor.cIsDamage) + "(+{1} per stack)".Style(FontColor.cStack) + ", ",
        RoundVal(0), RoundVal(0)),

        string.Format("recharging every " + "{0} ".Style(FontColor.cIsUtility) + "({1}% per stack) ".Style(FontColor.cStack) + " seconds.",
        RoundVal(0), RoundVal(0))
    ]);
    protected override string PickupText => "...";

    protected override bool IsEnabled()
    {
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