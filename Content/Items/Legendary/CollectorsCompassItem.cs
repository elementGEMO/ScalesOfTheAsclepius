using RoR2;
using R2API;
using UnityEngine;
using BepInEx.Configuration;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class CollectorsCompassItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    public static ConfigEntry<int> InteractAmount;

    protected override string Name => "CollectorsCompass";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier3;
    protected override ItemTag[] Tags => [ItemTag.Healing];
    protected override bool IsRemovable => true;

    private static string ConfigName => "Collectors Compass"; // Since the regular DisplayName errors with an apostrophe
    protected override string DisplayName => "Collector\'s Compass";

    protected override bool IsEnabled()
    {
        InteractAmount = SotAPlugin.Instance.Config.Bind(
            ConfigName + " - Item",
            "Interact Amount", 3,
            "[ 3 = 3 | Interactables Found to Proc Item ]"
        );

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
    }
}