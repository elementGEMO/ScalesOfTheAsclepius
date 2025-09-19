using BepInEx.Configuration;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class IVBagItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Heal_Percent;
    public static ConfigEntry<float> Item_Scale;
    public static ConfigEntry<float> Flat_Armor;
    public static ConfigEntry<float> Radius;

    protected override string Name => "IVBag";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier2;
    protected override ItemTag[] Tags => [ItemTag.Healing, ItemTag.Utility];
    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("IVBagModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("IVBagRender");

    protected override string DisplayName => "IV Bag";
    protected override string Description => string.Format(
        "Tether ".Style(FontColor.cIsHealing) + "to the nearest ally within " + "{0}m".Style(FontColor.cIsUtility) +
        ". While " + "tethered".Style(FontColor.cIsHealing) + ", share " + "{1}% ".Style(FontColor.cIsHealing) + "({2}% per stack) ".Style(FontColor.cStack) + "of " + "all healing".Style(FontColor.cIsHealing) +
        ", or " + "increase armor ".Style(FontColor.cIsHealing) + "by " + "{3} ".Style(FontColor.cIsHealing) + "when no healing is shared.",
        RoundVal(Radius.Value), RoundVal(Heal_Percent.Value * 100f), RoundVal(Heal_Percent.Value * Item_Scale.Value * 100f).SignVal(), RoundVal(Flat_Armor.Value)
    );
    protected override string PickupText => "Tether to a nearby ally, sharing health or gaining armor." ;

    protected override bool IsEnabled()
    {
        Heal_Percent = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Healing Shared", 0.15f,
            "[ 0.15 = 15% | Healing Shared to Tethered Ally ]"
        );
        Item_Scale = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Item Scaling", 0.33f,
            "[ 0.33 = 33% | Healing Shared per Item Stack Increase ]"
        );
        Flat_Armor = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Armor", 25f,
            "[ 25 = 25 | Flat Armor Increase when Not Sharing ]"
        );
        Radius = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Radius", 20f,
            "[ 20 = 20m | Radius to Tether ]"
        );
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