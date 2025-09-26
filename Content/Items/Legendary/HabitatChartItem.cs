using RoR2;
using R2API;
using UnityEngine;
using BepInEx.Configuration;

namespace ScalesAsclepius;
using static SAColors;
using static SAUtils;
using static SAREnderHelpers;
public class HabitatChartItem : ItemBase
{
    public static ConfigEntry<bool> Item_Enabled;

    protected override string Name => "HabitatChart";
    public static ItemDef ItemDef;
    protected override CombinedItemTier Tier => ItemTier.Tier3;
    protected override ItemTag[] Tags => [ItemTag.Healing];
    protected override bool IsRemovable => true;

    protected override string DisplayName => "Habitat Chart";

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
}