using RoR2;

namespace ScalesAsclepius;
public struct CombinedItemTier
{
    public ItemTierDef ItemTierDef;
    public ItemTier ItemTier;

    public static implicit operator ItemTierDef(CombinedItemTier self)  => self.ItemTierDef;
    public static implicit operator ItemTier(CombinedItemTier self)     => self.ItemTier;

    public static implicit operator CombinedItemTier(ItemTier itemTier)
    {
        return new CombinedItemTier
        {
            ItemTier = itemTier
        };
    }

    public static implicit operator CombinedItemTier(ItemTierDef itemTierDef)
    {
        return new CombinedItemTier
        {
            ItemTierDef = itemTierDef
        };
    }
}