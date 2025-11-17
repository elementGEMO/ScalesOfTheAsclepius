using RoR2;
using RoR2.Orbs;
using R2API;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;
using System.Collections.Generic;
using UnityEngine.Networking;
using static UnityEngine.UI.GridLayoutGroup;

namespace ScalesAsclepius;
public class CollectorsCompassHooks
{
    //private static readonly string InternalName = "HabitatChartHooks";
    public static bool ItemEnabled;
    public static GameObject HealthPopUp;
    public static GameObject ArrowPrefab;
    public static GameObject RoR2Indicator;
    public static int ActivateCount;

    public CollectorsCompassHooks()
    {
        ItemEnabled = CollectorsCompassItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            ActivateCount = 0;

            CreateIcon();
            CreateArrows();
            SetUpBarrel();

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.BarrelInteraction.OnInteractionBegin += BarrelInteraction_OnInteractionBegin;
        }
    }

    private void CreateIcon()
    {
        GameObject tempPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_ShrineChance.ShrineChance_prefab).WaitForCompletion().transform.Find("Symbol").gameObject.InstantiateClone("CollectorsCompassMark");
        Texture replacedCloud = Addressables.LoadAssetAsync<Texture>(RoR2_Base_Common.texCloudWaterFoam2_psd).WaitForCompletion();
        MeshRenderer meshRender = tempPrefab.GetComponent<MeshRenderer>();
        Material replaceMat = new(meshRender.sharedMaterial);

        tempPrefab.AddComponent<NetworkIdentity>();
        tempPrefab.transform.localScale = Vector3.one * 12f;
        replaceMat.mainTexture = SotAPlugin.Bundle.LoadAsset<Sprite>("compassMarkIcon").texture;
        replaceMat.SetTexture("_Cloud1Tex", replacedCloud);
        replaceMat.SetColor("_TintColor", new Color(1, 0, 0, 0.745f));
        replaceMat.SetFloat("_AlphaBoost", 0.35f);
        replaceMat.SetFloat("_AlphaBias", 0.4f);
        replaceMat.SetFloat("_DistortionStrength", 0.05f);
        meshRender.sharedMaterial = replaceMat;

        HealthPopUp = tempPrefab;
    }
    private void CreateArrows()
    {
        GameObject tempPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Common.BossPositionIndicator_prefab).WaitForCompletion().InstantiateClone("CompassPositionIndicator");
        GameObject RoR2Prefab = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Common.PoiPositionIndicator_prefab).WaitForCompletion();

        ArrowPrefab = tempPrefab;
        RoR2Indicator = RoR2Prefab;
    }
    private void SetUpBarrel()
    {
        GameObject barrelPrefab = Addressables.LoadAssetAsync<GameObject>(RoR2_Base_Barrel1.Barrel1_prefab).WaitForCompletion();
        barrelPrefab.AddComponent<CompassList>();
    }

    private GameObject MarkInteract()
    {
        if (SceneInfo.instance.countsAsStage)
        {
            CompassList interactable = null;
            int searchFailSafe = 0;

            while (!interactable && searchFailSafe < Math.Pow(CompassList.AllInteractables.Count, 2))
            {
                int arraySize               = CompassList.AllInteractables.Count;
                int randIndex               = UnityEngine.Random.RandomRange(0, arraySize - 1);
                CompassList tempInteract    = CompassList.AllInteractables[randIndex];

                if (tempInteract)
                {
                    PurchaseInteraction purchase    = tempInteract.gameObject.GetComponent<PurchaseInteraction>();
                    BarrelInteraction barrel        = tempInteract.gameObject.GetComponent<BarrelInteraction>();

                    if (purchase && purchase.available) interactable = tempInteract;
                    else if (barrel && !barrel.opened) interactable = tempInteract;
                    else tempInteract.RemoveFromList();
                }
                searchFailSafe++;
            }

            if (interactable)
            {
                GameObject interactModel    = interactable.gameObject;
                GameObject markEffect       = UnityEngine.Object.Instantiate(HealthPopUp, interactModel.transform.position, Quaternion.identity);
                Highlight outlineEffect     = interactable.gameObject.GetComponent<Highlight>();

                markEffect.transform.position += new Vector3(0, 4, 0);
                outlineEffect.highlightColor = Highlight.HighlightColor.custom;
                outlineEffect.CustomColor = new Color(1, 0, 0);

                if (interactModel.GetComponent<ShopTerminalBehavior>()?.serverMultiShopController)
                {
                    MultiShopController multiShop = interactModel.GetComponent<ShopTerminalBehavior>().serverMultiShopController;
                    foreach (GameObject terminal in multiShop.terminalGameObjects)
                    {
                        terminal.GetComponent<Highlight>().highlightColor = Highlight.HighlightColor.custom;
                        terminal.GetComponent<Highlight>().CustomColor = new Color(1, 0, 0);
                    }
                }

                interactable.MarkEffect = markEffect;
                interactable.IsMarked = true;

                return interactModel;
            }
        }

        return null;
    }
    private void ReplaceMark(CompassList component, GameObject interactable)
    {
        ActivateCount++;

        Util.PlaySound("Play_env_hiddenLab_laptop_sequence_lock", interactable);

        component.IsMarked = false;
        interactable.GetComponent<Highlight>().highlightColor = Highlight.HighlightColor.interactive;
        interactable.GetComponent<Highlight>().isOn = false;
        UnityEngine.Object.Destroy(component.MarkEffect);
        UnityEngine.Object.Destroy(component.PosIndicator);

        if (ActivateCount >= CollectorsCompassItem.InteractAmount.Value)
        {
            var allyListReadOnly                = TeamComponent.GetTeamMembers(TeamIndex.Player);
            List<CharacterBody> playerAllies    = [];

            foreach (TeamComponent team in allyListReadOnly)
            {
                CharacterBody ally                      = team.body;
                PlayerCharacterMasterController player  = ally.master ? ally.master.playerCharacterMasterController : null;

                if (player) playerAllies.Add(ally);
            }

            foreach (CharacterBody ally in playerAllies)
            {
                if (ally) ally.AddBuff(CompassFoundBuff.BuffDef);
            }

            foreach (CompassList interact in CompassList.AllInteractables)
            {
                GameObject interactObject = interact ? interact.gameObject : null;
                if (interactObject) interactObject.AddComponent<PermanentReveal>();
            }

            Util.PlaySound("Play_UI_rustedLockbox_open", interactable);

            return;
        }

        GameObject newMark = MarkInteract();

        if (newMark)
        {
            GameObject indicator = UnityEngine.Object.Instantiate(ArrowPrefab, newMark.transform);
            indicator.GetComponent<PositionIndicator>().targetTransform = newMark.transform;
            newMark.GetComponent<CompassList>().PosIndicator = indicator;
        }
    }

    private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (sender.HasBuff(CompassFoundBuff.BuffDef))
        {
            int itemCount       = Util.GetItemCountForTeam(sender.teamComponent.teamIndex, CollectorsCompassItem.ItemDef.itemIndex, false, true);
            float healthMult    = CollectorsCompassItem.Health_Multiply.Value;
            float speedMult     = CollectorsCompassItem.Speed_Multiply.Value;

            if (CollectorsCompassItem.Health_Multiply_Stack.Value > 0)
            {
                float itemScale = CollectorsCompassItem.Health_Multiply_Stack.Value * (itemCount - 1);
                healthMult += itemScale;
            }

            if (CollectorsCompassItem.Speed_Multiply_Stack.Value > 0)
            {
                float itemScale = CollectorsCompassItem.Speed_Multiply_Stack.Value * (itemCount - 1);
                speedMult += itemScale;
            }

            args.healthTotalMult += healthMult / 100f;
            args.moveSpeedMultAdd += speedMult / 100f;
        }
    }
    private void SceneDirector_onPostPopulateSceneServer(SceneDirector self)
    {
        ActivateCount = 0;

        foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
        {
            if (!player.master) continue;

            bool allyHasItem = player.master.inventory.GetItemCount(CollectorsCompassItem.ItemDef) > 0;

            if (allyHasItem)
            {
                MarkInteract();
                break;
            }
        }
    }
    private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
    {
        orig(self);

        bool isValidInteract;

        switch (self.costType)
        {
            case CostTypeIndex.None:
            case CostTypeIndex.Money:
            case CostTypeIndex.PercentHealth:
            case CostTypeIndex.SoulCost:
            case CostTypeIndex.WhiteItem:
            case CostTypeIndex.GreenItem:
            case CostTypeIndex.RedItem:
            case CostTypeIndex.BossItem:
            case CostTypeIndex.Equipment:
                isValidInteract = true;
                break;
            default:
                isValidInteract = false;
                break;
        }

        if (NetworkServer.active && isValidInteract && self.gameObject.activeSelf)
        {
            self.gameObject.AddComponent<CompassList>();
        }
    }
    private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
    {
        orig(self, activator);

        CompassList component = self.gameObject.GetComponent<CompassList>();

        if (component && component.IsMarked) ReplaceMark(component, self.gameObject);
        else if (component && component.gameObject.GetComponent<ShopTerminalBehavior>())
        {
            MultiShopController multiShop = component.gameObject.GetComponent<ShopTerminalBehavior>()?.serverMultiShopController;
            if (multiShop && !multiShop.available)
            {
                foreach (GameObject terminal in multiShop.terminalGameObjects)
                {
                    CompassList terminalComponent = terminal.GetComponent<CompassList>();
                    if (terminalComponent.IsMarked) ReplaceMark(terminalComponent, terminal);
                    else
                    {
                        terminal.GetComponent<Highlight>().highlightColor = Highlight.HighlightColor.interactive;
                        terminal.GetComponent<Highlight>().isOn = false;
                    }

                }
            }
        }
    }
    private void BarrelInteraction_OnInteractionBegin(On.RoR2.BarrelInteraction.orig_OnInteractionBegin orig, BarrelInteraction self, Interactor activator)
    {
        orig(self, activator);

        CompassList component = self.gameObject.GetComponent<CompassList>();
        if (component && component.IsMarked) ReplaceMark(component, self.gameObject);
    }

    public class CompassList : MonoBehaviour
    {
        public bool IsMarked;
        public GameObject PosIndicator;
        public GameObject MarkEffect;

        public static List<CompassList> AllInteractables = [];

        public void Awake() => AllInteractables.Add(this);
        public void OnDisable() => RemoveFromList();
        public void OnDestroy() => RemoveFromList();
        public void RemoveFromList() => AllInteractables.Remove(this);
    }
    public class PermanentReveal : NetworkBehaviour
    {
        private PositionIndicator PosIndicator;
        private PurchaseInteraction PurchaseInteract;
        private BarrelInteraction BarrelInteract;
        public static void ShowObject(GameObject gameObject)
        {
            if (!gameObject.GetComponent<PermanentReveal>()) gameObject.AddComponent<PermanentReveal>();
        }

        public void OnEnable()
        {
            if (gameObject)
            {
                NetworkIdentity networkComponent = gameObject.GetComponent<NetworkIdentity>();
                if (networkComponent) {
                    foreach (PlayerCharacterMasterController masterController in PlayerCharacterMasterController.instances) {
                        if (masterController != null && masterController.isPingTheSameNetworkObject(networkComponent)) {
                            Destroy(this);
                            return;
                        }
                    }
                }

                GameObject indicatorPrefab = Instantiate(RoR2Indicator, transform.position, Quaternion.identity);

                PosIndicator = indicatorPrefab.GetComponent<PositionIndicator>();
                PosIndicator.insideViewObject.GetComponent<SpriteRenderer>().sprite = RoR2.UI.PingIndicator.GetInteractableIcon(gameObject);
                PosIndicator.targetTransform = transform;

                PurchaseInteract = gameObject.GetComponent<PurchaseInteraction>();
                BarrelInteract = gameObject.GetComponent<BarrelInteraction>();
            }
        }
        public void OnDisable()
        {
            if (PosIndicator)
            {
                Destroy(PosIndicator.gameObject);
                PosIndicator = null;
            }
        }

        public void FixedUpdate()
        {
            bool isNotValid = false;

            if (PurchaseInteract && !PurchaseInteract.available) isNotValid = true;
            if (BarrelInteract && BarrelInteract.opened) isNotValid = true;

            if (isNotValid) OnDisable();
            else if (!isNotValid && !PosIndicator) OnEnable();
        }
    }
}