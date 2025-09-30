using RoR2;

namespace ScalesAsclepius;
public class HabitatChartCountItem : ItemBase
{
    protected override string Name => "HabitatChartCount";
    public static ItemDef ItemDef;

    protected override CombinedItemTier Tier => ItemTier.NoTier;
    protected override ItemTag[] Tags => [
        ItemTag.BrotherBlacklist,
        ItemTag.CannotCopy,
        ItemTag.CannotDuplicate,
        ItemTag.CannotSteal,
        ItemTag.WorldUnique
    ];
    protected override bool IsHidden => true;

    protected override bool IsEnabled() => HabitatChartItem.Item_Enabled.Value;

    protected override void Initialize()
    {
        ItemDef = Value;
        ItemDef.requiredExpansion = SotAPlugin.ScalesAsclepiusExp;
    }
}