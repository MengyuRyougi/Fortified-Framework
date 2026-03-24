using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fortified
{
	public class Building_TrapReleasePawn : Building_TrapReleaseEntity
	{
		protected override int CountToSpawn => def.GetModExtension<TrapReleasePawnExtension>().countToSpawn;

		protected override PawnKindDef PawnToSpawn => def.GetModExtension<TrapReleasePawnExtension>().pawnToSpawn;
	}

	public class TrapReleasePawnExtension : DefModExtension
	{
		public PawnKindDef pawnToSpawn;

		public int countToSpawn = 1;
	}
}