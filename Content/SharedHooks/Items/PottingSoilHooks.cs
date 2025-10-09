using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using RoR2BepInExPack.GameAssetPathsBetter;
using System;

namespace ScalesAsclepius;
public class PottingSoilHooks
{
    //private static readonly string InternalName = "PottingSoilHooks";
    public static bool ItemEnabled;

    public PottingSoilHooks()
    {
        ItemEnabled = PottingSoilItem.Item_Enabled.Value;

        if (ItemEnabled)
        {
            On.RoR2.GlobalEventManager.OnInteractionBegin += GlobalEventManager_OnInteractionBegin;
        }
    }

    private void GlobalEventManager_OnInteractionBegin(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
    {
        orig(self, interactor, interactable, interactableObject);

        CharacterBody body = interactor.GetComponent<CharacterBody>();

        if (body && body.inventory)
        {
            int itemCount = body.inventory.GetItemCount(PottingSoilItem.ItemDef);

            if (itemCount > 0 && PermittedInteractSpawn((MonoBehaviour)interactable, interactableObject))
            {
                SpawnCard squidPlaceholder = LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscSquidTurret");

                DirectorPlacementRule placeRule = new()
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = interactableObject.transform.position,
                    maxDistance = 20f,
                    minDistance = 5f
                };

                DirectorSpawnRequest spawnReq = new(squidPlaceholder, placeRule, RoR2Application.rng)
                {
                    teamIndexOverride = new TeamIndex?(TeamIndex.Player),
                    summonerBodyObject = interactor.gameObject
                };

                DirectorCore.instance.TrySpawnObject(spawnReq);

                /*
                SpawnCard spawnCard = LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscSquidTurret");
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 5f,
                    maxDistance = 25f,
                    position = interactableObject.transform.position
                };
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng);
                directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Player);
                directorSpawnRequest.summonerBodyObject = interactor.gameObject;
                DirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
                directorSpawnRequest2.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest2.onSpawnedServer, new Action<SpawnCard.SpawnResult>(CS$<> 8__locals1.< OnInteractionBegin > b__1));
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                */
            }
        }
    }

    // Why Hopoo.. A local class..?!?
    bool PermittedInteractSpawn(MonoBehaviour interactMonoBehaviour, GameObject interactObject)
    {
        if (!interactMonoBehaviour) return false;

        InteractionProcFilter interactionProcFilter = interactObject.GetComponent<InteractionProcFilter>();
        if (interactionProcFilter) return interactionProcFilter.shouldAllowOnInteractionBeginProc;


        if (interactMonoBehaviour.GetComponent<DelusionChestController>())
        {
            if (interactMonoBehaviour.GetComponent<PickupPickerController>().enabled) return false;
            return true;
        }

        if (interactMonoBehaviour.GetComponent<VehicleSeat>()) return false;
        if (interactMonoBehaviour.GetComponent<GenericPickupController>()) return false;
        if (interactMonoBehaviour.GetComponent<NetworkUIPromptController>()) return false;

        return true;
    }
}