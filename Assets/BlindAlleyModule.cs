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
    public KMSelectable[] Regions;

    enum RegionState
    {
        Strike = 0,
        Unclicked = 1,
        Clicked = 2
    }

    void Start()
    {
        Debug.Log("[Blind Alley] Started");
        Module.OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        Debug.Log("[Blind Alley] Activated");

        // Find out which region _should_ be clicked
        var counts = new int[8];

        if (Bomb.GetSerialNumber() == null)
        {
            // Testing in Unity
            counts[0] = Rnd.Range(0, 5);
            counts[1] = Rnd.Range(0, 5);
            counts[2] = Rnd.Range(0, 5);
            counts[3] = Rnd.Range(0, 5);
            counts[4] = Rnd.Range(0, 5);
            counts[5] = Rnd.Range(0, 5);
            counts[6] = Rnd.Range(0, 5);
            counts[7] = Rnd.Range(0, 5);
        }
        else
        {
            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.BOB)) counts[0]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.CAR)) counts[0]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.IND)) counts[0]++;
            if (Bomb.GetBatteryHolderCount() % 2 == 0) counts[0]++;

            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.CAR)) counts[1]++;
            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.NSA)) counts[1]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.FRK)) counts[1]++;
            if (Bomb.GetPortCount(KMBombInfoExtensions.KnownPortType.RJ45) > 0) counts[1]++;

            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.FRQ)) counts[2]++;
            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.IND)) counts[2]++;
            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.TRN)) counts[2]++;
            if (Bomb.GetPortCount(KMBombInfoExtensions.KnownPortType.DVI) > 0) counts[2]++;

            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.SIG)) counts[3]++;
            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.SND)) counts[3]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.NSA)) counts[3]++;
            if (Bomb.GetBatteryCount() % 2 == 0) counts[3]++;

            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.BOB)) counts[4]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.CLR)) counts[4]++;
            if (Bomb.GetPortCount(KMBombInfoExtensions.KnownPortType.PS2) > 0) counts[4]++;
            if (Bomb.GetPortCount(KMBombInfoExtensions.KnownPortType.Serial) > 0) counts[4]++;

            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.FRQ)) counts[5]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.SIG)) counts[5]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.TRN)) counts[5]++;
            if (Bomb.GetSerialNumber().Any("02468".Contains)) counts[5]++;

            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.FRK)) counts[6]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.MSA)) counts[6]++;
            if (Bomb.GetPortCount(KMBombInfoExtensions.KnownPortType.Parallel) > 0) counts[6]++;
            if (Bomb.GetSerialNumber().Any("AEIOU".Contains)) counts[6]++;

            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.CLR)) counts[7]++;
            if (Bomb.IsIndicatorOff(KMBombInfoExtensions.KnownIndicatorLabel.MSA)) counts[7]++;
            if (Bomb.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.SND)) counts[7]++;
            if (Bomb.GetPortCount(KMBombInfoExtensions.KnownPortType.StereoRCA) > 0) counts[7]++;
        }

        var highestCount = counts.Max();
        var states = new RegionState[8];

        for (int i = 0; i < 8; i++)
        {
            states[i] = counts[i] == highestCount ? RegionState.Unclicked : RegionState.Strike;
            var j = i;
            Regions[i].OnInteract += delegate
            {
                if (states[j] == RegionState.Strike)
                {
                    Debug.LogFormat("[Blind Alley] You pressed region #{0}, which is wrong.", j + 1);
                    Module.HandleStrike();
                }
                else
                {
                    states[j] = RegionState.Clicked;
                    var unclicked = Enumerable.Range(0, 8).Where(ix => states[ix] == RegionState.Unclicked).Select(ix => (ix + 1).ToString()).ToArray();
                    Debug.LogFormat("[Blind Alley] Region #{0} is correct. Remaining unclicked regions: {1}", j + 1, unclicked.Length == 0 ? "none" : string.Join(", ", unclicked));
                    if (unclicked.Length == 0)
                        Module.HandlePass();
                }
                return false;
            };
        }

        Debug.Log("[Blind Alley] Region condition counts: " + string.Join(", ", counts.Select(c => c.ToString()).ToArray()));
        Debug.Log("[Blind Alley] Must press regions: " + string.Join(", ", Enumerable.Range(0, 8).Where(ix => states[ix] == RegionState.Unclicked).Select(ix => (ix + 1).ToString()).ToArray()));
    }
}
