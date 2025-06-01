using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

using HarmonyLib;

namespace Fortified
{
    public static class HumnalikeMechRenderingUtility //不使用
    {
		public const float headAltOffset = 0.0289575271f;

		public static void DrawHeadOverride(this HumanlikeMech mech)
		{
			if (mech.HeadGraphic == null) return;
			Quaternion rotation = Quaternion.AngleAxis(mech.Rotation.AsAngle, Vector3.up);
			Vector3 drawLoc = mech.DrawPos + mech.Extension.headOffset + rotation * mech.BaseHeadOffset() ;
			Matrix4x4 matrix = default;
			drawLoc.y += headAltOffset;
			drawLoc = drawLoc.RotatedBy(BodyAngle(mech));
			matrix.SetTRS(drawLoc, Quaternion.identity, new Vector3(mech.Extension.headGraphic.drawSize.x, 1, mech.Extension.headGraphic.drawSize.x));

			Mesh mesh;

			if ((mech.Rotation == Rot4.West && mech.HeadGraphic.WestFlipped) || (mech.Rotation == Rot4.East && mech.HeadGraphic.EastFlipped))
			{
				mesh = MeshPool.plane10Flip;
			}
			else
            {
				mesh = MeshPool.plane10;
			}


			Graphics.DrawMesh(mesh, matrix, mech.HeadGraphic.MatAt(mech.Rotation), 0);
		}

		public static Vector3 BaseHeadOffset(this HumanlikeMech mech)
		{
			Vector2 headOffset = mech.Extension.headOffset;
			switch (mech.Rotation.AsInt)
			{
				case 0:
					return new Vector3(0f, 0f, headOffset.y);
				case 1:
					return new Vector3(headOffset.x, 0f, headOffset.y);
				case 2:
					return new Vector3(0f, 0f, headOffset.y);
				case 3:
					return new Vector3(0f - headOffset.x, 0f, headOffset.y);
				default:
					Log.Error("BaseHeadOffsetAt error in " + mech);
					return Vector3.zero;
			}
		}

		public static float BodyAngle(HumanlikeMech pawn)
		{
			if (pawn.GetPosture() == PawnPosture.Standing)
			{
				return 0f;
			}
			Building_Bed building_Bed = pawn.CurrentBed();
			if (building_Bed != null && pawn.RaceProps.Humanlike)
			{
				Rot4 rotation = building_Bed.Rotation;
				rotation.AsInt += 2;
				return rotation.AsAngle;
			}
			IThingHolderWithDrawnPawn thingHolderWithDrawnPawn;
			if ((thingHolderWithDrawnPawn = (pawn.ParentHolder as IThingHolderWithDrawnPawn)) != null)
			{
				return thingHolderWithDrawnPawn.HeldPawnBodyAngle;
			}
			Pawn_CarryTracker pawn_CarryTracker;
			if (pawn.RaceProps.Humanlike && pawn.DevelopmentalStage.Baby() && (pawn_CarryTracker = (pawn.ParentHolder as Pawn_CarryTracker)) != null)
			{
				return ((pawn_CarryTracker.pawn.Rotation == Rot4.West) ? 290f : 70f) + pawn_CarryTracker.pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.None);
			}
			if (pawn.RaceProps.Humanlike)
			{
				return LayingFacing(pawn).AsAngle;
			}
			if (RotationForcedByJob(pawn).IsValid)
			{
				return 0f;
			}
			Rot4 rot = Rot4.West;
			int num = pawn.thingIDNumber % 2;
			if (num != 0)
			{
				if (num == 1)
				{
					rot = Rot4.East;
				}
			}
			else
			{
				rot = Rot4.West;
			}
			return rot.AsAngle;
		}

		public static Rot4 LayingFacing(HumanlikeMech pawn)
		{
			Rot4 result = RotationForcedByJob(pawn);
			if (result.IsValid)
			{
				return result;
			}
			PawnPosture posture = pawn.GetPosture();
			if (posture == PawnPosture.LayingOnGroundFaceUp || pawn.Deathresting)
			{
				return Rot4.South;
			}
			if (pawn.RaceProps.Humanlike)
			{
				Pawn_CarryTracker pawn_CarryTracker;
				if (pawn.DevelopmentalStage.Baby() && (pawn_CarryTracker = (pawn.ParentHolder as Pawn_CarryTracker)) != null)
				{
					if (!(pawn_CarryTracker.pawn.Rotation == Rot4.West))
					{
						return Rot4.West;
					}
					return Rot4.East;
				}
				else
				{
					if (posture.FaceUp() && pawn.CurrentBed() != null)
					{
						return Rot4.South;
					}
					switch (pawn.thingIDNumber % 4)
					{
					case 0:
						return Rot4.South;
					case 1:
						return Rot4.South;
					case 2:
						return Rot4.East;
					case 3:
						return Rot4.West;
					}
				}
			}
			else
			{
				switch (pawn.thingIDNumber % 4)
				{
				case 0:
					return Rot4.South;
				case 1:
					return Rot4.East;
				case 2:
					return Rot4.West;
				case 3:
					return Rot4.West;
				}
			}
			return Rot4.Random;
		}

		private static Rot4 RotationForcedByJob(HumanlikeMech pawn)
		{
			if (pawn.jobs != null && pawn.jobs.curDriver != null && pawn.jobs.curDriver.ForcedLayingRotation.IsValid)
			{
				return pawn.jobs.curDriver.ForcedLayingRotation;
			}
			return Rot4.Invalid;
		}
	}
}
