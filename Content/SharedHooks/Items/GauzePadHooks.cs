using RoR2;
using R2API;
using System;
using R2API.Networking;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ScalesAsclepius;
public class GauzePadHooks
{
    //private static readonly string InternalName = "GauzePadHooks";
    public static bool ItemEnabled;

    private EffectDef SplashEffect;

    public GauzePadHooks()
    {
        ItemEnabled = GauzePadItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            CreateSplashEffect();

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.DotController.OnDotStackAddedServer += DotController_OnDotStackAddedServer;
            On.RoR2.CharacterBody.AddBuff_BuffIndex += CharacterBody_AddBuff_BuffIndex;
        }
    }

    private void CreateSplashEffect()
    {
        GameObject tempPrefab = Addressables.LoadAsset<GameObject>("RoR2/Base/Phasing/ProcStealthkit.prefab").WaitForCompletion().InstantiateClone("GauzePadProc");
        EffectComponent effect = tempPrefab.GetComponent<EffectComponent>();

        foreach (Transform transform in tempPrefab.GetComponentsInChildren<Transform>()) transform.localScale = Vector3.one * 0.5f;

        SplashEffect = new()
        {
            prefab = tempPrefab,
            prefabName = "GauzePadProc",
            prefabEffectComponent = effect
        };

        ContentAddition.AddEffect(SplashEffect.prefab);
    }

    private void AddHealFromDebuff(CharacterBody self)
    {
        int itemCount = self.inventory.GetItemCount(GauzePadItem.ItemDef);
        if (itemCount > 0)
        {
            float regenDuration = GauzePadItem.Duration.Value;

            if (GauzePadItem.Duration_Stack.Value > 0)
            {
                float itemScale = GauzePadItem.Duration_Stack.Value * (itemCount - 1);
                regenDuration += itemScale;
            }

            EffectManager.SimpleEffect(SplashEffect.prefab, self.corePosition, self.transform.rotation, true);
            self.ApplyBuff(HealFromDebuff.BuffDef.buffIndex, 1, regenDuration);
            Util.PlaySound("Play_scav_backpack_open", self.gameObject);
        }
    }

    private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (sender?.inventory)
        {
            int itemCount = sender.inventory.GetItemCount(GauzePadItem.ItemDef);
            bool hasBuff = sender.HasBuff(HealFromDebuff.BuffDef);

            if (hasBuff && itemCount > 0)
            {
                /*
                float adjustedLevel     = sender.level - 1f;
                float scaleMultiplier   = 1f + GauzePadItem.Level_Scale.Value * adjustedLevel;
                float itemScale         = GauzePadItem.Heal_Amount.Value * GauzePadItem.Item_Scale.Value * (itemCount - 1);
                float totalRegen        = (GauzePadItem.Heal_Amount.Value + itemScale) * scaleMultiplier;
                */

                float regenAmount   = GauzePadItem.Heal_Amount.Value;
                float regenLevel    = regenAmount * GauzePadItem.Level_Scale;

                if (GauzePadItem.Heal_Amount_Stack.Value > 0)
                {
                    float itemScale = GauzePadItem.Heal_Amount_Stack.Value * (itemCount - 1);
                    regenAmount += itemScale;
                }

                args.baseRegenAdd += regenAmount;
                args.levelRegenAdd += regenLevel;
            }
        }
    }

    private void DotController_OnDotStackAddedServer(On.RoR2.DotController.orig_OnDotStackAddedServer orig, DotController self, DotController.DotStack dotStack)
    {
        orig(self, dotStack);

        CharacterBody victim = self.victimBody;
        if (dotStack.dotDef.associatedBuff != null && victim?.inventory) AddHealFromDebuff(victim);
    }

    private void CharacterBody_AddBuff_BuffIndex(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
    {
        orig(self, buffType);

        BuffDef foundBuff = BuffCatalog.GetBuffDef(buffType);
        if (foundBuff && self?.inventory)
        {
            bool isNegative = foundBuff.isDebuff && !foundBuff.isDOT;
            if (isNegative) AddHealFromDebuff(self);
        }
    }
}