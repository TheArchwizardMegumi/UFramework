using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 六边形网格主类
    /// </summary>
    public class HexGrid : MonoBehaviour
    {
        #region 基础配置
        [Header("六边形配置")]
        [SerializeField]
        private float cellRadius = 1f;
        [SerializeField]
        private int gridRadius = 5;
        [SerializeField]
        private bool flatTopped = true;

        [Header("网格变换")]
        [SerializeField]
        [Tooltip("是否跟随物体的位置")]
        private bool followTransformPosition = true;
        [SerializeField]
        [Tooltip("是否跟随物体的旋转")]
        private bool followTransformRotation = true;
        [SerializeField]
        [Tooltip("是否跟随物体的缩放")]
        private bool followTransformScale = true;
        [SerializeField]
        private Vector3 gridCenter = Vector3.zero;
        [SerializeField]
        private Vector3 gridRotationEuler = Vector3.zero;
        [SerializeField]
        private Vector3 gridScale = Vector3.one;
        #endregion

        #region 核心数据
        // 六边形单元格字典
        private readonly Dictionary<HexCoord, HexCell> cells = new();
        // 网格边界
        private readonly List<HexCoord> boundaryCells = new();
        #endregion

        #region 属性访问器
        public int GridRadius => gridRadius;
        public float CellRadius => cellRadius;
        public bool FlatTopped => flatTopped;
        public bool FollowTransformPosition => followTransformPosition;
        public bool FollowTransformRotation => followTransformRotation;
        public bool FollowTransformScale => followTransformScale;
        public Vector3 GridCenter => gridCenter;
        public Vector3 GridRotation => gridRotationEuler;
        public Vector3 GridScale => gridScale;
        public Dictionary<HexCoord, HexCell> Cells => cells;
        #endregion

        #region Unity生命周期
        protected virtual void Awake()
        {
        }
        #endregion

        #region 网格初始化
        /// <summary>
        /// 清空所有网格数据
        /// </summary>
        public virtual void Clear()
        {
            cells.Clear();
            boundaryCells.Clear();
        }
        /// <summary>
        /// 创建默认六边形网格，覆盖原有网格
        /// </summary>
        /// <param name="newRadius">网格半径</param>
        public virtual void CreateDefaultGrid(int newRadius)
        {
            CreateDefaultGrid<object>(newRadius);
        }
        /// <summary>
        /// 创建默认六边形网格，覆盖原有网格
        /// </summary>
        /// <typeparam name="T">网格中单元格储存的对象的类型</typeparam>
        /// <param name="newRadius">网格半径</param>
        public virtual void CreateDefaultGrid<T>(int newRadius)
        {
            if (newRadius != gridRadius)
            {
                cells.Clear();
                boundaryCells.Clear();
            }
            gridRadius = newRadius;

            // 使用轴坐标(q, r)遍历六边形
            for (int q = -newRadius; q <= newRadius; q++)
            {
                int r1 = Mathf.Max(-newRadius, -q - newRadius);
                int r2 = Mathf.Min(newRadius, -q + newRadius);

                for (int r = r1; r <= r2; r++)
                {
                    HexCoord coord = new(q, r);
                    CreateCell<T>(coord);
                }
            }

            CalculateBoundary();
        }
        /// <summary>
        /// 计算边界单元格
        /// </summary>
        protected virtual void CalculateBoundary()
        {
            boundaryCells.Clear();

            foreach (var kvp in cells)
            {
                HexCoord coord = kvp.Key;
                bool isBoundary = false;

                for (int i = 0; i < 6; i++)
                {
                    HexCoord neighbor = coord.GetNeighbor(i);
                    if (!cells.ContainsKey(neighbor))
                    {
                        isBoundary = true;
                        break;
                    }
                }

                if (isBoundary)
                {
                    boundaryCells.Add(coord);
                }
            }
        }
        #endregion

        #region 坐标转换
        /// <summary>
        /// 将六边形坐标转换为世界坐标
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>世界坐标</returns>
        public Vector3 HexToWorld(HexCoord coord)
        {
            Vector3 localPos;

            if (flatTopped)
            {
                float x = CellRadius * (3f / 2f * coord.q);
                float z = CellRadius * (Mathf.Sqrt(3) / 2f * -coord.q + Mathf.Sqrt(3) * -coord.r);
                localPos = new Vector3(x, 0f, z);
            }
            else
            {
                float x = CellRadius * (Mathf.Sqrt(3) * coord.q + Mathf.Sqrt(3) / 2f * coord.r);
                float z = CellRadius * (3f / 2f * -coord.r);
                localPos = new Vector3(x, 0f, z);
            }

            // 应用网格缩放
            if (gridScale != Vector3.one)
            {
                localPos = Vector3.Scale(localPos, gridScale);
            }

            // 应用物体缩放（如果启用）
            if (followTransformScale)
            {
                localPos = Vector3.Scale(localPos, transform.lossyScale);
            }

            // 应用网格旋转
            if (gridRotationEuler.sqrMagnitude > 0.001f)
            {
                Quaternion rotation = Quaternion.Euler(gridRotationEuler);
                localPos = rotation * localPos;
            }

            // 应用物体旋转到局部坐标（如果启用）
            if (followTransformRotation)
            {
                localPos = transform.rotation * localPos;
            }

            // 计算最终的偏移（考虑 GridScale/GridRotation 和 Transform Rotation/Scale）
            Vector3 centerOffset = gridCenter;

            // 应用网格缩放到偏移
            if (gridScale != Vector3.one)
            {
                centerOffset = Vector3.Scale(centerOffset, gridScale);
            }

            // 应用物体缩放到偏移（如果启用）
            if (followTransformScale)
            {
                centerOffset = Vector3.Scale(centerOffset, transform.lossyScale);
            }

            // 应用网格旋转到偏移
            if (gridRotationEuler.sqrMagnitude > 0.001f)
            {
                Quaternion rotation = Quaternion.Euler(gridRotationEuler);
                centerOffset = rotation * centerOffset;
            }

            // 应用物体旋转到偏移（如果启用）
            if (followTransformRotation)
            {
                centerOffset = transform.rotation * centerOffset;
            }

            // 组合最终位置
            Vector3 finalPos = localPos + centerOffset;

            // 应用物体位置（如果启用）
            if (followTransformPosition)
            {
                finalPos = transform.position + finalPos;
            }

            return finalPos;
        }
        /// <summary>
        /// 将世界坐标转换为六边形坐标
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <returns>六边形坐标</returns>
        public HexCoord WorldToHex(Vector3 worldPos)
        {
            Vector3 relativePos = worldPos;

            // 移除物体位置（如果启用）
            if (followTransformPosition)
            {
                relativePos = worldPos - transform.position;
            }

            // 计算并移除偏移（考虑 GridScale/GridRotation 和 Transform Rotation/Scale）
            Vector3 centerOffset = gridCenter;

            // 应用网格缩放到偏移
            if (gridScale != Vector3.one)
            {
                centerOffset = Vector3.Scale(centerOffset, gridScale);
            }

            // 应用物体缩放到偏移（如果启用）
            if (followTransformScale)
            {
                centerOffset = Vector3.Scale(centerOffset, transform.lossyScale);
            }

            // 应用网格旋转到偏移
            if (gridRotationEuler.sqrMagnitude > 0.001f)
            {
                Quaternion rotation = Quaternion.Euler(gridRotationEuler);
                centerOffset = rotation * centerOffset;
            }

            // 应用物体旋转到偏移（如果启用）
            if (followTransformRotation)
            {
                centerOffset = transform.rotation * centerOffset;
            }

            // 移除偏移
            relativePos -= centerOffset;

            // 移除物体旋转（如果启用）
            if (followTransformRotation)
            {
                Quaternion inverseRotation = Quaternion.Inverse(transform.rotation);
                relativePos = inverseRotation * relativePos;
            }

            // 移除网格旋转
            if (gridRotationEuler.sqrMagnitude > 0.001f)
            {
                Quaternion inverseRotation = Quaternion.Inverse(Quaternion.Euler(gridRotationEuler));
                relativePos = inverseRotation * relativePos;
            }

            // 移除物体缩放（如果启用）
            if (followTransformScale)
            {
                // 使用逆缩放
                Vector3 inverseScale = transform.lossyScale;
                inverseScale = new Vector3(
                    inverseScale.x != 0 ? 1f / inverseScale.x : 1f,
                    inverseScale.y != 0 ? 1f / inverseScale.y : 1f,
                    inverseScale.z != 0 ? 1f / inverseScale.z : 1f
                );
                relativePos = Vector3.Scale(relativePos, inverseScale);
            }

            // 移除网格缩放
            if (gridScale != Vector3.one)
            {
                Vector3 inverseGridScale = new(
                    gridScale.x != 0 ? 1f / gridScale.x : 1f,
                    gridScale.y != 0 ? 1f / gridScale.y : 1f,
                    gridScale.z != 0 ? 1f / gridScale.z : 1f
                );
                relativePos = Vector3.Scale(relativePos, inverseGridScale);
            }

            float q, r;

            if (flatTopped)
            {
                q = (2f / 3f * relativePos.x) / CellRadius;
                r = -(1f / 3f * relativePos.x + Mathf.Sqrt(3) / 3f * relativePos.z) / CellRadius;
            }
            else
            {
                q = (Mathf.Sqrt(3) / 3f * relativePos.x + 1f / 3f * relativePos.z) / CellRadius;
                r = (2f / 3f * -relativePos.z) / CellRadius;
            }

            return HexCoord.Round(q, r);
        }
        #endregion

        #region 单元格操作
        /// <summary>
        /// 获取指定坐标的单元格
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>单元格，不存在返回null</returns>
        public HexCell GetCell(HexCoord coord)
        {
            if (cells.TryGetValue(coord, out HexCell cell))
            {
                return cell;
            }
            return null;
        }
        /// <summary>
        /// 获取指定坐标的单元格
        /// </summary>
        /// <returns>单元格，不存在返回null</returns>
        public HexCell GetCell(int q, int r)
        {
            return GetCell(new HexCoord(q, r));
        }
        /// <summary>
        /// 获取指定坐标的单元格
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        public bool TryGetCell(HexCoord coord, out HexCell cell) => cells.TryGetValue(coord, out cell);
        /// <summary>
        /// 获取指定坐标的单元格
        /// </summary>
        public bool TryGetCell(int q, int r, out HexCell cell) => cells.TryGetValue(new HexCoord(q, r), out cell);
        /// <summary>
        /// 获取指定范围内的所有单元格
        /// </summary>
        /// <param name="center">中心坐标</param>
        /// <param name="radius">范围半径</param>
        /// <returns>单元格列表</returns>
        public List<HexCell> GetCellsInRange(HexCoord center, int radius)
        {
            List<HexCell> result = new();

            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);

                for (int r = r1; r <= r2; r++)
                {
                    HexCoord coord = new(center.q + q, center.r + r);
                    HexCell cell = GetCell(coord);
                    if (cell != null)
                    {
                        result.Add(cell);
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// 检查坐标是否在网格内
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>是否在网格内</returns>
        public bool ContainsCell(HexCoord coord)
        {
            return cells.ContainsKey(coord);
        }
        /// <summary>
        /// 创建单个单元格
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        public virtual HexCell CreateCell(HexCoord coord)
        {
            return CreateCell<object>(coord);
        }
        /// <summary>
        /// 创建单个单元格
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        public virtual HexCell CreateCell(int q, int r)
        {
            return CreateCell<object>(new HexCoord(q, r));
        }
        /// <summary>
        /// 创建单个单元格
        /// </summary>
        /// <typeparam name="T">单元格储存的信息类型</typeparam>>
        /// <param name="coord">六边形坐标</param>
        public virtual HexCell<T> CreateCell<T>(HexCoord coord, T userData = default)
        {
            HexCell<T> cell = new(coord, $"{coord.q}, {coord.r}", userData);
            cells[coord] = cell;
            return cell;
        }
        /// <summary>
        /// 设置单个单元格
        /// </summary>
        /// <param name="coord">单元格坐标</param>
        /// <param name="newCell">新的单元格对象</param>
        public void SetCell(HexCoord coord, HexCell newCell)
        {
            cells[coord] = newCell;
        }
        /// <summary>
        /// 设置单个单元格
        /// </summary>
        /// <param name="newCell">新的单元格对象</param>
        public void SetCell(int q, int r, HexCell newCell)
        {
            SetCell(new HexCoord(q, r), newCell);
        }
        /// <summary>
        /// 获取单元格的邻居
        /// </summary>
        /// <param name="coord">六边形坐标</param>
        /// <returns>邻居列表</returns>
        public List<HexCell> GetNeighbors(HexCoord coord)
        {
            List<HexCell> neighbors = new();

            for (int i = 0; i < 6; i++)
            {
                HexCoord neighborCoord = coord.GetNeighbor(i);
                HexCell neighbor = GetCell(neighborCoord);
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }
        /// <summary>
        /// 检查两个单元格是否相邻
        /// </summary>
        /// <param name="coord1">坐标1</param>
        /// <param name="coord2">坐标2</param>
        /// <returns>是否相邻</returns>
        public bool IsAdjacent(HexCoord coord1, HexCoord coord2)
        {
            int distance = HexCoord.Distance(coord1, coord2);
            return distance == 1;
        }
        /// <summary>
        /// 计算射线与网格的交点所在的单元格
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="selectedCell">被选中的单元格<param>
        /// <returns>是否选中有效的单元格</returns>
        public bool GetCellByRay(Ray ray, out HexCell selectedCell)
        {
            RayPlaneIntersection(ray, out Vector3 intersectionWorldPos);
            selectedCell = GetCell(WorldToHex(intersectionWorldPos));
            return selectedCell != null;
        }
        #endregion

        #region 网格操作
        /// <summary>
        /// 清空网格
        /// </summary>
        public virtual void ClearGrid()
        {
            cells.Clear();
            boundaryCells.Clear();
        }
        /// <summary>
        /// 重置网格
        /// </summary>
        public virtual void ResetGrid()
        {
            ClearGrid();
            Clear();
        }
        #endregion

        #region 数学计算
        /// <summary>
        /// 计算两个坐标之间的距离
        /// </summary>
        /// <param name="coord1">坐标1</param>
        /// <param name="coord2">坐标2</param>
        /// <returns>距离</returns>
        public int GetDistance(HexCoord coord1, HexCoord coord2)
        {
            return HexCoord.Distance(coord1, coord2);
        }
        /// <summary>
        /// 获取网格平面的原点（世界坐标）
        /// </summary>
        public Vector3 GetCenterWorldPosition()
        {
            Vector3 origin = GridCenter;

            // 应用 GridScale
            if (GridScale != Vector3.one)
            {
                origin = Vector3.Scale(origin, GridScale);
            }

            // 应用 Transform Scale
            if (FollowTransformScale)
            {
                origin = Vector3.Scale(origin, transform.lossyScale);
            }

            // 应用 GridRotation
            if (GridRotation != Vector3.zero)
            {
                Quaternion gridRotation = Quaternion.Euler(GridRotation);
                origin = gridRotation * origin;
            }

            // 应用 Transform Rotation
            if (FollowTransformRotation)
            {
                origin = transform.rotation * origin;
            }

            // 应用 Transform Position
            if (FollowTransformPosition)
            {
                origin = transform.position + origin;
            }

            return origin;
        }
        /// <summary>
        /// 获取网格平面的法线方向
        /// </summary>
        public Vector3 GetGridPlaneNormal()
        {
            Vector3 normal = Vector3.up;

            // 应用 GridRotation
            if (GridRotation != Vector3.zero)
            {
                Quaternion gridRotation = Quaternion.Euler(GridRotation);
                normal = gridRotation * normal;
            }

            // 应用 Transform Rotation
            if (FollowTransformRotation)
            {
                normal = transform.rotation * normal;
            }

            return normal;
        }
        /// <summary>
        /// 计算射线与网格所在平面的交点
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="intersectionWorldPos">交点输出(世界坐标)</param>
        /// <returns>射线是否与网格平面相交</returns>
        public bool RayPlaneIntersection(Ray ray, out Vector3 intersectionWorldPos)
        {
            intersectionWorldPos = Vector3.zero;
            Vector3 planeOrigin = GetCenterWorldPosition();
            Vector3 planeNormal = GetGridPlaneNormal();

            // 计算射线方向与平面法线的点积
            float denominator = Vector3.Dot(ray.direction, planeNormal);

            // 如果分母为0，说明射线与平面平行
            if (Mathf.Abs(denominator) < Mathf.Epsilon)
            {
                return false;
            }

            // 计算射线原点到平面原点的向量
            Vector3 rayOriginToPlane = planeOrigin - ray.origin;

            // 计算射线到平面的距离
            float t = Vector3.Dot(rayOriginToPlane, planeNormal) / denominator;

            // t 必须大于0，交点才在射线前方
            if (t < 0)
            {
                return false;
            }

            // 计算交点
            intersectionWorldPos = ray.origin + ray.direction * t;
            return true;
        }
        /// <summary>
        /// 获取单元格的六个顶点的世界坐标
        /// </summary>
        /// <param name="coord">六边形中心</param>
        /// <returns>顶点数组</returns>
        public Vector3[] GetHexagonCorners(HexCoord coord)
        {
            Vector3 center = HexToWorld(coord);
            Vector3[] corners = new Vector3[6];
            float angleStep = 60f;
            float startAngle = FlatTopped ? 0f : 30f;

            // 计算旋转后的偏移方向
            Quaternion gridRotation = Quaternion.Euler(GridRotation);
            Quaternion totalRotation = gridRotation;

            if (FollowTransformRotation)
            {
                totalRotation = transform.rotation * gridRotation;
            }

            // 计算缩放（网格缩放 × 物体缩放）
            Vector3 scale = GridScale;
            if (FollowTransformScale)
            {
                scale = Vector3.Scale(scale, transform.lossyScale);
            }

            for (int i = 0; i < 6; i++)
            {
                float angle = (startAngle + i * angleStep) * Mathf.Deg2Rad;

                // 计算局部坐标系中的顶点相对于六边形中心的偏移
                float x = CellRadius * Mathf.Cos(angle);
                float z = CellRadius * Mathf.Sin(angle);

                // 应用缩放到偏移
                Vector3 offset = new(x, 0, z);
                offset = Vector3.Scale(offset, scale);

                // 应用旋转到偏移
                offset = totalRotation * offset;

                // 应用到中心点
                corners[i] = center + offset;
            }

            return corners;
        }
        private Vector3 DirectionAngleToVector(float originRotationDegree, int rotationCount)
        {
            // 获取总旋转
            Quaternion gridRotation = Quaternion.Euler(GridRotation);
            Quaternion totalRotation = gridRotation;

            if (FollowTransformRotation)
            {
                totalRotation = transform.rotation * gridRotation;
            }

            // 平顶六边形的方向角度（相对于X轴, 逆时针为正）
            // SouthEast = -30°, 按顺序依次+60°
            float angle = originRotationDegree + (int)rotationCount * 60f;

            // 计算该方向在局部坐标系中的单位向量
            Vector3 localDirection = new(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));

            // 应用总旋转
            Vector3 worldDirection = totalRotation * localDirection;

            // 归一化并返回单位向量
            return worldDirection.normalized;
        }
        /// <summary>
        /// 获取边指向方向的单位向量
        /// </summary>
        /// <param name="rotationCount">方向索引 0-5，代表从q轴正方向开始顺时针旋转的次数，每次旋转60度</param>
        /// <returns></returns>
        public Vector3 GetDirection(int rotationCount)
        {
            rotationCount %= 6;
            if (flatTopped)
            {
                return DirectionAngleToVector(-30f, rotationCount);
            }
            else
            {
                return DirectionAngleToVector(0f, rotationCount);
            }
        }
        /// <summary>
        /// 获取对角线指向方向的单位向量
        /// </summary>
        /// <param name="rotationCount">方向索引 0-5，代表从q轴正方向与r轴正方向之间的对角线开始顺时针旋转的次数，每次旋转60度</param>
        /// <returns></returns>
        public Vector3 GetDiagonal(int rotationCount)
        {
            rotationCount %= 6;
            if (flatTopped)
            {
                return DirectionAngleToVector(-60f, rotationCount);
            }
            else
            {
                return DirectionAngleToVector(-30f, rotationCount);
            }
        }
        #endregion
    }

    [Serializable]
    /// <summary>
    /// 六边形坐标系统（轴坐标 q, r）
    /// </summary>
    public struct HexCoord
    {
        public int q;
        public int r;
#pragma warning disable IDE1006 // 命名样式
        public readonly int s => -q - r; // 第三个坐标，满足 q + r + s = 0
#pragma warning restore IDE1006 // 命名样式

        private static readonly int[][] Directions = new int[6][]
        {
            new int[2] { 0, -1 }, // 平顶：北，尖顶：西北
            new int[2] { 1, -1 }, // 平顶：东北，尖顶：东北
            new int[2] { 1, 0 }, // 平顶：东南，尖顶：东, 默认为q轴正方向
            new int[2] { 0, 1 }, // 平顶：南，尖顶：东南, 默认为r轴正方向
            new int[2] { -1, 1 }, // 平顶：西南，尖顶：西南
            new int[2] { -1, 0 }, // 平顶：西北，尖顶：西
        };
        private static readonly int[][] Diagonals = new int[6][]
        {
            new int[2] { 1, -2 }, // 平顶：东北，尖顶：北
            new int[2] { 2, -1 }, // 平顶：东，尖顶：东北
            new int[2] { 1, 1 }, // 平顶：东南，尖顶：东南, 默认为q轴正方向与r轴正方向夹角
            new int[2] { -1, 2 }, // 平顶：西南，尖顶：南
            new int[2] { -2, 1 }, // 平顶：西，尖顶：西南
            new int[2] { -1, -1 }, // 平顶：西北，尖顶：西北
        };

        public HexCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        #region 坐标操作
        /// <summary>
        /// 获取邻居坐标，方向参数值为0-5，代表从r轴负方向开始顺时针旋转的次数，即默认方向下世界坐标的Z轴正方向(平顶)，每次旋转60度
        /// </summary>
        /// <param name="direction">方向索引 0-5，代表从r轴负方向开始顺时针旋转的次数，即默认方向下世界坐标的Z轴正方向(平顶)，每次旋转60度</param>
        /// <returns>邻居坐标</returns>
        public readonly HexCoord GetNeighbor(int direction)
        {
            return new HexCoord(q + Directions[direction][0], r + Directions[direction][1]);
        }
        /// <summary>
        /// 获取全部邻居坐标
        /// </summary>
        public readonly HexCoord[] GetNeighbors()
        {
            HexCoord[] neighbors = new HexCoord[6];
            for (int i = 0; i < neighbors.Length; i++)
            {
                neighbors[i] = new HexCoord(q + Directions[i][0], r + Directions[i][1]);
            }
            return neighbors;
        }
        /// <summary>
        /// 获取对角线方向坐标，方向参数值为0-5，代表从q轴正方向与r轴负方向之间靠近r轴的对角线开始顺时针旋转的次数，
        /// 即默认方向下世界坐标的Z轴正方向(尖顶)，每次旋转60度
        /// </summary>
        /// <param name="direction">方向索引 0-5，代表从q轴正方向与r轴负方向之间靠近r轴的对角线开始顺时针旋转的次数，
        /// 即默认方向下世界坐标的Z轴正方向(尖顶)，每次旋转60度</param>
        /// <returns>对角线坐标</returns>
        public readonly HexCoord GetDiagonal(int direction)
        {
            return new HexCoord(q + Diagonals[direction][0], r + Diagonals[direction][1]);
        }
        /// <summary>
        /// 计算两个坐标的距离
        /// </summary>
        public static int Distance(HexCoord a, HexCoord b)
        {
            Vector3 vecA = new(a.q, a.r, a.s);
            Vector3 vecB = new(b.q, b.r, b.s);
            return (int)((Mathf.Abs(vecA.x - vecB.x) + Mathf.Abs(vecA.y - vecB.y) + Mathf.Abs(vecA.z - vecB.z)) / 2f);
        }
        /// <summary>
        /// 四舍五入浮点坐标到整数坐标
        /// </summary>
        public static HexCoord Round(float fq, float fr)
        {
            float fs = -fq - fr;

            int q = Mathf.RoundToInt(fq);
            int r = Mathf.RoundToInt(fr);
            int s = Mathf.RoundToInt(fs);

            float qDiff = Mathf.Abs(q - fq);
            float rDiff = Mathf.Abs(r - fr);
            float sDiff = Mathf.Abs(s - fs);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                q = -r - s;
            }
            else if (rDiff > sDiff)
            {
                r = -q - s;
            }

            return new HexCoord(q, r);
        }
        #endregion

        #region 坐标转换
        /// <summary>
        /// 轴坐标转换为立方坐标
        /// </summary>
        public readonly Vector3 ToCube()
        {
            return new Vector3(q, r, s);
        }
        /// <summary>
        /// 立方坐标转换为轴坐标
        /// </summary>
        public static HexCoord FromCube(int x, int z)
        {
            return new HexCoord(x, z);
        }
        #endregion

        #region 运算符重载
        public static HexCoord operator +(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.q + b.q, a.r + b.r);
        }
        public static HexCoord operator -(HexCoord a, HexCoord b)
        {
            return new HexCoord(a.q - b.q, a.r - b.r);
        }
        public static HexCoord operator *(HexCoord a, int k)
        {
            return new HexCoord(a.q * k, a.r * k);
        }
        public static bool operator ==(HexCoord a, HexCoord b)
        {
            return a.q == b.q && a.r == b.r;
        }
        public static bool operator !=(HexCoord a, HexCoord b)
        {
            return !(a == b);
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is HexCoord other)
            {
                return this == other;
            }
            return false;
        }
        public override readonly int GetHashCode()
        {
            return (q * 397) ^ r.GetHashCode();
        }
        public override readonly string ToString()
        {
            return $"({q}, {r})";
        }
        #endregion
    }

    /// <summary>
    /// 六边形单元格数据类
    /// </summary>
    public class HexCell
    {
        public HexCoord position;
        public string id;
        public bool isActive;
        protected object userData; // 自定义数据，可存储任何类型

        private HexCell(HexCoord position)
        {
            this.position = position;
            this.isActive = true;
            this.userData = default;
        }
        public HexCell(HexCoord position, string id, object userData = default) : this(position)
        {
            this.id = id;
            this.userData = userData;
        }

        public T GetData<T>()
        {
            return userData is T data ? data : default;
        }
        public void SetData<T>(T data)
        {
            userData = data;
        }
        public override string ToString() => $"HexCell({id})";
    }
    /// <summary>
    /// 六边形单元格泛型数据类
    /// </summary>
    public class HexCell<T> : HexCell
    {
        public T Data
        {
            get => GetData<T>();
            set => SetData(value);
        }

        public HexCell(HexCoord position, string id, T userData = default) : base(position, id, userData) { }
    }
}
