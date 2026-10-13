using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero_SelectGroup : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _layoutPlatform;

    [Space(10)]
    [SerializeField] private Image _heroIcon;
    [SerializeField] private Image _spawnCardContent;

    [Space(20)]
    [SerializeField] private Sprite[] _coinSprites;
    [SerializeField] private Image[] _coinImages;


    // UI
    public void Update_SelectHero(Hero_ScrObj updateHero)
    {
        _layoutPlatform.sprite = updateHero.cardPlatformSprite;

        _heroIcon.sprite = updateHero.toolTipBaseSprite;
        _spawnCardContent.sprite = updateHero.spawnCard.contentSprite;
    }

    public void Update_Coins(int updateAmount)
    {
        int updateCount = Mathf.Clamp(updateAmount, 0, _coinImages.Length);

        for (int i = 0; i < _coinImages.Length; i++)
        {
            _coinImages[i].sprite = updateCount <= 0 ? _coinSprites[0] : _coinSprites[1];
            updateCount--;
        }
    }


    // Button
    public void Select()
    {
        GameManager.instance.heroManager.heroSelectManager.Select_Hero(this);
    }
}
