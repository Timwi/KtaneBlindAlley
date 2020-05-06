using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using UnityEngine;

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
    public KMRuleSeedable RuleSeedable;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private readonly RegionState[] _regionStates = new RegionState[8];

    enum RegionState
    {
        Strike = 0,
        Unclicked = 1,
        Clicked = 2
    }

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        var conditions = new KeyValuePair<string, bool>[]
        {
            new KeyValuePair<string, bool>("lit IND", Bomb.IsIndicatorOn(Indicator.IND)),
            new KeyValuePair<string, bool>("DVI-D port", Bomb.IsPortPresent(Port.DVI)),
            new KeyValuePair<string, bool>("RJ-45 port", Bomb.IsPortPresent(Port.RJ45)),
            new KeyValuePair<string, bool>("even number of battery holders", Bomb.GetBatteryHolderCount() % 2 == 0),
            new KeyValuePair<string, bool>("unlit FRQ", Bomb.IsIndicatorOff(Indicator.FRQ)),
            new KeyValuePair<string, bool>("lit CAR", Bomb.IsIndicatorOn(Indicator.CAR)),
            new KeyValuePair<string, bool>("lit SND", Bomb.IsIndicatorOn(Indicator.SND)),
            new KeyValuePair<string, bool>("unlit CLR", Bomb.IsIndicatorOff(Indicator.CLR)),
            new KeyValuePair<string, bool>("lit TRN", Bomb.IsIndicatorOn(Indicator.TRN)),
            new KeyValuePair<string, bool>("parallel port", Bomb.IsPortPresent(Port.Parallel)),
            new KeyValuePair<string, bool>("even number of batteries", Bomb.GetBatteryCount() % 2 == 0),
            new KeyValuePair<string, bool>("Stereo RCA port", Bomb.IsPortPresent(Port.StereoRCA)),
            new KeyValuePair<string, bool>("serial port", Bomb.IsPortPresent(Port.Serial)),
            new KeyValuePair<string, bool>("unlit TRN", Bomb.IsIndicatorOff(Indicator.TRN)),
            new KeyValuePair<string, bool>("PS/2 port", Bomb.IsPortPresent(Port.PS2)),
            new KeyValuePair<string, bool>("unlit FRK", Bomb.IsIndicatorOff(Indicator.FRK)),
            new KeyValuePair<string, bool>("lit BOB", Bomb.IsIndicatorOn(Indicator.BOB)),
            new KeyValuePair<string, bool>("lit NSA", Bomb.IsIndicatorOn(Indicator.NSA)),
            new KeyValuePair<string, bool>("lit SIG", Bomb.IsIndicatorOn(Indicator.SIG)),
            new KeyValuePair<string, bool>("lit MSA", Bomb.IsIndicatorOn(Indicator.MSA)),
            new KeyValuePair<string, bool>("even digit in serial number", Bomb.GetSerialNumberNumbers().Any(n => n % 2 == 0)),
            new KeyValuePair<string, bool>("lit FRK", Bomb.IsIndicatorOn(Indicator.FRK)),
            new KeyValuePair<string, bool>("unlit CAR", Bomb.IsIndicatorOff(Indicator.CAR)),
            new KeyValuePair<string, bool>("unlit NSA", Bomb.IsIndicatorOff(Indicator.NSA)),
            new KeyValuePair<string, bool>("unlit BOB", Bomb.IsIndicatorOff(Indicator.BOB)),
            new KeyValuePair<string, bool>("vowel in serial number", Bomb.GetSerialNumber().Any(ch => "AEIOU".Contains(ch))),
            new KeyValuePair<string, bool>("unlit SND", Bomb.IsIndicatorOff(Indicator.SND)),
            new KeyValuePair<string, bool>("lit CLR", Bomb.IsIndicatorOn(Indicator.CLR)),
            new KeyValuePair<string, bool>("unlit MSA", Bomb.IsIndicatorOff(Indicator.MSA)),
            new KeyValuePair<string, bool>("unlit IND", Bomb.IsIndicatorOff(Indicator.IND)),
            new KeyValuePair<string, bool>("unlit SIG", Bomb.IsIndicatorOff(Indicator.SIG)),
            new KeyValuePair<string, bool>("lit FRQ", Bomb.IsIndicatorOn(Indicator.FRQ))
        };

        var rnd = RuleSeedable.GetRNG();
        Debug.LogFormat("[Blind Alley #{0}] Using rule seed: {1}", _moduleId, rnd.Seed);

        rnd.ShuffleFisherYates(conditions);

        // Find out which regions _should_ be clicked
        var conditionsMet = new List<string>[8];
        for (int i = 0; i < 8; i++)
            conditionsMet[i] = new List<string>();
        for (int i = 0; i < 4 * 8; i++)
            if (conditions[i].Value)
                conditionsMet[i / 4].Add(conditions[i].Key);

        var highestCount = conditionsMet.Max(l => l.Count);
        var regionNames = new[] { "TL", "TM", "ML", "MC", "MR", "BL", "BM", "BR" };
        var isSolved = false;

        for (int i = 0; i < 8; i++)
        {
            _regionStates[i] = conditionsMet[i].Count == highestCount ? RegionState.Unclicked : RegionState.Strike;
            var j = i;
            Regions[i].OnInteract += delegate
            {
                Regions[j].AddInteractionPunch();
                if (isSolved)
                    return false;
                if (_regionStates[j] == RegionState.Strike)
                {
                    Debug.LogFormat("[Blind Alley #{1}] You pressed region {0}, which is wrong.", regionNames[j], _moduleId);
                    Module.HandleStrike();
                }
                else
                {
                    _regionStates[j] = RegionState.Clicked;
                    var unclicked = Enumerable.Range(0, 8).Where(ix => _regionStates[ix] == RegionState.Unclicked).Select(ix => regionNames[ix]).ToArray();
                    Debug.LogFormat("[Blind Alley #{2}] Region {0} is correct. Remaining unclicked regions: {1}", regionNames[j], unclicked.Length == 0 ? "none" : string.Join(", ", unclicked), _moduleId);
                    if (unclicked.Length == 0)
                    {
                        Module.HandlePass();
                        isSolved = true;
                    }
                }
                return false;
            };
        }

        for (int i = 0; i < 8; i++)
            Debug.LogFormat("[Blind Alley #{0}] Region {1} conditions met: {2}{3}", _moduleId, regionNames[i], conditionsMet[i].Count, conditionsMet[i].Count == 0 ? null : " (" + string.Join(", ", conditionsMet[i].ToArray()) + ")");

        Debug.LogFormat("[Blind Alley #{0}] Must press regions: {1}", _moduleId, string.Join(", ", Enumerable.Range(0, 8).Where(ix => _regionStates[ix] == RegionState.Unclicked).Select(ix => regionNames[ix]).ToArray()));
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} TL, TM [top-left, top-middle, etc.]";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        var split = command.Trim().ToLowerInvariant().Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
            return null;
        var skip = split[0].Equals("press", StringComparison.InvariantCulture | StringComparison.CurrentCulture) ? 1 : 0;
        if (!split.Skip(skip).Any())
            return null;

        var btns = new List<KMSelectable>();
        foreach (var cmd in split.Skip(skip))
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

    IEnumerator TwitchHandleForcedSolve()
    {
        for (var i = 0; i < 8; i++)
        {
            if (_regionStates[i] == RegionState.Unclicked)
            {
                Regions[i].OnInteract();
                yield return new WaitForSeconds(.25f);
            }
        }
    }
}
