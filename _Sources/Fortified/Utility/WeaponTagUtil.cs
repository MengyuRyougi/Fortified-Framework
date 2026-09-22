using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Fortified
{
    /// <summary>
    /// 武器標籤工具類，用於管理和查詢武器定義及其關聯資料
    /// </summary>
    [StaticConstructorOnStartup]
    public static class WeaponTagUtil
    {
        private static readonly Dictionary<string, HashSet<ThingDef>> AllTags = new Dictionary<string, HashSet<ThingDef>>();
        private static ThingDef[] _turrets = new ThingDef[0];
        private static ThingDef[] _weaponUseableMechs = new ThingDef[0];
        private static ThingDef[] _allWeaponDefs = new ThingDef[0];
        private static readonly Dictionary<string, ThingDef> _caches = new Dictionary<string, ThingDef>();

        public static ThingDef[] GetTurrets => _turrets;

        static WeaponTagUtil()
        {
            InitializeAllWeapons();
            InitializeTurrets();
            InitializeWeaponUseableMechs();
        }

        /// <summary>
        /// 初始化所有武器及其標籤映射
        /// </summary>
        private static void InitializeAllWeapons()
        {
            _allWeaponDefs = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.IsWeapon)
                .ToArray();

            foreach (ThingDef weaponDef in _allWeaponDefs)
            {
                RegisterWeaponTags(weaponDef);
            }
        }

        /// <summary>
        /// 註冊單個武器的所有標籤
        /// </summary>
        private static void RegisterWeaponTags(ThingDef weaponDef)
        {
            if (weaponDef.weaponTags.NullOrEmpty())
            {
                return;
            }

            foreach (string tag in weaponDef.weaponTags.Distinct())
            {
                if (string.IsNullOrEmpty(tag))
                {
                    continue;
                }

                if (!AllTags.TryGetValue(tag, out var set))
                {
                    set = new HashSet<ThingDef>();
                    AllTags[tag] = set;
                }

                set.Add(weaponDef);
            }
        }

        /// <summary>
        /// 初始化可操作的砲塔
        /// </summary>
        private static void InitializeTurrets()
        {
            var turretList = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.building?.turretGunDef != null && HasMannableComponent(def))
                .ToList();

            turretList.SortBy(def => def.BaseMass);
            _turrets = turretList.ToArray();
        }

        /// <summary>
        /// 初始化機械體可用的重型裝備
        /// </summary>
        private static void InitializeWeaponUseableMechs()
        {
            var mechList = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.GetModExtension<MechWeaponExtension>() != null)
                .ToList();

            mechList.SortBy(def => def.BaseMass);
            _weaponUseableMechs = mechList.ToArray();
        }

        /// <summary>
        /// 檢查定義是否具有可操作元件
        /// </summary>
        private static bool HasMannableComponent(ThingDef def)
        {
            return def.GetCompProperties<CompProperties_Mannable>() != null;
        }

        /// <summary>
        /// 根據標籤列表獲取所有匹配的武器
        /// </summary>
        public static IEnumerable<ThingDef> GetWeapons(List<string> tags)
        {
            var weapons = new HashSet<ThingDef>();

            if (tags == null)
            {
                return weapons;
            }

            foreach (string tag in tags)
            {
                if (string.IsNullOrEmpty(tag))
                {
                    continue;
                }

                if (AllTags.TryGetValue(tag, out var set))
                {
                    weapons.UnionWith(set);
                }
            }

            return weapons;
        }

        /// <summary>
        /// 檢查武器是否存在
        /// </summary>
        public static bool WeaponExists(string defName, out ThingDef weaponDef)
        {
            if (string.IsNullOrEmpty(defName))
            {
                weaponDef = null;
                return false;
            }

            if (_caches.TryGetValue(defName, out weaponDef))
            {
                return true;
            }

            weaponDef = _allWeaponDefs.FirstOrDefault(def => def.defName == defName);

            if (weaponDef != null)
            {
                _caches[defName] = weaponDef;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 檢查砲塔定義是否存在
        /// </summary>
        public static bool WeaponExistsInTurretDict(string defName, out ThingDef weaponDef)
        {
            weaponDef = _turrets.FirstOrDefault(t => t.defName == defName);
            return weaponDef != null;
        }

        /// <summary>
        /// 獲取能使用指定武器的機械體列表
        /// </summary>
        public static ThingDef[] UseableByListsOfMechs(ThingWithComps weapon)
        {
            return UseableByListsOfMechs(weapon?.def);
        }

        /// <summary>
        /// 只看 def 的版本，不需要先把武器實例化。
        /// </summary>
        public static ThingDef[] UseableByListsOfMechs(ThingDef weaponDef)
        {
            var compatibleMechs = new List<ThingDef>();

            if (weaponDef == null)
            {
                return compatibleMechs.ToArray();
            }

            foreach (ThingDef mechDef in _weaponUseableMechs)
            {
                if (CanMechUseWeapon(mechDef, weaponDef))
                {
                    compatibleMechs.AddDistinct(mechDef);
                }
            }

            return compatibleMechs.ToArray();
        }

        /// <summary>
        /// 檢查機械體是否可以使用指定武器
        /// </summary>
        public static bool CanMechUseWeapon(ThingDef mechDef, ThingWithComps weapon)
        {
            return CanMechUseWeapon(mechDef, weapon?.def);
        }

        /// <summary>
        /// 只看 def 的版本，判定順序與實際裝備檢查（CheckUtility.UseableInStatic）一致：
        /// 1.重型武器的種族白名單可無視武器系統與體型限制。
        /// 2.啟用武器系統篩選的機械體只認白名單（標籤／科技等級／分類），體型符合也不算支援。
        /// 3.未啟用篩選的機械體仍要通過科技等級／分類，再比對重型武器的體型門檻。
        /// </summary>
        public static bool CanMechUseWeapon(ThingDef mechDef, ThingDef weaponDef)
        {
            if (mechDef == null || weaponDef == null)
            {
                return false;
            }

            var mechExtension = mechDef.GetModExtension<MechWeaponExtension>();

            if (mechExtension == null)
            {
                return false;
            }

            var heavyExtension = weaponDef.GetModExtension<HeavyEquippableExtension>();

            if (heavyExtension?.EquippableDef != null &&
                heavyExtension.EquippableDef.EquippableByRace.NotNullAndContains(mechDef))
            {
                return true;
            }

            // 武器系統（標籤／科技等級／分類）不支援的話，體型再大也拿不起來。
            if (!mechExtension.CanUse(weaponDef))
            {
                return false;
            }

            // 啟用篩選時白名單就是授權，不再受體型門檻限制。
            if (mechExtension.EnableWeaponFilter)
            {
                return true;
            }

            return MeetsHeavyBodySizeRequirement(heavyExtension, mechDef);
        }

        /// <summary>
        /// 比對重型武器的體型門檻；掛載型武器沒有體型門檻，
        /// 只能靠種族／服裝／Hediff／基因取得，不會因為體型夠大就能裝備。
        /// </summary>
        private static bool MeetsHeavyBodySizeRequirement(HeavyEquippableExtension heavyExtension, ThingDef mechDef)
        {
            if (heavyExtension?.EquippableDef == null)
            {
                return true;
            }

            if (heavyExtension.EquippableDef.IsMountedWeapon)
            {
                return false;
            }

            float requiredSize = heavyExtension.EquippableDef.EquippableBaseBodySize;

            return mechDef.race != null && mechDef.race.baseBodySize >= requiredSize;
        }
    }
}