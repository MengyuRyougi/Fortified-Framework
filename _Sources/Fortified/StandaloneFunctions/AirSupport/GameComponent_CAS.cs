using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Fortified
{
    public class GameComponent_CAS : GameComponent
    {
        private static GameComponent_CAS instance;

        public GameComponent_CAS()
        {
            instance = null;
        }
        public GameComponent_CAS(Game game)
        {
            instance = null;
        }

        public override void FinalizeInit()
        {
            instance = this;
        }

        private List<AirSupportData> datas = new List<AirSupportData>(), tempRemoveDatas = new List<AirSupportData>();

        public static void AddData(AirSupportData data)
        {
            if (instance == null)
            {
                return;
            }
            instance.datas.Add(data);
            instance.datas.SortBy(d => d.triggerTick);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (datas.Any())
            {
                tempRemoveDatas = datas.ListFullCopy();
                foreach (AirSupportData data in tempRemoveDatas)
                {
                    if (data.triggerTick > Find.TickManager.TicksGame) break;
                    try
                    {
                        data.Trigger();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{ex.Message} {ex.StackTrace}");
                    }
                    datas.Remove(data);
                }
                tempRemoveDatas.Clear();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref datas, "datas", LookMode.Deep);
        }
    }
}
