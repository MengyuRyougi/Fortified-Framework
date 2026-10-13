using Verse;

namespace Fortified
{
    public class FortifiedSettings : ModSettings
    {
        // 開發模式下是否在建築選單顯示 FFF_DevCategory（關閉時開發模式也隱藏）
        public bool showDevCategory = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref showDevCategory, "showDevCategory", false);
        }
    }
}
