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
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }
    }

    private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (sender?.inventory)
        {
            int itemCount = sender.inventory.GetItemCount(HabitatChartItem.ItemDef);
            
            if (itemCount > 0)
            {
                float undecidedHealth = 1000f;

                args.baseHealthAdd += undecidedHealth;
            }
        }
    }
}