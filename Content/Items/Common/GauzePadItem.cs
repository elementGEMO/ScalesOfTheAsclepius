using RoR2;
using R2API;
using UnityEngine;
using BepInEx.Configuration;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class GauzePadItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<float> Heal_Amount;
    public static ConfigEntry<float> Heal_Amount_Stack;
    public static ConfigEntry<float> Duration;
    public static ConfigEntry<float> Duration_Stack;

    public static float Level_Scale = 0.2f; // Why I would let the normal user change regular basis level scaling

    protected override string Name => "GauzePad";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier1;
    protected override ItemTag[] Tags => [ItemTag.Healing];
    protected override bool IsRemovable => true;

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("gauzePadModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("gauzeIconRender");

    protected override string DisplayName => "Gauze Pad";
    protected override string Description => FuseText([
        string.Format("Increase base health regeneration ".Style(FontColor.cIsHealing) + "by " + "{0} hp/s ".Style(FontColor.cIsHealing) + "({1} hp/s per stack) ".Style(FontColor.cStack).OptText(Heal_Amount_Stack.Value != 0),
        RoundVal(Heal_Amount.Value), RoundVal(Heal_Amount_Stack.Value).SignVal()),

        string.Format("for " + "{0}s ".Style(FontColor.cIsUtility) + "({1}s per stack) ".Style(FontColor.cStack).OptText(Duration_Stack.Value != 0) + "when gaining a " + "debuff".Style(FontColor.cIsDamage) + ".",
        RoundVal(Duration.Value), RoundVal(Duration_Stack.Value).SignVal())
    ]);
    protected override string PickupText => "Rapidly heal when afflicted with a debuff.";

    protected override bool IsEnabled()
    {
        Heal_Amount = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Regeneration Amount", 6f,
            "[ 6 = +6 hp/s | Regeneration Amount ]"
        );
        Heal_Amount_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Regeneration Amount Stack", 6f,
            "[ 6 = +6 hp/s | Regeneration Amount per Item Stack | 0 to Disable ]"
        );

        Duration = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Effect Duration", 3f,
            "[ 3 = 3s | Duration for Regeneration ]"
        );
        Duration_Stack = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Effect Duration Stack", 0f,
            "[ 1 = +1s | Duration for Regeneration per Item Stack | 0 to Disable ]"
        );

        Item_Enabled = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        Heal_Amount.Value = Mathf.Max(Heal_Amount.Value, 0);
        Heal_Amount_Stack.Value = Mathf.Max(Heal_Amount_Stack.Value, 0);

        Duration.Value = Mathf.Max(Duration.Value, 0);
        Duration_Stack.Value = Mathf.Max(Duration_Stack.Value, 0);

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
    protected override ItemDisplayRuleDict ItemDisplay()
    {
        PickupModelPrefab.AddComponent<ItemDisplay>().rendererInfos = ItemDisplaySetup(PickupModelPrefab);
        ItemDisplayRuleDict baseDisplay = new();

        // Risk of Rain 2
        baseDisplay.Add("CommandoBody", new ItemDisplayRule
        {
            followerPrefab = PickupModelPrefab,
            ruleType = ItemDisplayRuleType.ParentedPrefab,

            childName = "Head",
            localPos = new Vector3(0.09018F, 0.33654F, 0.08826F),
            localAngles = new Vector3(359.2535F, 256.1467F, 31.56238F),
            localScale = new Vector3(0.25F, 0.25F, 0.25F)

        });

        return baseDisplay;
    }
}