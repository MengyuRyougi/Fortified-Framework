using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

namespace Fortified
{
    public class Command_SelectApparelOutfit : Command
    {
        public Pawn_OutfitTracker outfitSource;

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => GetFloatMenu();

        private IEnumerable<FloatMenuOption> GetFloatMenu()
        {
            if (outfitSource == null) yield break;
            List<ApparelPolicy> outfis = Current.Game.outfitDatabase.AllOutfits;
            foreach (var outfit in outfis)
            {
                yield return new FloatMenuOption(outfit.label, () =>
                {
                    outfitSource.CurrentApparelPolicy = outfit;
                    outfitSource.pawn.TryOptimizeApparel();
                });
            }
            yield break;
        }
    }
}