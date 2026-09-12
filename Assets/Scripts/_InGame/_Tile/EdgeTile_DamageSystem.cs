using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTile_DamageSystem : MonoBehaviour
{
    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);
    }


    // Data
    private void Set_Data()
    {
        // Damage_EdgeTile_PlayerInteractables end turn sequence to order 0 registeration
    }


    // Damaging
    private List<IInteractable> EdgeTile_Interactables()
    {
        List<Tile> tiles = GameManager.instance.tileManager.Edged_Tiles();
        List<IInteractable> interactables = new();

        for (int i = 0; i < tiles.Count; i++)
        {
            IInteractable interactable = tiles[i].CurrentOccupant_Interactable();
            if (interactable == null) continue;

            interactables.Add(interactable);
        }
        return interactables;
    }

    private IEnumerator Damage_EdgeTile_PlayerInteractables()
    {
        List<IInteractable> damageInteractables = EdgeTile_Interactables();

        for (int i = 0; i < damageInteractables.Count; i++)
        {
            IInteractable interactable = damageInteractables[i];
            if (interactable is Enemy) continue;

            // get damage stacking data

            // damage interactable

            // wait for health update event bus to end

            // update damage stacking data
        }
        yield break;
    }
}
