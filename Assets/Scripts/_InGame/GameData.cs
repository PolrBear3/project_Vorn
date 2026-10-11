using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [SerializeField] private Hero_ScrObj _hero;
    public Hero_ScrObj hero => _hero;
}
