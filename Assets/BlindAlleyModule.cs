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

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    enum RegionState
    {
        Strike = 0,
        Unclicked = 1,
        Clicked = 2
    }

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        // Find out which regions _should_ be clicked
        var counts = new int[8];

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

        var highestCount = counts.Max();
        var states = new RegionState[8];
        var regionNames = new[] { "TL", "TM", "ML", "MC", "MR", "BL", "BM", "BR" };

        for (int i = 0; i < 8; i++)
        {
            states[i] = counts[i] == highestCount ? RegionState.Unclicked : RegionState.Strike;
            var j = i;
            Regions[i].OnInteract += delegate
            {
                Regions[j].AddInteractionPunch();
                if (states[j] == RegionState.Strike)
                {
                    Debug.LogFormat("[Blind Alley #{1}] You pressed region {0}, which is wrong.", regionNames[j], _moduleId);
                    Module.HandleStrike();
                }
                else
                {
                    states[j] = RegionState.Clicked;
                    var unclicked = Enumerable.Range(0, 8).Where(ix => states[ix] == RegionState.Unclicked).Select(ix => regionNames[ix]).ToArray();
                    Debug.LogFormat("[Blind Alley #{2}] Region {0} is correct. Remaining unclicked regions: {1}", regionNames[j], unclicked.Length == 0 ? "none" : string.Join(", ", unclicked), _moduleId);
                    if (unclicked.Length == 0)
                        Module.HandlePass();
                }
                return false;
            };
        }

        Debug.LogFormat("[Blind Alley #{8}] Region condition counts:\n{0} {1}\n{2} {3} {4}\n{5} {6} {7}", counts.Select(c => c.ToString()).Concat(new[] { _moduleId.ToString() }).ToArray());
        Debug.LogFormat("[Blind Alley #{1}] Must press regions: {0}", string.Join(", ", Enumerable.Range(0, 8).Where(ix => states[ix] == RegionState.Unclicked).Select(ix => regionNames[ix]).ToArray()), _moduleId);
    }

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        var split = command.Trim().ToLowerInvariant().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (split.Length < 2 || split[0] != "press")
            return null;

        var btns = new List<KMSelectable>();
        foreach (var cmd in split.Skip(1))
            switch (cmd.Replace("center", "middle").Replace("centre", "middle"))
            {
                case "tl": case "lt": case "topleft": case "lefttop": btns.Add(Regions[0]); break;
                case "tm": case "tc": case "mt": case "ct": case "topmiddle": case "middletop": btns.Add(Regions[1]); break;

                case "ml": case "cl": case "lm": case "lc": case "middleleft": case "leftmiddle": btns.Add(Regions[2]); break;
                case "mm": case "cm": case "mc": case "cc": case "middle": case "middlemiddle": btns.Add(Regions[3]); break;
                case "mr": case "cr": case "rm": case "rc": case "middleright": case "rightmiddle": btns.Add(Regions[4]); break;

                case "bl": case "lb": case "bottomleft": case "leftbottom": btns.Add(Regions[5]); break;
                case "bm": case "bc": case "mb": case "cb": case "bottommiddle": case "middlebottom": btns.Add(Regions[6]); break;
                case "br": case "rb": case "bottomright": case "rightbottom": btns.Add(Regions[7]); break;

                default: return null;
            }
        return btns.ToArray();
    }
}
