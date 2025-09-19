using RoR2;
using R2API;
using System;
using R2API.Networking;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil;

namespace ScalesAsclepius;
public class BubbleWrapHooks
{
    //private static readonly string InternalName = "BubbleWrapHooks";
    public static bool ItemEnabled;

    private static EffectDef BounceEffect;

    public BubbleWrapHooks()
    {
        ItemEnabled = BubbleWrapItem.Item_Enabled.Value;

        if (ItemEnabled && GauzePadItem.Item_Enabled.Value)
        {
            CreateBubbleEffect();

            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.DotController.OnDotStackRemovedServer += DotController_OnDotStackRemovedServer;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
        }
    }
    private static void CreateBubbleEffect()
    {
        GameObject tempPrefab   = Addressables.LoadAsset<GameObject>("RoR2/DLC1/BearVoid/BearVoidProc.prefab").WaitForCompletion().InstantiateClone("BubbleWrapProc");
        EffectComponent effect  = tempPrefab.GetComponent<EffectComponent>();
        GameObject textRemoval  = tempPrefab.transform.FindChild("TextCamScaler").gameObject;

        effect.soundName = "";
        UnityEngine.Object.Destroy(textRemoval);

        foreach (Transform scale in tempPrefab.GetComponentInChildren<Transform>()) scale.localScale *= 0.5f;

        BounceEffect = new()
        {
            prefab = tempPrefab,
            prefabName = "BubbleWrapProc",
            prefabEffectComponent = effect,
        };

        ContentAddition.AddEffect(BounceEffect.prefab);
    }

    private void AddHealFromDebuff(CharacterBody self)
    {
        int itemCount = self.inventory.GetItemCount(BubbleWrapItem.ItemDef);
        if (itemCount > 0)
        {
            float itemScale = BubbleWrapItem.Heal_Percent.Value * BubbleWrapItem.Item_Scale.Value * (itemCount - 1);
            float totalHeal = BubbleWrapItem.Heal_Percent.Value + itemScale;

            Util.PlaySound("Play_gup_step", self.gameObject);
            self.GetComponent<HealthComponent>().HealFraction(totalHeal, new ProcChainMask());
            EffectManager.SpawnEffect(BounceEffect.prefab, new EffectData()
            {
                rootObject = self.gameObject,
                rotation = Quaternion.identity
            }, true);
        }
    }

    private void DotController_OnDotStackRemovedServer(On.RoR2.DotController.orig_OnDotStackRemovedServer orig, DotController self, DotController.DotStack dotStack)
    {
        DotController.DotDef dotDef = dotStack.dotDef;
        CharacterBody victim        = self.victimBody;
        int dotCount                = victim.GetBuffCount(dotDef.associatedBuff);

        if (victim?.inventory && dotCount <= 1) AddHealFromDebuff(self.victimBody);

        orig(self, dotStack);
    }

    private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
    {
        orig(self, buffDef);

        if (self?.inventory && buffDef)
        {
            bool isHarmful = (buffDef.isDebuff || buffDef.isCooldown) && !buffDef.isDOT;
            if (isHarmful) AddHealFromDebuff(self);
        }
    }

    private void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
    {
        if (self?.inventory)
        {
            int itemCount   = self.inventory.GetItemCount(BubbleWrapItem.ItemDef);
            bool isHarmful  = buffDef.isDebuff || buffDef.isDOT || buffDef.isCooldown;

            if (itemCount > 0 && isHarmful) duration = Math.Max(0f, duration - BubbleWrapItem.Debuff_Reduce.Value);
        }

        orig(self, buffDef, duration);
    }
}