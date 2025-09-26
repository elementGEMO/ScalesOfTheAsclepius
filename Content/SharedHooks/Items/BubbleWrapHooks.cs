using RoR2;
using R2API;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;

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
        GameObject tempPrefab   = Addressables.LoadAsset<GameObject>(RoR2_DLC1_BearVoid.BearVoidProc_prefab).WaitForCompletion().InstantiateClone("BubbleWrapProc");
        EffectComponent effect  = tempPrefab.GetComponent<EffectComponent>();
        GameObject textRemoval  = tempPrefab.transform.Find("TextCamScaler").gameObject;

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
        int itemCount = self?.inventory ? self.inventory.GetItemCount(BubbleWrapItem.ItemDef) : 0;

        if (itemCount > 0)
        {
            ProcChainMask healMask  = new();
            float healPercent       = BubbleWrapItem.Heal_Percent.Value;

            if (BubbleWrapItem.Heal_Percent_Stack.Value > 0)
            {
                float itemScale = BubbleWrapItem.Heal_Percent_Stack.Value * (itemCount - 1);
                healPercent += itemScale;
            }

            self.GetComponent<HealthComponent>().HealFraction(healPercent / 100f, healMask);
            Util.PlaySound("Play_gup_step", self.gameObject);

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
            int itemCount       = self.inventory.GetItemCount(BubbleWrapItem.ItemDef);
            bool isHarmful      = buffDef.isDebuff || buffDef.isDOT || buffDef.isCooldown;
            float timeReduce    = BubbleWrapItem.Debuff_Reduce.Value;

            if (itemCount > 0 && isHarmful)
            {
                if (BubbleWrapItem.Debuff_Reduce_Stack.Value > 0)
                {
                    float itemScale = BubbleWrapItem.Debuff_Reduce_Stack.Value * (itemCount - 1);
                    timeReduce += itemScale;
                }

                duration = Math.Max(0f, duration - timeReduce);
            }
        }

        orig(self, buffDef, duration);
    }
}