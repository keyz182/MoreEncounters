using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace MoreCaravanEncounters;

public class IncidentWorker_RelicComplex : IncidentWorker_Ambush, ISignalReceiver
{
    public IncidentWorker_RelicComplex()
    {
        Find.SignalManager?.RegisterReceiver(this);
    }

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return parms.target is Caravan;
    }


    private bool TryFindEntryCell(Map map, out IntVec3 cell)
    {
        return CellFinder.TryFindRandomEdgeCellWith(x => x.Standable(map) && map.reachability.CanReachColony(x), map, CellFinder.EdgeRoadChance_Hostile, out cell);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        LongEventHandler.QueueLongEvent(() => DoExecute(parms), "GeneratingMapForNewEncounter", false, null);
        return true;
    }

    public static readonly string AwakenSecurityThreatsSignal = "MCE_AwakenSecurityThreats";

    private Site GenerateSite(IncidentParms incidentParms, int tile)
    {
        TryGetRandomPlayerRelic(out Precept_Relic preceptRelic);
        Thing relic = preceptRelic.GenerateRelic();
        relic.questTags = [relic.ThingID];

        SitePartParams parms = new() { points = incidentParms.points, relicThing = relic, triggerSecuritySignal = AwakenSecurityThreatsSignal, relicLostSignal = "RelicLost" };
        if (!(incidentParms.points > 0.0))
        {
            return SiteMaker.MakeSite(Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.AncientAltar, parms)), tile, Faction.OfMechanoids);
        }

        parms.exteriorThreatPoints = ExteriorThreatPointsOverPoints.Evaluate(incidentParms.points);
        parms.interiorThreatPoints = InteriorThreatPointsOverPoints.Evaluate(incidentParms.points);
        return SiteMaker.MakeSite(Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.AncientAltar, parms)), tile, Faction.OfMechanoids);
    }

    private void DoExecute(IncidentParms parms)
    {
        if (parms.target is not Caravan caravan) return;
        Site site = GenerateSite(parms, caravan.Tile.tileId);

        Find.World.worldObjects.Add(site);

        TaggedString letterLabel = (TaggedString) GetLetterLabel(caravan.pawns[0], parms);
        TaggedString letterText = (TaggedString) GetLetterText(caravan.pawns[0], parms);
        SendStandardLetter(letterLabel, letterText, GetLetterDef(caravan.pawns[0], parms), parms, null);
        Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
    }

    protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
    {
        return def.letterText.Translate(anyPawn.Named("PAWN"));
    }


    private static readonly SimpleCurve ExteriorThreatPointsOverPoints = new SimpleCurve()
    {
        { new CurvePoint(0.0f, 500f), true }, { new CurvePoint(500f, 500f), true }, { new CurvePoint(10000f, 10000f), true }
    };

    private static readonly SimpleCurve InteriorThreatPointsOverPoints = new SimpleCurve()
    {
        { new CurvePoint(0.0f, 300f), true }, { new CurvePoint(300f, 300f), true }, { new CurvePoint(10000f, 5000f), true }
    };


    private bool TryFindSiteTile(out PlanetTile tile, bool exitOnFirstTileFound = false)
    {
        return TileFinder.TryFindNewSiteTile(out tile, 2, 10, exitOnFirstTileFound: exitOnFirstTileFound);
    }

    private bool TryGetRandomPlayerRelic(out Precept_Relic relic)
    {
        return Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>().Where<Precept_Relic>((Func<Precept_Relic, bool>) (p => p.CanGenerateRelic))
            .TryRandomElement<Precept_Relic>(out relic);
    }

    private IEnumerable<QuestScriptDef> GetAllSubquests(QuestScriptDef parent)
    {
        return DefDatabase<QuestScriptDef>.AllDefs.Where<QuestScriptDef>((Func<QuestScriptDef, bool>) (q => q.epicParent == parent));
    }

    protected override List<Pawn> GeneratePawns(IncidentParms parms)
    {
        return [];
    }

    public void Notify_SignalReceived(Signal signal)
    {
        if (signal.tag.EndsWith(".StartedExtractingFromContainer"))
        {
            Find.SignalManager.SendSignal(new Signal(AwakenSecurityThreatsSignal, signal.args, true));
        }
    }
}
