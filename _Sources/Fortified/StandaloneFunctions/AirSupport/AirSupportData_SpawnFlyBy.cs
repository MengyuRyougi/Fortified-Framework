using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class AirSupportData_SpawnFlyBy : AirSupportData_SpawnThing
    {
        public Vector3 origin;

        public List<AirSupportData> attachedDatas = new List<AirSupportData>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref origin, "origin");
            Scribe_Collections.Look(ref attachedDatas, "datas", LookMode.Deep);
        }

        protected override void PreProcess(Thing t)
        {
            var flyby = t as FlyByThing;
            flyby.vector = target.Cell.ToVector3Shifted() - origin;
            flyby.exactPos = origin;
            foreach (var data in attachedDatas)
            {
                if (data is IAttachedToFlyBy ia) ia.plane = flyby;
                GameComponent_CAS.AddData(data);
            }
            attachedDatas.Clear();
        }
    }
}
