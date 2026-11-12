using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private EventSystems_Controller _iconHoverController;
    [SerializeField] private TextMeshProUGUI _deckCardCountText;

    [Space(20)]
    [SerializeField] private Image _shadowBackGround;
    [SerializeField] private Image _builtDeckPanel;

    [Space(20)]
    [SerializeField] private Sprite _drawnIndicationSprite;
    [SerializeField] private DisplayCard[] _builtDeckDisplayCards;


    // MonoBehaviour
    private void Awake()
    {
        Update_DeckCardCount_Text(0);
        UnToggle();

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
        _iconHoverController.OnPointerState += HoverIcon_PreviewPanel;

        GameManager manager = GameManager.instance;

        manager.handInventory.drawCardFromDeck.Register(0, Load_DeckCards);
        manager.stageManager.endTurnEventBus.Register(0, UnToggle);
    }

    public void UnSubscribe_All()
    {
        _iconHoverController.OnPointerState -= HoverIcon_PreviewPanel;

        GameManager manager = GameManager.instance;

        manager.handInventory.drawCardFromDeck.UnRegister(Load_DeckCards);
        manager.stageManager.endTurnEventBus.UnRegister(UnToggle);
    }



    // Hover Icon
    public void Update_DeckCardCount_Text(int updateCount)
    {
        _deckCardCountText.text = Mathf.Max(0, updateCount).ToString();
    }

    private void HoverIcon_PreviewPanel(bool previewToggle)
    {

    }


    // Built Deck Panel
    private void Toggle(bool toggle)
    {
        _shadowBackGround.gameObject.SetActive(toggle);
        _builtDeckPanel.gameObject.SetActive(toggle);

        if (toggle == false) return;
        Load_DeckCards();
    }

    public void Toggle()
    {
        Toggle(!_builtDeckPanel.gameObject.activeSelf);
    }
    public void UnToggle()
    {
        Toggle(false);
    }

    public void Load_DeckCards()
    {
        GameManager manager = GameManager.instance;
        DeckBuild_Data deckBuildData = manager.deckBuildManager.data;

        List<CardData> deckCards = new(deckBuildData.builtDeckCardDatas);

        bool loadCurrentDeck = deckBuildData.deckBuildComplete && manager.stageManager.Is_BattleStage();
        List<CardData> currentDeckCards = loadCurrentDeck ? new(manager.handInventory.deckCards) : null;

        for (int i = 0; i < _builtDeckDisplayCards.Length; i++)
        {
            DisplayCard displayCard = _builtDeckDisplayCards[i];

            bool toggle = i <= deckCards.Count - 1;
            displayCard.Toggle(toggle);

            if (toggle == false) continue;

            CardData loadCardData = deckCards[i];
            displayCard.Load_CardComponents(loadCardData.cardScrObj);

            if (loadCurrentDeck == false) continue;

            bool drawn = currentDeckCards.Contains(loadCardData) == false;

            displayCard.Update_UnlockVisuals(drawn == false);
            displayCard.Load_IndicationIcon(drawn ? _drawnIndicationSprite : null);
        }
    }
}