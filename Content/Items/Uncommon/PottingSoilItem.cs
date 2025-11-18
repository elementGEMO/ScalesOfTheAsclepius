using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class PottingSoilItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    protected override string Name => "PottingSoil";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier2;
    protected override ItemTag[] Tags => [ItemTag.Healing, ItemTag.Utility, ItemTag.CanBeTemporary];
    protected override bool IsRemovable => true;

    protected override string DisplayName => "Potting Soil";

    protected override bool IsEnabled()
    {
        Item_Enabled = SotAPlugin.Instance.Config.Bind(
            DisplayName + " - Item", "Enable Item", false,
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