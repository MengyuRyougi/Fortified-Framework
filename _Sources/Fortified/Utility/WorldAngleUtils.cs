using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Fortified
{
    public static class WorldAngleUtils
    {
        public static Vector3 Position(float angle, Map map)
        {
            float theta = Mathf.Deg2Rad * angle; // 角度轉換為弧度

            // 計算方向向量
            float dx = Mathf.Sin(theta);
            float dy = -Mathf.Cos(theta);

            float x = map.Center.x;
            float y = map.Center.y;

            while (x >= 0 && x < map.Size.x && y >= 0 && y < map.Size.y)
            {
                x += dx;
                y += dy;
            }

            int edgeX = Mathf.RoundToInt(x - dx);
            int edgeY = Mathf.RoundToInt(y - dy);

            return new Vector3(edgeX, 0, edgeY);
        }
        public static float GetRangeBetweenTiles(this Map map, int tileB)
        {
            return Find.WorldGrid.ApproxDistanceInTiles(map.Tile, tileB);
        }
        /// <summary>
        /// 取得從 tileA 指向 tileB 的角度（以北為 0 度，順時針）
        /// </summary>
        public static float GetAngleBetweenTiles(this Map map, int tileB)
        {
            int tileA = map.Tile;
            // 將經緯度轉為世界座標（球面上的點）
            Vector3 posA = Find.WorldGrid.GetTileCenter(tileA);
            Vector3 posB = Find.WorldGrid.GetTileCenter(tileB);

            // 將三維向量轉為平面 2D 座標
            Vector2 flatA = new Vector2(posA.x, posA.z);
            Vector2 flatB = new Vector2(posB.x, posB.z);

            // 計算方向向量
            Vector2 dir = (flatB - flatA).normalized;

            // 計算角度：以北為 0，順時針為正
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;

            if (angle < 0)
                angle += 360f;

            return angle;
        }
    }
}