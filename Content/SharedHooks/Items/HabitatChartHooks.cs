using RoR2;
using RoR2.Items;
using R2API;
using System;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2BepInExPack.GameAssetPathsBetter;
using System.Xml.Linq;

namespace ScalesAsclepius;
public class HabitatChartHooks
{
    //private static readonly string InternalName = "HabitatChartHooks";
    public static bool ItemEnabled;

    public HabitatChartHooks()
    {
        ItemEnabled = HabitatChartItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            Inventory.onInventoryChangedGlobal += Inventory_onInventoryChangedGlobal;
            On.RoR2.CharacterMaster.OnServerStageBegin += CharacterMaster_OnServerStageBegin;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }
    }

    private void Inventory_onInventoryChangedGlobal(Inventory inventory)
    {
        CharacterMaster charMaster = inventory.GetComponent<CharacterMaster>();
        CharacterBody body = charMaster ? charMaster.GetBody() : null;

        if (body)
        {
            int itemCount = inventory.GetItemCount(HabitatChartItem.ItemDef);
            int effectCount = inventory.GetItemCount(HabitatChartCountItem.ItemDef);
        }
    }

    private void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
    {
        if (self?.inventory)
        {
            int itemCount = self.inventory.GetItemCount(HabitatChartItem.ItemDef);
            if (itemCount > 0) self.inventory.GiveItem(HabitatChartCountItem.ItemDef);
        }
    }

    private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (sender?.inventory)
        {
            int itemCount = sender.inventory.GetItemCount(HabitatChartCountItem.ItemDef);
            
            if (itemCount > 0)
            {
                float undecidedHealth = 1000f;
                args.baseHealthAdd += undecidedHealth * itemCount;
            }
        }
    }
}