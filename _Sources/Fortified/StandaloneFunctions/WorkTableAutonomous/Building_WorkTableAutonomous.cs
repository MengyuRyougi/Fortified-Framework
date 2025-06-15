using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Sound;
using Verse;

namespace Fortified
{
    public class Building_WorkTableAutonomous : Building_MechGestator
    {
        public override void Notify_FormingCompleted()
        {
            if (this.activeBill.CreateProducts() is Pawn p)
            {
                Messages.Message("GestationComplete".Translate() + ": " + p.kindDef.LabelCap, this, MessageTypeDefOf.PositiveEvent, true);
                this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
                this.innerContainer.TryAdd(p, true);
            }
            SoundDefOf.MechGestatorBill_Completed.PlayOneShot(this);
        }
        public override void PostPostMake()
        {
            if (!this.def.randomStyle.NullOrEmpty<ThingStyleChance>() && Rand.Chance(this.def.randomStyleChance))
            {
                this.StyleDef = this.def.randomStyle.RandomElementByWeight((ThingStyleChance x) => x.Chance).StyleDef;
            }
        }
    }
}
