using System;
using System.Collections.Generic;
using System.Linq;
using MysteryMeat;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Mystery Meat
/// Created by Timwi
/// </summary>
public class MysteryMeatModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;

    void Start()
    {
        Debug.Log("[MysteryMeat] Started");
        Module.OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        Debug.Log("[MysteryMeat] Activated");
    }
}
