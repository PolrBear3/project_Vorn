using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventMenu : MonoBehaviour
{
    public abstract bool Toggle_Available(Event_ScrObj checkEvent);
    
    public abstract void Toggle(Event_ScrObj toggleEvent);
}