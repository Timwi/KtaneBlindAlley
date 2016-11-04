using System;
using System.Collections.Generic;
using System.Linq;
using BlindAlley;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Blind Alley
/// Created by Timwi
/// </summary>
public class BlindAlleyModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;

    void Start()
    {
        Debug.Log("[BlindAlley] Started");
        Module.OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        Debug.Log("[BlindAlley] Activated");

        for (int i = 0; i < 6; i++)
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[0];
        }
    }
}
