using RoR2;
using R2API;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class CollectorsCompassItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<int> InteractAmount;
    public static ConfigEntry<float> Health_Multiply;
    public static ConfigEntry<float> Health_Multiply_Stack;
    public static ConfigEntry<float> Speed_Multiply;
    public static ConfigEntry<float> Speed_Multiply_Stack;

    protected override string Name => "CollectorsCompass";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier3;
    protected override ItemTag[] Tags => [ItemTag.Healing, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CanBeTemporary];
    protected override bool IsRemovable => true;

    protected override GameObject PickupModelPrefab => SotAPlugin.Bundle.LoadAsset<GameObject>("collectorsCompassModel");
    protected override Sprite PickupIconSprite => SotAPlugin.Bundle.LoadAsset<Sprite>("collectorsCompassRender");

    private static string ConfigName => "Collectors Compass"; // Since the regular DisplayName errors with an apostrophe
    protected override string DisplayName => "Collector\'s Compass";
    protected override string Description => FuseText([
        "At the " + "start of each stage".Style(FontColor.cIsUtility) + ", " + "mark ".Style(FontColor.cIsHealth) + "a random interactable. ",
        "Using a " + "marked ".Style(FontColor.cIsHealth) + "interactable " + "marks ".Style(FontColor.cIsHealth) + "another. ",

        string.Format("Claiming " + "{0} marks ".Style(FontColor.cIsHealth) + "reveals".Style(FontColor.cIsUtility) + " all remaining interactables ",
        InteractAmount.Value),

        string.Format("and increases all allies " + "maximum health ".Style(FontColor.cIsHealing) + "by " + "{0}% ".Style(FontColor.cIsHealing) + "({1}% per stack) ".OptText(Health_Multiply_Stack.Value != 0).Style(FontColor.cStack),
        RoundVal(Health_Multiply.Value), RoundVal(Health_Multiply_Stack.Value).SignVal()),

        string.Format("and " + "movement speed ".Style(FontColor.cIsUtility) + "by " + "{0}%".Style(FontColor.cIsUtility) + " ({1}% per stack)".OptText(Speed_Multiply_Stack.Value != 0).Style(FontColor.cStack) + ".",
        RoundVal(Speed_Multiply.Value), RoundVal(Speed_Multiply_Stack.Value).SignVal())
    ]);
    protected override string PickupText => string.Format("Marks interactables, revealing all interactables and increases maximum health and speed after claiming {0}.", InteractAmount.Value);

    protected override bool IsEnabled()
    {
        InteractAmount = SotAPlugin.Instance.Config.Bind(
            ConfigName + " - Item",
            "Interact Amount", 3,
            "[ 3 = 3 | Interactables Found to Proc Item ]"
        ).PostConfig(PluginConfig.MathProcess.Max, 1);

        Health_Multiply = SotAPlugin.Instance.Config.Bind(
            ConfigName + " - Item",
            "Health Multiply", 100f,
            "[ 100 = 100% | Max Health Increase on Buff ]"
        ).PostConfig(PluginConfig.MathProcess.Max, 0);
        Health_Multiply_Stack = SotAPlugin.Instance.Config.Bind(
            ConfigName + " - Item",
            "Health Multiply Stack", 50f,
            "[ 50 = +50% | Max Health Increase on Buff per Item Stack ]"
        ).PostConfig(PluginConfig.MathProcess.Max, 0);

        Speed_Multiply = SotAPlugin.Instance.Config.Bind(
            ConfigName + " - Item",
            "Speed Multiply", 15f,
            "[ 15 = 15% | Speed Increase on Buff ]"
        ).PostConfig(PluginConfig.MathProcess.Max, 0);
        Speed_Multiply_Stack = SotAPlugin.Instance.Config.Bind(
            ConfigName + " - Item",
            "Speed Multiply Stack", 0f,
            "[ 15 = +15% | Speed Increase on Buff per Item Stack ]"
        ).PostConfig(PluginConfig.MathProcess.Max, 0);

        Item_Enabled = SotAPlugin.Instance.Config.Bind(
            ConfigName + " - Item", "Enable Item", true,
            "[ True = Enabled | False = Disabled ]"
        );

        return Item_Enabled.Value;
    }

    protected override void Initialize()
    {
        ItemDef = Value;
        ItemDef.requiredExpansion = SotAPlugin.ScalesAsclepiusExp;

        MeshRenderer itemModel = ItemDef.pickupModelPrefab.transform.Find("mdlCompass").GetComponent<MeshRenderer>();
        Texture banditRamp = Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common_ColorRamps.texRampBandit_png).WaitForCompletion();
        itemModel.sharedMaterial.SetTexture("_FresnelRamp", banditRamp);
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
    }
}