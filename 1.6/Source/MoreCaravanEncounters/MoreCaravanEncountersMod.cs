using Verse;
using UnityEngine;
using HarmonyLib;

namespace MoreCaravanEncounters;

public class MoreCaravanEncountersMod : Mod
{
    public static Settings settings;

    public MoreCaravanEncountersMod(ModContentPack content) : base(content)
    {

        // initialize settings
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("keyz182.rimworld.MoreCaravanEncounters.main");	
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "MoreCaravanEncounters_SettingsCategory".Translate();
    }
}
