using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeTile_DamageSystem : MonoBehaviour
{
    private EdgeTile_DamageSystemData _data = new();  // for save & load
    public EdgeTile_DamageSystemData data => _data;
    
    
    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        GameManager.instance.stageManager.endTurnEventBus.UnRegister(Damage_EdgeTile_PlayerInteractables);
    }


    // Data
    private void Set_Data()
    {
        GameManager.instance.stageManager.endTurnEventBus.Register(0, Damage_EdgeTile_PlayerInteractables);
    }


    // Damaging
    private List<IInteractable> EdgeTile_Interactables()
    {
        List<Tile> tiles = GameManager.instance.tileManager.Edged_Tiles();
        List<IInteractable> interactables = new();

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            
            IInteractable interactable = tile.CurrentOccupant_Interactable();
            if (interactable == null) continue;

            interactables.Add(interactable);
        }
        return interactables;
    }

    private IEnumerator Damage_EdgeTile_PlayerInteractables()
    {
        List<IInteractable> damageInteractables = EdgeTile_Interactables();

        for (int i = damageInteractables.Count - 1; i >= 0 ; i--)
        {
            IInteractable interactable = damageInteractables[i];
            if (interactable is Enemy) continue;

            InteractionData data = interactable.interactionData;
            int damageCount = _data.Current_StackDamageCount(interactable) + 1;

            data.Update_CurrentHealth(data.currentHealth - damageCount);
            _data.Update_StackDamage(interactable);

            yield return null;
            while (data.healthUpdating) yield return null;
        }
        
        yield break;
    }
}
