using RimWorld;
using Verse;

namespace Fortified
{
    public class HeavyEquippableExtension : DefModExtension
    {
        public HeavyEquippableDef EquippableDef;

        /// <summary>
        /// 處裡以下狀況：
        /// 1.是特定種族(Race)
        /// 2.穿戴特定服裝
        /// 3.具有特定Hediff
        /// 4.具有特定基因
        /// 5.體型判定。(掛載型武器沒有體型門檻，只認 1~4 的白名單)
        /// 6.任一符合則回傳true，否則false
        /// </summary>
        public bool CanEquippedBy(Pawn pawn)
        {
            if (EquippableDef == null)
            {
                Log.Warning("HeavyEquippableExtension doesn't have a HeavyEquippableDef sets!");
                return true;
            }

            // 先處理所有可免除體型檢查的條件
            if (EquippableDef.EquippableByRace.NotNullAndContains(pawn.def))
                return true;
            
            if (CheckUtility.HasAnyApparelOf(pawn, EquippableDef.EquippableWithApparel))
                return true;
            
            if (CheckUtility.HasAnyHediffOf(pawn, EquippableDef.EquippableWithHediff))
                return true;
            
            if (CheckUtility.HasAnyGeneOf(pawn, EquippableDef.EquippableWithGene))
                return true;

            // 掛載型武器沒有體型門檻，上面的白名單都沒中就是不能裝備。
            // （機械體靠武器系統白名單取得的權限由 CheckUtility.UseableInStatic 負責）
            if (EquippableDef.IsMountedWeapon)
                return false;

            // 體型判定。
            float requiredSize = EquippableDef.EquippableBaseBodySize;

            // 機械體特殊處理：只有在未啟用武器過濾時才允許以體型判定
            if (pawn.RaceProps.IsMechanoid)
            {
                var ext = pawn.def.GetModExtension<MechWeaponExtension>();
                if (ext == null)
                    return false;
                
                if (!ext.EnableWeaponFilter && pawn.BodySize >= requiredSize)
                    return true;
                
                return false;
            }

            // 非機械體：直接進行體型比對
            return pawn.BodySize >= requiredSize;
        }

        /// <summary>
        /// 初始化階段提供清單生成使用的判斷。
        /// </summary>
        public bool CanEquippedBy(ThingDef pawnRaceDef)
        {
            if (EquippableDef == null)
            {
                Log.Warning("HeavyEquippableExtension doesn't have a HeavyEquippableDef sets!");
                return true;
            }

            if (EquippableDef.EquippableByRace.Contains(pawnRaceDef))
                return true;

            var ext = pawnRaceDef.GetModExtension<MechWeaponExtension>();
            
            // 機械體：未啟用武器過濾才允許以體型判定
            if (ext != null && ext.EnableWeaponFilter)
                return false;

            // 掛載型武器沒有體型門檻，種族白名單以外一律不過。
            if (EquippableDef.IsMountedWeapon)
                return false;

            float requiredSize = EquippableDef.EquippableBaseBodySize;

            if (pawnRaceDef?.race != null && pawnRaceDef.race.baseBodySize >= requiredSize)
                return true;

            return false;
        }
    }
}
