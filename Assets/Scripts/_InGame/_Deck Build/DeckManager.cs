using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeckManager : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private TextMeshProUGUI _deckCardCountText;


    // MonoBehaviour
    private void Awake()
    {
        Update_DeckCardCount_Text(0);

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

    }

    public void UnSubscribe_All()
    {

    }



    // Hover Icon
    public void Update_DeckCardCount_Text(int updateCount)
    {
        _deckCardCountText.text = Mathf.Max(0, updateCount).ToString();
    }
}
