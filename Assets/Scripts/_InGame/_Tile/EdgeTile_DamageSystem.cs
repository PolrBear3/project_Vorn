using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTile_DamageSystem : MonoBehaviour, ISubscriptionHandler
{
    private EdgeTile_DamageSystemData _data = new();  // for save & load
    public EdgeTile_DamageSystemData data => _data;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        GameManager.instance.stageManager.endTurnEventBus.Register(0, Damage_EdgeTile_PlayerInteractables);
    }

    public void UnSubscribe_All()
    {
        GameManager.instance.stageManager.endTurnEventBus.UnRegister(Damage_EdgeTile_PlayerInteractables);
    }


    // Damaging
    private List<IInteractable> TargetTiles_DamageInteractables(List<Tile> targetTiles)
    {
        List<IInteractable> interactables = new();

        for (int i = 0; i < targetTiles.Count; i++)
        {
            Tile tile = targetTiles[i];

            IInteractable interactable = tile.CurrentOccupant_Interactable();
            if (interactable == null) continue;

            interactables.Add(interactable);
        }
        return interactables;
    }
    private void Play_DamagingStates(List<Tile> statePlayTiles)
    {
        for (int i = 0; i < statePlayTiles.Count; i++)
        {
            Animator_Controller[] controllers = statePlayTiles[i].animatorControllers;

            for (int j = 0; j < controllers.Length; j++)
            {
                controllers[j].Play_State(OccupantAnimation.Damaged);
            }
        }
    }

    private IEnumerator Damage_EdgeTile_PlayerInteractables()
    {
        List<Tile> tiles = GameManager.instance.tileManager.Edged_Tiles();
        List<IInteractable> damageInteractables = TargetTiles_DamageInteractables(tiles);

        for (int i = damageInteractables.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = damageInteractables[i];
            if (interactable is Enemy) continue;

            InteractionData data = interactable.interactionData;
            int damageCount = _data.Current_StackDamageCount(interactable) + 1;

            data.Update_CurrentHealth(data.currentHealth - damageCount);
            _data.Update_StackDamage(interactable);

            Play_DamagingStates(tiles);

            yield return null;
            while (data.dataUpdating) yield return null;
        }

        yield break;
    }
}