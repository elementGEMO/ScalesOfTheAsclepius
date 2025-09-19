using BepInEx.Configuration;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;
using HG;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
public class BubbleWrapItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Heal_Percent;
    public static ConfigEntry<float> Item_Scale;
    public static ConfigEntry<float> Debuff_Reduce;

    private static string VoidCorruptText = "Corrupts all Gauze Pads".Style(FontColor.cIsVoid) + ".";

    protected override string Name => "BubbleWrap";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.VoidTier1;
    protected override ItemTag[] Tags => [ItemTag.Healing];

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("bubbleWrapModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("bubbleIconRender");

    protected override string DisplayName => "Bubble Wrap";
    protected override string Description => string.Format(
        "Reduce all " + "debuff ".Style(FontColor.cIsDamage) + "durations by " + "{0} ".Style(FontColor.cIsUtility) + "second" + "s".OptText(Debuff_Reduce.Value > 1 || Debuff_Reduce.Value < -1) +". " +
        "Heal ".Style(FontColor.cIsHealing) + "for " + "{1}% ".Style(FontColor.cIsHealing) + "({2}% per stack) ".Style(FontColor.cStack) + "of " + "maximum health ".Style(FontColor.cIsHealing) + "after a " + "debuff ".Style(FontColor.cIsDamage) + "ends. " +
        VoidCorruptText, RoundVal(Debuff_Reduce.Value), RoundVal(Heal_Percent.Value * 100f), RoundVal(Heal_Percent.Value * 100f * Item_Scale.Value).SignVal()
    );
    protected override string PickupText => string.Format(
        "Reduce debuff durations by {0} second. Heal after removing a debuff. " + VoidCorruptText,
        RoundVal(Debuff_Reduce.Value)
    );

    protected override bool IsEnabled()
    {
        Heal_Percent = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Percent Heal", 0.05f,
            "[ 0.05 = 5% | of Maximum Health Heal ]"
        );
        Item_Scale = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Item Scaling", 0.5f,
            "[ 0.5 = 50% | Heal per Item Stack Increase ]"
        );
        Debuff_Reduce = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Debuff Reduce", 1f,
            "[ 1 = 1s | Debuff Duration Reduced ]"
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