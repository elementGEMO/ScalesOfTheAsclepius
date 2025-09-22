using RoR2;
using UnityEngine;
using BepInEx.Configuration;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
public class BubbleWrapItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<float> Heal_Percent;
    public static ConfigEntry<float> Heal_Percent_Stack;
    public static ConfigEntry<float> Debuff_Reduce;
    public static ConfigEntry<float> Debuff_Reduce_Stack;

    private static string VoidCorruptText = "Corrupts all Gauze Pads".Style(FontColor.cIsVoid) + ".";

    protected override string Name => "BubbleWrap";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.VoidTier1;
    protected override ItemTag[] Tags => [ItemTag.Healing];

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("bubbleWrapModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("bubbleIconRender");

    protected override string DisplayName => "Bubble Wrap";
    protected override string Description => FuseText(
        [
            string.Format("Reduce all " + "debuff ".Style(FontColor.cIsDamage) + "durations by " + "{0} ".Style(FontColor.cIsUtility) + "({1} per stack) ".Style(FontColor.cStack).OptText(Debuff_Reduce_Stack.Value != 0) + "second" + "s".OptText(Debuff_Reduce_Stack.Value != 0 || Debuff_Reduce.Value > 1) + ". ",
            RoundVal(Debuff_Reduce.Value), RoundVal(Debuff_Reduce_Stack.Value).SignVal()),

            string.Format("Heal ".Style(FontColor.cIsHealing) + "for " + "{0}% ".Style(FontColor.cIsHealing) + "({1}% per stack) ".Style(FontColor.cStack).OptText(Heal_Percent_Stack.Value != 0) + "of " + "maximum health ".Style(FontColor.cIsHealing) + "after a " + "debuff ".Style(FontColor.cIsDamage) + "ends. ",
            RoundVal(Heal_Percent.Value), RoundVal(Heal_Percent_Stack.Value).SignVal()),

            VoidCorruptText
        ]
    );
    protected override string PickupText => string.Format(
        "Reduce debuff durations by {0} second" + "s".OptText(Debuff_Reduce.Value > 1) + ". Heal after removing a debuff. " + VoidCorruptText,
        RoundVal(Debuff_Reduce.Value)
    );

    protected override bool IsEnabled()
    {
        Heal_Percent = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Percent Heal", 5f,
            "[ 5 = 5% | of Maximum Health Healing ]"
        );
        Heal_Percent_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Percent Heal Stack", 2.5f,
            "[ 2.5 = +2.5% | of Maximum Health Healing per Item Stack | 0 to Disable ]"
        );

        Debuff_Reduce = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Debuff Reduce", 1f,
            "[ 1 = 1s | Debuff Duration Reduced ]"
        );
        Debuff_Reduce_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Debuff Reduce Stack", 0f,
            "[ 1 = +1s | Debuff Duration Reduced per Item Stack | 0 to Disable ]"
        );

        Item_Enabled = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        Heal_Percent.Value = Mathf.Max(Heal_Percent.Value, 0);
        Heal_Percent_Stack.Value = Mathf.Max(Heal_Percent_Stack.Value, 0);

        Debuff_Reduce.Value = Mathf.Max(Debuff_Reduce.Value, 0);
        Debuff_Reduce_Stack.Value = Mathf.Max(Debuff_Reduce_Stack.Value, 0);

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