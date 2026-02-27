using RimWorld;
using Verse;

namespace MoreCaravanEncounters;

[DefOf]
public static class MoreCaravanEncountersDefOf
{
    // Remember to annotate any Defs that require a DLC as needed e.g.
    // [MayRequireBiotech]
    // public static GeneDef YourPrefix_YourGeneDefName;
    
    static MoreCaravanEncountersDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MoreCaravanEncountersDefOf));
}
