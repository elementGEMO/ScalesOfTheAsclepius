using BepInEx.Configuration;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class GauzePadItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;
    public static ConfigEntry<float> Heal_Amount;
    public static ConfigEntry<float> Level_Scale;
    public static ConfigEntry<float> Item_Scale;
    public static ConfigEntry<float> Duration;

    protected override string Name => "GauzePad";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier1;
    protected override ItemTag[] Tags => [ItemTag.Healing];

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("gauzePadModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("gauzeIconRender");
    protected override string DisplayName => "Gauze Pad";
    protected override string Description => string.Format(
        "Increase base health regeneration".Style(FontColor.cIsHealing) + " by" + " {0} hp/s".Style(FontColor.cIsHealing) + " ({1} hp/s per stack)".Style(FontColor.cStack) + " for" + " {2}s".Style(FontColor.cIsUtility) + " when gaining a " + "debuff".Style(FontColor.cIsDamage) + ".",
        RoundVal(Heal_Amount.Value).SignVal(), RoundVal(Heal_Amount.Value * Item_Scale.Value).SignVal(), RoundVal(Duration.Value)
    );
    protected override string PickupText => "Rapidly heal when afflicted with a debuff.";

    protected override bool IsEnabled()
    {
        Heal_Amount = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Regeneration Amount", 6f,
            "[ 5 = +5 hp/s | Base Amount Healed, and Increase per Stack ]"
        );
        Item_Scale = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Item Scaling", 1f,
            "[ 1 = 100% | Regeneration per Item Stack based on Regeneration Amount ]"
        );
        Level_Scale = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Level Scaling", 0.2f,
            "[ 0.2 = +20% | Increase per Level for Scaling ]"
        );
        Duration = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item",
            "Effect Duration", 3f,
            "[ 3 = 3s | Duration on the Regeneration Effect ]"
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