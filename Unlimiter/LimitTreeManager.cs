using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using ColossalFramework.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace TreeUnlimiter
{
    internal static class LimitTreeManager
    {
        private static void AfterTerrainUpdate(TreeManager tm, TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            unsafe
            {
                float mMin = heightArea.m_min.x;
                float single = heightArea.m_min.z;
                float mMax = heightArea.m_max.x;
                float mMax1 = heightArea.m_max.z;
                int num = Mathf.Max((int)((mMin - 8f) / 32f + 270f), 0);
                int num1 = Mathf.Max((int)((single - 8f) / 32f + 270f), 0);
                int num2 = Mathf.Min((int)((mMax + 8f) / 32f + 270f), 539);
                int num3 = Mathf.Min((int)((mMax1 + 8f) / 32f + 270f), 539);
                for (int i = num1; i <= num3; i++)
                {
                    for (int j = num; j <= num2; j++)
                    {
                        uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                        int num4 = 0;
                        while (mTreeGrid != 0)
                        {
                            Vector3 position = tm.m_trees.m_buffer[mTreeGrid].Position;
                            if (Mathf.Max(Mathf.Max(mMin - 8f - position.x, single - 8f - position.z), Mathf.Max(position.x - mMax - 8f, position.z - mMax1 - 8f)) < 0f)
                            {
                                tm.m_trees.m_buffer[mTreeGrid].AfterTerrainUpdated(mTreeGrid, mMin, single, mMax, mMax1);
                            }
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 < LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                    }
                }
            }
        }

        private static void CalculateAreaHeight(TreeManager tm, float minX, float minZ, float maxX, float maxZ, out int num, out float min, out float avg, out float max)
        {
            unsafe
            {
                    int num1 = Mathf.Max((int)((minX - 8f) / 32f + 270f), 0);
                    int num2 = Mathf.Max((int)((minZ - 8f) / 32f + 270f), 0);
                    int num3 = Mathf.Min((int)((maxX + 8f) / 32f + 270f), 539);
                    int num4 = Mathf.Min((int)((maxZ + 8f) / 32f + 270f), 539);
                    num = 0;    //OUT number of times hit.
                    min = 1024f; //OUT Min height seen.
                    avg = 0f;  //Out avg height seen .
                    max = 0f;  //Out Max height seen.
                    for (int i = num2; i <= num4; i++)
                    {
                        for (int j = num1; j <= num3; j++)
                        {
                            uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                            int num5 = 0;
                            while (mTreeGrid != 0)
                            {
                                Vector3 position = tm.m_trees.m_buffer[mTreeGrid].Position;
                                if (Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max(position.x - maxX - 8f, position.z - maxZ - 8f)) < 0f)
                                {
                                    TreeInfo info = tm.m_trees.m_buffer[mTreeGrid].Info;
                                    if (info != null)
                                    {
                                        Randomizer randomizer = new Randomizer(mTreeGrid);
                                        float mMinScale = info.m_minScale + (float)randomizer.Int32(10000) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                                        float mSize = position.y + info.m_generatedInfo.m_size.y * mMinScale * 2f;
                                        if (mSize < min)
                                        {
                                            min = mSize;
                                        }
                                        avg = avg + mSize;
                                        if (mSize > max)
                                        {
                                            max = mSize;
                                        }
                                        num = num + 1;
                                    }
                                }
                                mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                                int num6 = num5 + 1;
                                num5 = num6;
                                if (num6 < LimitTreeManager.Helper.TreeLimit )
                                {
                                    continue;
                                }
                                CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                                break;
                            }
                        }
                    }
                    if (avg != 0f)
                    {
                        avg = avg / (float)num;
                    }
            }
        }

        private static bool CalculateGroupData(TreeManager tm, int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        {
            unsafe
            {
                bool flag = false;
                if (layer != tm.m_treeLayer)
                {
                    return flag;
                }
                int num = groupX * 540 / 45;
                int num1 = groupZ * 540 / 45;
                int num2 = (groupX + 1) * 540 / 45 - 1;
                int num3 = (groupZ + 1) * 540 / 45 - 1;
                for (int i = num1; i <= num3; i++)
                {
                    for (int j = num; j <= num2; j++)
                    {
                        uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                        int num4 = 0;
                        while (mTreeGrid != 0)
                        {
                            if (tm.m_trees.m_buffer[mTreeGrid].CalculateGroupData(mTreeGrid, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays))
                            {
                                flag = true;
                            }
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 < LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                    }
                }
                return flag;
            }
        }

        private static bool CheckLimits(TreeManager tm)
        {
            ItemClass.Availability mMode = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((mMode & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
            {
                if (tm.m_treeCount >= LimitTreeManager.Helper.TreeLimit - 5)
                {
                    return false;
                }
            }
            else if ((mMode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                if (tm.m_treeCount + Singleton<PropManager>.instance.m_propCount >= 64)
                {
                    return false;
                }
            }
            else if (tm.m_treeCount >= LimitTreeManager.Helper.TreeLimit - 5)
            {
                return false;
            }
            return true;
        }

        private static void EndRenderingImpl(TreeManager tm, RenderManager.CameraInfo cameraInfo)
        {
            unsafe
            {
                FastList<RenderGroup> mRenderedGroups = Singleton<RenderManager>.instance.m_renderedGroups;
                for (int i = 0; i < mRenderedGroups.m_size; i++)
                {
                    RenderGroup mBuffer = mRenderedGroups.m_buffer[i];
                    if ((mBuffer.m_instanceMask & 1 << (tm.m_treeLayer & 31 & 31)) != 0)
                    {
                        int mX = mBuffer.m_x * 540 / 45;
                        int mZ = mBuffer.m_z * 540 / 45;
                        int num = (mBuffer.m_x + 1) * 540 / 45 - 1;
                        int mZ1 = (mBuffer.m_z + 1) * 540 / 45 - 1;
                        for (int j = mZ; j <= mZ1; j++)
                        {
                            for (int k = mX; k <= num; k++)
                            {
                                uint mTreeGrid = tm.m_treeGrid[j * 540 + k];
                                int num1 = 0;
                                while (mTreeGrid != 0)
                                {
                                    tm.m_trees.m_buffer[mTreeGrid].RenderInstance(cameraInfo, mTreeGrid, mBuffer.m_instanceMask);
                                    mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                                    int num2 = num1 + 1;
                                    num1 = num2;
                                    if (num2 < LimitTreeManager.Helper.TreeLimit)
                                    {
                                        continue;
                                    }
                                    Logger.dbgLog(string.Format("mTreeGrid = {0}  num2= {1}  limit= {2}", mTreeGrid.ToString(), num2, LimitTreeManager.Helper.TreeLimit.ToString()));
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                                    break;
                                }
                            }
                        }
                    }
                }
                int num3 = PrefabCollection<TreeInfo>.PrefabCount();
                for (int l = 0; l < num3; l++)
                {
                    TreeInfo prefab = PrefabCollection<TreeInfo>.GetPrefab((uint)l);
                    if (prefab != null && prefab.m_lodCount != 0)
                    {
                        TreeInstance.RenderLod(cameraInfo, prefab);
                    }
                }
            }
        }

        private static void FinalizeTree(TreeManager tm, uint tree, ref TreeInstance data)
        {
            unsafe
            {
                int num;
                int num1;
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor)
                {
                    num = Mathf.Clamp((data.m_posX / 16 + 32768) * 540 / 65536, 0, 539);
                    num1 = Mathf.Clamp((data.m_posZ / 16 + 32768) * 540 / 65536, 0, 539);
                }
                else
                {
                    num = Mathf.Clamp((data.m_posX + 32768) * 540 / 65536, 0, 539);
                    num1 = Mathf.Clamp((data.m_posZ + 32768) * 540 / 65536, 0, 539);
                }
                int num2 = num1 * 540 + num;
                while (!Monitor.TryEnter(tm.m_treeGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }
                try
                {
                    uint num3 = 0;
                    uint mTreeGrid = tm.m_treeGrid[num2];
                    int num4 = 0;
                    while (mTreeGrid != 0)
                    {
                        if (mTreeGrid != tree)
                        {
                            num3 = mTreeGrid;
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 <= LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                        else if (num3 == 0)
                        {
                            tm.m_treeGrid[num2] = data.m_nextGridTree;
                            break;
                        }
                        else
                        {
                            tm.m_trees.m_buffer[num3].m_nextGridTree = data.m_nextGridTree;
                            break;
                        }
                    }
                    data.m_nextGridTree = 0;
                }
                finally
                {
                    Monitor.Exit(tm.m_treeGrid);
                }
                Singleton<RenderManager>.instance.UpdateGroup(num * 45 / 540, num1 * 45 / 540, tm.m_treeLayer);
            }
        }

        private static void InitializeTree(TreeManager tm, uint tree, ref TreeInstance data, bool assetEditor)
        {
            unsafe
            {
                int num;
                int num1;
                if (assetEditor)
                {
                    num = Mathf.Clamp((data.m_posX / 16 + 32768) * 540 / 65536, 0, 539);
                    num1 = Mathf.Clamp((data.m_posZ / 16 + 32768) * 540 / 65536, 0, 539);
                }
                else
                {
                    num = Mathf.Clamp((data.m_posX + 32768) * 540 / 65536, 0, 539);
                    num1 = Mathf.Clamp((data.m_posZ + 32768) * 540 / 65536, 0, 539);
                }
                int num2 = num1 * 540 + num;
                while (!Monitor.TryEnter(tm.m_treeGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }
                try
                {
                    tm.m_trees.m_buffer[tree].m_nextGridTree = tm.m_treeGrid[num2];
                    tm.m_treeGrid[num2] = tree;
                }
                finally
                {
                    Monitor.Exit(tm.m_treeGrid);
                }
            }
        }

        private static bool OverlapQuad(TreeManager tm, Quad2 quad, float minY, float maxY,ItemClass.CollisionType collisionType, int layer, uint ignoreTree)
        {
            unsafe
            {
                Vector2 vector2 = quad.Min();
                Vector2 vector21 = quad.Max();
                int num = Mathf.Max((int)(((double)vector2.x - 8) / 32 + 270), 0);
                int num1 = Mathf.Max((int)(((double)vector2.y - 8) / 32 + 270), 0);
                int num2 = Mathf.Min((int)(((double)vector21.x + 8) / 32 + 270), 539);
                int num3 = Mathf.Min((int)(((double)vector21.y + 8) / 32 + 270), 539);
                for (int i = num1; i <= num3; i++)
                {
                    for (int j = num; j <= num2; j++)
                    {
                        uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                        int num4 = 0;
                        while (mTreeGrid != 0)
                        {
                            Vector3 position = tm.m_trees.m_buffer[mTreeGrid].Position;
                            if ((double)Mathf.Max(Mathf.Max(vector2.x - 8f - position.x, vector2.y - 8f - position.z), Mathf.Max((float)((double)position.x - (double)vector21.x - 8), (float)((double)position.z - (double)vector21.y - 8))) < 0 && tm.m_trees.m_buffer[mTreeGrid].OverlapQuad(mTreeGrid, quad, minY, maxY,collisionType))
                            {
                                return true;
                            }
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 < LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                    }
                }
                return false;
            }
        }

        private static void PopulateGroupData(TreeManager tm, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        {
            unsafe
            {
                if (layer != tm.m_treeLayer)
                {
                    return;
                }
                int num = groupX * 540 / 45;
                int num1 = groupZ * 540 / 45;
                int num2 = (groupX + 1) * 540 / 45 - 1;
                int num3 = (groupZ + 1) * 540 / 45 - 1;
                for (int i = num1; i <= num3; i++)
                {
                    for (int j = num; j <= num2; j++)
                    {
                        uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                        int num4 = 0;
                        while (mTreeGrid != 0)
                        {
                            tm.m_trees.m_buffer[mTreeGrid].PopulateGroupData(mTreeGrid, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 < LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                    }
                }
            }
        }

        private static bool RayCast(TreeManager tm, Segment3 ray, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Layer itemLayers, TreeInstance.Flags ignoreFlags, out Vector3 hit, out uint treeIndex)
        {
            unsafe
            {
                int num;
                int num1;
                int num2;
                int num3;
                int num4;
                int num5;
                float single;
                float single1;
                Bounds bound = new Bounds(new Vector3(0f, 512f, 0f), new Vector3(17280f, 1152f, 17280f));
                if (ray.Clip(bound))
                {
                    Vector3 vector3 = ray.b - ray.a;
                    int num6 = (int)((double)ray.a.x / 32 + 270);
                    int num7 = (int)((double)ray.a.z / 32 + 270);
                    int num8 = (int)((double)ray.b.x / 32 + 270);
                    int num9 = (int)((double)ray.b.z / 32 + 270);
                    float single2 = Mathf.Abs(vector3.x);
                    float single3 = Mathf.Abs(vector3.z);
                    if ((double)single2 >= (double)single3)
                    {
                        num = ((double)vector3.x <= 0 ? -1 : 1);
                        num1 = 0;
                        if ((double)single2 != 0)
                        {
                            vector3 = vector3 * (32f / single2);
                        }
                    }
                    else
                    {
                        num = 0;
                        num1 = ((double)vector3.z <= 0 ? -1 : 1);
                        if ((double)single3 != 0)
                        {
                            vector3 = vector3 * (32f / single3);
                        }
                    }
                    float single4 = 2f;
                    float single5 = 10000f;
                    treeIndex = 0;
                    Vector3 vector31 = ray.a;
                    Vector3 vector32 = ray.a;
                    int num10 = num6;
                    int num11 = num7;
                    do
                    {
                        Vector3 vector33 = vector32 + vector3;
                        if (num != 0)
                        {
                            num2 = ((num10 != num6 || num <= 0) && (num10 != num8 || num >= 0) ? Mathf.Max(num10, 0) : Mathf.Max((int)(((double)vector33.x - 72) / 32 + 270), 0));
                            num3 = ((num10 != num6 || num >= 0) && (num10 != num8 || num <= 0) ? Mathf.Min(num10, 539) : Mathf.Min((int)(((double)vector33.x + 72) / 32 + 270), 539));
                            num4 = Mathf.Max((int)(((double)Mathf.Min(vector31.z, vector33.z) - 72) / 32 + 270), 0);
                            num5 = Mathf.Min((int)(((double)Mathf.Max(vector31.z, vector33.z) + 72) / 32 + 270), 539);
                        }
                        else
                        {
                            num4 = ((num11 != num7 || num1 <= 0) && (num11 != num9 || num1 >= 0) ? Mathf.Max(num11, 0) : Mathf.Max((int)(((double)vector33.z - 72) / 32 + 270), 0));
                            num5 = ((num11 != num7 || num1 >= 0) && (num11 != num9 || num1 <= 0) ? Mathf.Min(num11, 539) : Mathf.Min((int)(((double)vector33.z + 72) / 32 + 270), 539));
                            num2 = Mathf.Max((int)(((double)Mathf.Min(vector31.x, vector33.x) - 72) / 32 + 270), 0);
                            num3 = Mathf.Min((int)(((double)Mathf.Max(vector31.x, vector33.x) + 72) / 32 + 270), 539);
                        }
                        for (int i = num4; i <= num5; i++)
                        {
                            for (int j = num2; j <= num3; j++)
                            {
                                uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                                int num12 = 0;
                                while (mTreeGrid != 0)
                                {
                                    if ((tm.m_trees.m_buffer[mTreeGrid].m_flags & (ushort)ignoreFlags) == 0 && (double)ray.DistanceSqr(tm.m_trees.m_buffer[mTreeGrid].Position) < 2500)
                                    {
                                        TreeInfo info = tm.m_trees.m_buffer[mTreeGrid].Info;
                                        if ((service == ItemClass.Service.None || info.m_class.m_service == service) && (subService == ItemClass.SubService.None || info.m_class.m_subService == subService) && (itemLayers == ItemClass.Layer.None || (info.m_class.m_layer & itemLayers) != ItemClass.Layer.None) && tm.m_trees.m_buffer[mTreeGrid].RayCast(mTreeGrid, ray, out single, out single1) && ((double)single < (double)single4 - 9.99999974737875E-05 || (double)single < (double)single4 + 9.99999974737875E-05 && (double)single1 < (double)single5))
                                        {
                                            single4 = single;
                                            single5 = single1;
                                            treeIndex = mTreeGrid;
                                        }
                                    }
                                    mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                                    int num13 = num12 + 1;
                                    num12 = num13;
                                    if (num13 <= LimitTreeManager.Helper.TreeLimit)
                                    {
                                        continue;
                                    }
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                                    break;
                                }
                            }
                        }
                        vector31 = vector32;
                        vector32 = vector33;
                        num10 = num10 + num;
                        num11 = num11 + num1;
                    }
                    while ((num10 <= num8 || num <= 0) && (num10 >= num8 || num >= 0) && (num11 <= num9 || num1 <= 0) && (num11 >= num9 || num1 >= 0));
                    if (single4 != 2f)
                    {
                        hit = ray.Position(single4);
                        return true;
                    }
                }
                hit = Vector3.zero;
                treeIndex = 0;
                return false;
            }
        }

        private static void ReleaseTreeImplementation(TreeManager tm, uint tree, ref TreeInstance data)
        {
            if (data.m_flags != 0)
            {
                InstanceID instanceID = new InstanceID()
                {
                    Tree = tree
                };
                Singleton<InstanceManager>.instance.ReleaseInstance(instanceID);
                data.m_flags = (ushort)(data.m_flags | 2);
                data.UpdateTree(tree);
                data.m_flags = 0;
                try
                {
                    LimitTreeManager.FinalizeTree(tm, tree, ref data);
                }
                catch (Exception exception1)
                {
                    Exception exception = exception1;
                    object[] objArray = new object[] { tree, tm.m_trees.m_size, LimitTreeManager.Helper.TreeLimit, LimitTreeManager.Helper.UseModifiedTreeCap };
                    Logger.dbgLog(string.Format(" FinalizeTree exception: Releasing {0} {1} {2} {3}", objArray), exception1, true);
                }
                tm.m_trees.ReleaseItem(tree);
                tm.m_treeCount = (int)(tm.m_trees.ItemCount() - 1);
            }
        }

        private static float SampleSmoothHeight(TreeManager tm, Vector3 worldPos)
        {
            unsafe
            {
                float single = 0f;
                int num = Mathf.Max((int)(((double)worldPos.x - 32) / 32 + 270), 0);
                int num1 = Mathf.Max((int)(((double)worldPos.z - 32) / 32 + 270), 0);
                int num2 = Mathf.Min((int)(((double)worldPos.x + 32) / 32 + 270), 539);
                int num3 = Mathf.Min((int)(((double)worldPos.z + 32) / 32 + 270), 539);
                for (int i = num1; i <= num3; i++)
                {
                    for (int j = num; j <= num2; j++)
                    {
                        uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                        int num4 = 0;
                        while (mTreeGrid != 0)
                        {
                            if (tm.m_trees.m_buffer[mTreeGrid].GrowState != 0)
                            {
                                Vector3 position = tm.m_trees.m_buffer[mTreeGrid].Position;
                                Vector3 vector3 = worldPos - position;
                                float single1 = 1024f;
                                float single2 = (float)((double)vector3.x * (double)vector3.x + (double)vector3.z * (double)vector3.z);
                                if ((double)single2 < (double)single1)
                                {
                                    TreeInfo info = tm.m_trees.m_buffer[mTreeGrid].Info;
                                    float single3 = MathUtils.SmoothClamp01(1f - Mathf.Sqrt(single2 / single1));
                                    float mSize = position.y + info.m_generatedInfo.m_size.y * 1.25f * single3;
                                    if ((double)mSize > (double)single)
                                    {
                                        single = mSize;
                                    }
                                }
                            }
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 < LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                    }
                }
                return single;
            }
        }

        private static void TerrainUpdated(TreeManager tm, TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            unsafe
            {
                float mMin = surfaceArea.m_min.x;
                float single = surfaceArea.m_min.z;
                float mMax = surfaceArea.m_max.x;
                float mMax1 = surfaceArea.m_max.z;
                int num = Mathf.Max((int)(((double)mMin - 8) / 32 + 270), 0);
                int num1 = Mathf.Max((int)(((double)single - 8) / 32 + 270), 0);
                int num2 = Mathf.Min((int)(((double)mMax + 8) / 32 + 270), 539);
                int num3 = Mathf.Min((int)(((double)mMax1 + 8) / 32 + 270), 539);
                for (int i = num1; i <= num3; i++)
                {
                    for (int j = num; j <= num2; j++)
                    {
                        uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                        int num4 = 0;
                        while (mTreeGrid != 0)
                        {
                            Vector3 position = tm.m_trees.m_buffer[mTreeGrid].Position;
                            if ((double)Mathf.Max(Mathf.Max(mMin - 8f - position.x, single - 8f - position.z), Mathf.Max((float)((double)position.x - (double)mMax - 8), (float)((double)position.z - (double)mMax1 - 8))) < 0)
                            {
                                tm.m_trees.m_buffer[mTreeGrid].TerrainUpdated(mTreeGrid, mMin, single, mMax, mMax1);
                            }
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 < LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                    }
                }
            }
        }


        private static void UpdateData(TreeManager tm, SimulationManager.UpdateMode mode)
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("TreeManager.UpdateData");
            if (Mod.DEBUG_LOG_ON){Logger.dbgLog(" UpdateData() calling Ensure Init");}
            LimitTreeManager.Helper.EnsureInit(3);
            
            for (int i = 1; i < LimitTreeManager.Helper.TreeLimit; i++)
            {
                if (tm.m_trees.m_buffer[i].m_flags != 0 && tm.m_trees.m_buffer[i].Info == null)
                {
                    tm.ReleaseTree((uint)i);
                }
            }
            int num = PrefabCollection<TreeInfo>.PrefabCount();
            int num1 = 1;
            while (num1 * num1 < num)
            {
                num1++;
            }
            for (int j = 0; j < num; j++)
            {
                TreeInfo prefab = PrefabCollection<TreeInfo>.GetPrefab((uint)j);
                if (prefab != null)
                {
                    prefab.SetRenderParameters(j, num1);
                }
            }
            ColossalFramework.Threading.ThreadHelper.dispatcher.Dispatch(() => {
                tm.GetType().GetField("m_lastShadowRotation", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tm, new Quaternion());
                tm.GetType().GetField("m_lastCameraRotation", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tm, new Quaternion());
            });
            tm.m_infoCount = num;
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

        private static void UpdateTree(TreeManager tm, uint tree)
        {
            unsafe
            {
                tm.m_updatedTrees[tree >> 6] = tm.m_updatedTrees[tree >> 6] | (ulong)1L << (int)(tree & 63);
                tm.m_treesUpdated = true;
            }
        }

        private static void UpdateTrees(TreeManager tm, float minX, float minZ, float maxX, float maxZ)
        {
            unsafe
            {
                int num = Mathf.Max((int)(((double)minX - 8) / 32 + 270), 0);
                int num1 = Mathf.Max((int)(((double)minZ - 8) / 32 + 270), 0);
                int num2 = Mathf.Min((int)(((double)maxX + 8) / 32 + 270), 539);
                int num3 = Mathf.Min((int)(((double)maxZ + 8) / 32 + 270), 539);
                for (int i = num1; i <= num3; i++)
                {
                    for (int j = num; j <= num2; j++)
                    {
                        uint mTreeGrid = tm.m_treeGrid[i * 540 + j];
                        int num4 = 0;
                        while (mTreeGrid != 0)
                        {
                            Vector3 position = tm.m_trees.m_buffer[mTreeGrid].Position;
                            if ((double)Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max((float)((double)position.x - (double)maxX - 8), (float)((double)position.z - (double)maxZ - 8))) < 0)
                            {
                                tm.m_updatedTrees[mTreeGrid >> 6] = tm.m_updatedTrees[mTreeGrid >> 6] | (ulong)1ul << (int)(mTreeGrid & 63);
                                tm.m_treesUpdated = true;
                            }
                            mTreeGrid = tm.m_trees.m_buffer[mTreeGrid].m_nextGridTree;
                            int num5 = num4 + 1;
                            num4 = num5;
                            if (num5 < LimitTreeManager.Helper.TreeLimit)
                            {
                                continue;
                            }
                            CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat("Invalid list detected!\n", Environment.StackTrace));
                            break;
                        }
                    }
                }
            }
        }

        internal static class CustomSerializer
        {
            internal static bool Deserialize()
            {
                unsafe
                {
                    if (Mod.DEBUG_LOG_ON){Logger.dbgLog(string.Concat(" treelimit = ", Helper.TreeLimit.ToString()));}
//9-25-2015         if (Mod.DEBUG_LOG_ON){Debug.Log("[TreeUnlimiter::CustomSerializer::Deserialize()] calling Ensure Init");}
                    LimitTreeManager.Helper.EnsureInit(2);
                    if (!LimitTreeManager.Helper.UseModifiedTreeCap) { return false; }

                    byte[] numArray = null;
                    if (!Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue("mabako/unlimiter", out numArray))
                    {
                        Logger.dbgLog(" No extra data saved or found with this savegame or map.");
                        return false;
                    }
                    if (Mod.DEBUG_LOG_ON)
                    {
                        object[] length = new object[] { (int)numArray.Length };
                        Logger.dbgLog(string.Format("We have {0} bytes of extra trees", length));
                    }
                    if ((int)numArray.Length < 2 || (int)numArray.Length % 2 != 0)
                    {
                        Logger.dbgLog(" Invalid chunk size");
                        return false;
                    }
                    TreeInstance[] mBuffer = Singleton<TreeManager>.instance.m_trees.m_buffer;
                    ushort[] numArray1 = new ushort[(int)numArray.Length / 2];
                    Buffer.BlockCopy(numArray, 0, numArray1, 0, (int)numArray.Length);
                    uint num = 0;
                    uint num1 = num;
                    ushort versionnum = numArray1[num1];
                    if (versionnum != 1 & versionnum != 2 & versionnum !=3)
                    {
                        object[] objArray = new object[] { numArray1[0], versionnum, numArray[0], numArray[1] };
                        Logger.dbgLog(string.Format(" Wrong version ({0}|{1}|{2},{3}).", objArray));
                        return false;
                    }
                    int numStoredTrees = 0;
                    int headerTreeCount = 0;
                    ushort fileflags = 0;

                    if (versionnum == 1)
                    {
                        num = num1 + 1; //adjust for version header.

                        if (Mod.DEBUG_LOG_ON) { Logger.dbgLog("save format - version 1 detected."); }
                        numStoredTrees = Mod.FormatVersion1NumOfTrees;
                    }
                    //handles new version where we've stored the number of stored trees in the save.
                    if(versionnum > 1)
                    {
                        num = num1 + 3; //adjust for version header + saved array size(v2).
                        if (versionnum == 3)
                        { num = num1 + 10; } //adjust for version+full_v3_header

                        if (Mod.DEBUG_LOG_ON) { Logger.dbgLog(string.Format("save format - version {0} detected.",versionnum.ToString())); }

                        numStoredTrees = (numArray1[1] << 16) | (numArray1[2] & 0xffff);
                        if (Mod.DEBUG_LOG_ON) { Logger.dbgLog("stored array count = " + numStoredTrees.ToString()); }
                        if (numStoredTrees <= 0 | numStoredTrees > 2097152)
                        { 
                            Logger.dbgLog(" *Warning* - Aborting deserialize; storedTreeArray <= 0 or > 2097152");
                            return false;
                        }
                        if (numStoredTrees > LimitTreeManager.Helper.TreeLimit)
                        {
                            Logger.dbgLog(string.Format("** WARNING ** Number of trees in file is greater then scaled value, we will only load as many trees ({0}) as will fit in currently scaled limit of {1}.",
                                numStoredTrees.ToString(),LimitTreeManager.Helper.TreeLimit.ToString()));

                            numStoredTrees = LimitTreeManager.Helper.TreeLimit;
                        }
                        //get treecount from header in v3+
                        if (versionnum > 2) 
                        { 
                            headerTreeCount = (numArray1[3] << 16) | (numArray1[4] & 0xffff);
                            fileflags = numArray1[9];
                            if (Mod.DEBUG_LOG_ON) { Logger.dbgLog("stored treecount = " + headerTreeCount.ToString()); }
                        }
                    }

                    int num3 = 0; //holds our number of processed trees
                    bool flgPacked = false;
                    if ((fileflags & (ushort)Helper.SaveFlags.packed) != (ushort)Helper.SaveFlags.packed)
                    {
                        if (Mod.DEBUG_LOG_ON) { Logger.dbgLog("Stored data is not packed. reading " + (numStoredTrees - Mod.DEFAULT_TREE_COUNT ).ToString() + " number of objects"); }
                        for (int i = 262144; i < numStoredTrees; i++) //start at the top thier limit.
                        {
                            uint num4 = num;
                            try
                            {
                                uint num5 = num;
                                num = num5 + 1;
                                mBuffer[i].m_flags = numArray1[num5];
                                if (mBuffer[i].m_flags != 0)
                                {
                                    uint num6 = num;
                                    num = num6 + 1;
                                    mBuffer[i].m_infoIndex = numArray1[num6];
                                    uint num7 = num;
                                    num = num7 + 1;
                                    mBuffer[i].m_posX = (short)numArray1[num7];
                                    //mBuffer[i].m_posY = 0; // we do later for entire buffer instead of here.
                                    uint num8 = num;
                                    num = num8 + 1;
                                    mBuffer[i].m_posZ = (short)numArray1[num8];
                                    num3++;
                                }
                                if ((ulong)num == (ulong)((int)numArray1.Length))
                                {
                                    break;
                                }
                            }
                            catch (Exception exception1)
                            {
                                Exception exception = exception1;
                                object[] objArray1 = new object[] { i, num4, (int)numArray1.Length };
                                Logger.dbgLog(string.Format("Error - While fetching tree {0} in pos {1} of {2}", objArray1), exception1, true);
                                throw exception;
                            }
                        }
                    }
                    else //packed version.
                    {
                        flgPacked = true;
                        if (Mod.DEBUG_LOG_ON)
                        { Logger.dbgLog("Stored data is packed. reading " + headerTreeCount.ToString() + " number of objects"); }
                        if (headerTreeCount > (Helper.TreeLimit - Mod.DEFAULT_TREE_COUNT))
                        {
                            headerTreeCount = (Helper.TreeLimit - Mod.DEFAULT_TREE_COUNT);
                            Logger.dbgLog(string.Format("**Warning** Stored treecount is greater then exisiting scaled arraysize, will only load {0} number of trees", headerTreeCount.ToString()));  
                        }
                        int i = Mod.DEFAULT_TREE_COUNT;  //start adding at location 262144.
                        int j = 0;
                        for (j = 0; j < headerTreeCount ; j++) //use our stored treecount limit.
                        {
                            uint num4 = num; //for error logging
                            try
                            {
                                uint num5 = num; //store current array index.
                                num = num5 + 1; //bump master arrayindex tracker.
                                mBuffer[i].m_flags = numArray1[num5];
                                if (mBuffer[i].m_flags != 0)
                                {
                                    uint num6 = num; //store current master.
                                    num = num6 + 1; //bumper master up 
                                    mBuffer[i].m_infoIndex = numArray1[num6];
                                    uint num7 = num; //store current master.
                                    num = num7 + 1; //bump master up. 
                                    mBuffer[i].m_posX = (short)numArray1[num7];
                                    //mBuffer[i].m_posY = 0; // we do later for entire buffer instead of here.
                                    uint num8 = num; //store current master
                                    num = num8 + 1;  //bump master
                                    mBuffer[i].m_posZ = (short)numArray1[num8];
                                    num3++;  //data tracker
                                }
                                //needed cause we 'for' on j, might as well stop if we're at the limit of the array.
                                if ((ulong)num == (ulong)((int)numArray1.Length))
                                {
                                    break;
                                }
                                i++;  //increment our treemanager buffer index.
                            }
                            catch (Exception exception1)
                            {
                                Exception exception = exception1;
                                object[] objArray1 = new object[] { i.ToString(), num4.ToString(), (int)numArray1.Length,j.ToString() };
                                Logger.dbgLog(string.Format("Error - While fetching packed tree i={0} j={4} in array pos {1} of {2}", objArray1), exception1, true);
                                throw exception;
                            }
                        }
                    }
                    object[] treeLimit1;
                    if (!flgPacked)
                    { treeLimit1 = new object[] { num3, (numStoredTrees - Mod.DEFAULT_TREE_COUNT),(numStoredTrees - Mod.DEFAULT_TREE_COUNT) }; }
                    else
                    { treeLimit1 = new object[] { num3, headerTreeCount ,(numStoredTrees - Mod.DEFAULT_TREE_COUNT) }; }
                    Logger.dbgLog(string.Format(" Loaded {0} trees of {1} (out of {2} possible in extra range)", treeLimit1));
                    return true;
                }
            }


            internal static void Serialize()
            {
                if (!LimitTreeManager.Helper.UseModifiedTreeCap)
                {
                    return;
                }
                /* Insert possible enhancement here. Should we not check here first if the m_trees.m_buffer.length 
                 * is as large as we're about to assume it is?  I mean 99% of the time it should be.
                 * but if something before us blows up and Loader.OnCreated() never runs, and somehow redirects
                 * are still active... this will bomb out with an exception...it will not do damage persay but
                 * why not just prevent it and let the game save the first 262144.
                 */
                
                if (Mod.DEBUG_LOG_ON) { Logger.dbgLog(string.Concat(" treelimit = ", LimitTreeManager.Helper.TreeLimit.ToString() , " buffersize=" , Singleton<TreeManager>.instance.m_trees.m_size.ToString())); }

                TreeInstance[] mBuffer = Singleton<TreeManager>.instance.m_trees.m_buffer;
                if (Loader.LastSaveList == null)
                {
                    if (Mod.DEBUG_LOG_ON) { Logger.dbgLog("Obtaining fresh PackedList"); }
                    Loader.LastSaveList = Packer.GetPackedList();
                }
                if (mBuffer.Length <= Mod.DEFAULT_TREE_COUNT || Loader.LastSaveList.Count <= Mod.DEFAULT_TREE_COUNT)
                { 
                    Logger.dbgLog("No extra tree data to save."); 
                    Loader.LastSaveUsedPacking = false;
                    return; 
                }
                Loader.LastSaveUsedPacking = true;

                List<ushort> nums = new List<ushort>();
                nums.Add(Mod.CurrentFormatVersion); //this is our internal save format version #
                
                //now add our present 32bit array size as 2 ushorts.
                uint orgint = (uint)LimitTreeManager.Helper.TreeLimit;
                ushort firsthalf = (ushort) (orgint >> 16); //right shift 16
                ushort secondhalf = (ushort)(orgint & 0xffff); //and 16bits  
                // now store them in entries 1 and 2.
                nums.Add(firsthalf);    //int32-1 arraysize at time of save.
                nums.Add(secondhalf);   //get these back later by (first <<16) | (second & 0xffff) 
                nums.Add(0);   //int32-2 actual treecount 
                nums.Add(0);   //int32-2 actual treecount
                nums.Add(0);    //int32-3 reserved for future use.
                nums.Add(0);   //int32-3 reserved for future use.
                nums.Add(0);    //int32-4 reserved for future use.
                nums.Add(0);   //int32-4 reserved for future use.
                nums.Add(0);   //ushort Flags reserved for furture use. 

                int num = 0;

                
                //orignal
                if (Loader.LastSaveUsedPacking == false | Loader.LastSaveList == null)
                {
                    if (Mod.DEBUG_LOG_ON && Mod.DEBUG_LOG_LEVEL > 1) { Logger.dbgLog("Start using custom seralizer.(no packing)"); }

                    for (int i = Mod.DEFAULT_TREE_COUNT; i < LimitTreeManager.Helper.TreeLimit; i++) //from top of their range to ours.
                    {
                        TreeInstance treeInstance = mBuffer[i];
                        nums.Add(treeInstance.m_flags);
                        if (treeInstance.m_flags != 0)
                        {
                            nums.Add(treeInstance.m_infoIndex);
                            nums.Add((ushort)treeInstance.m_posX);
                            nums.Add((ushort)treeInstance.m_posZ);
                            num++;
                        }
                    }
                }
                else //use packing
                {
                    if (Mod.DEBUG_LOG_ON && Mod.DEBUG_LOG_LEVEL > 1) { Logger.dbgLog("Start using custom seralizer.(packing)"); }

                    for (int i = Mod.DEFAULT_TREE_COUNT; i < Loader.LastSaveList.Count; i++) //from top of thier range to ours.
                    {
                        TreeInstance treeInstance = mBuffer[Loader.LastSaveList[i]];
                        nums.Add(treeInstance.m_flags);
                        if (treeInstance.m_flags != 0)
                        {
                            nums.Add(treeInstance.m_infoIndex);
                            nums.Add((ushort)treeInstance.m_posX);
                            nums.Add((ushort)treeInstance.m_posZ);
                            num++;
                        }
                    }
                }
                //save actual tree count to header.
                nums[3] = (ushort)(num >> 16);
                nums[4] = (ushort)(num & 0xffff);
                nums[9] = 0; //options
                if (Loader.LastSaveUsedPacking) //add packed flag.
                { nums[9] = (ushort)(nums[9] | (ushort)Helper.SaveFlags.packed); }
                object[] treeLimit = new object[] { num, LimitTreeManager.Helper.TreeLimit - Mod.DEFAULT_TREE_COUNT, nums.Count * 2, Mod.CurrentFormatVersion.ToString() };
                Logger.dbgLog(string.Format("Saving {0} of {1} in extra trees range, size in savegame approx: {2} bytes, saveformatverion:{3}", treeLimit));
                Singleton<SimulationManager>.instance.m_serializableDataStorage["mabako/unlimiter"] = nums.SelectMany<ushort, byte>((ushort v) => BitConverter.GetBytes(v)).ToArray<byte>();
            }
        }

        internal class Data
        {
            public Data()
            {
            }

            private static void Deserialize(TreeManager.Data data, DataSerializer s)
            {
                short num;
                short num1;
//9-25-2015     if (Mod.DEBUG_LOG_ON){Debug.Log("[TreeUnlimiter::LimitTreeManager::Data:Deserialize()] calling Ensure Init");}
                LimitTreeManager.Helper.EnsureInit(1);
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "TreeManager");
                TreeManager treeManager = Singleton<TreeManager>.instance;
                TreeInstance[] mBuffer = treeManager.m_trees.m_buffer;
                if (Mod.DEBUG_LOG_ON) { Logger.dbgLog(" mbuffersize=" + mBuffer.Length.ToString()); }
                uint[] mTreeGrid = treeManager.m_treeGrid;
                int num2 = Mod.DEFAULT_TREE_COUNT ;  //262144
                int length = (int)mTreeGrid.Length;
                treeManager.m_trees.ClearUnused();
                SimulationManager.UpdateMode mUpdateMode = Singleton<SimulationManager>.instance.m_metaData.m_updateMode;
                if (Mod.DEBUG_LOG_ON) { Logger.dbgLog(string.Concat(" mUpdatemode =", mUpdateMode.ToString())); }
                bool flag = (mUpdateMode == SimulationManager.UpdateMode.NewAsset ? true : mUpdateMode == SimulationManager.UpdateMode.LoadAsset);
                for (int i = 0; i < length; i++)
                {
                    mTreeGrid[i] = 0;
                }
                EncodedArray.UShort num3 = EncodedArray.UShort.BeginRead(s);
                for (int j = 1; j < num2; j++)
                {
                    mBuffer[j].m_flags = num3.Read();
                }
                num3.EndRead();
                PrefabCollection<TreeInfo>.BeginDeserialize(s);
                for (int k = 1; k < num2; k++)
                {
                    if (mBuffer[k].m_flags != 0)
                    {
                        mBuffer[k].m_infoIndex = (ushort)PrefabCollection<TreeInfo>.Deserialize();
                    }
                }
                PrefabCollection<TreeInfo>.EndDeserialize(s);
                EncodedArray.Short num4 = EncodedArray.Short.BeginRead(s);
                for (int l = 1; l < num2; l++)
                {
                    if (mBuffer[l].m_flags != 0)
                    {
                        num = num4.Read();
                    }
                    else
                    {
                        num = 0;
                    }
                    mBuffer[l].m_posX = num;
                }
                num4.EndRead();
                EncodedArray.Short num5 = EncodedArray.Short.BeginRead(s);
                for (int m = 1; m < num2; m++)
                {
                    if (mBuffer[m].m_flags != 0)
                    {
                        num1 = num5.Read();
                    }
                    else
                    {
                        num1 = 0;
                    }
                    mBuffer[m].m_posZ = num1;
                }
                num5.EndRead();
                //go load our data if enabled.
                if (LimitTreeManager.Helper.UseModifiedTreeCap)
                {
                    if (Mod.DEBUG_LOG_ON) { Logger.dbgLog("Using ModifiedTreeCap - Calling Custom Deserializer."); }
                    LimitTreeManager.CustomSerializer.Deserialize();
                }
                //shared
                for (int o = 1; o < LimitTreeManager.Helper.TreeLimit; o++)
                {
                    mBuffer[o].m_nextGridTree = 0;
                    mBuffer[o].m_posY = 0;
                    if (mBuffer[o].m_flags != 0)
                    {
                        LimitTreeManager.InitializeTree(treeManager, (uint)o, ref mBuffer[o], flag);
                    }
                    else
                    {
                        treeManager.m_trees.ReleaseItem((uint)o);
                    }
                }
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, "TreeManager");
            }


            
            private static void Serialize(TreeManager.Data data, DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, "TreeManager");
                //orig TreeInstance[] mBuffer = Singleton<TreeManager>.instance.m_trees.m_buffer;
                
                //orig int num = Mod.DEFAULT_TREE_COUNT ; //262144
                try
                {
                    Packer.Serialize(ref Loader.LastSaveList, ref s);
                }
                catch (Exception ex)
                { Logger.dbgLog("", ex, true); }
                //original
                /*
                EncodedArray.UShort num1 = EncodedArray.UShort.BeginWrite(s);
                for (int i = 1; i < num; i++)
                {
                    num1.Write(mBuffer[i].m_flags);
                }
                num1.EndWrite();
                try
                {
                    PrefabCollection<TreeInfo>.BeginSerialize(s);
                    for (int j = 1; j < num; j++)
                    {
                        if (mBuffer[j].m_flags != 0)
                        {
                            PrefabCollection<TreeInfo>.Serialize(mBuffer[j].m_infoIndex);
                        }
                    }
                }
                finally
                {
                    PrefabCollection<TreeInfo>.EndSerialize(s);
                }
                EncodedArray.Short num2 = EncodedArray.Short.BeginWrite(s);
                for (int k = 1; k < num; k++)
                {
                    if (mBuffer[k].m_flags != 0)
                    {
                        num2.Write(mBuffer[k].m_posX);
                    }
                }
                num2.EndWrite();
                EncodedArray.Short num3 = EncodedArray.Short.BeginWrite(s);
                for (int l = 1; l < num; l++)
                {
                    if (mBuffer[l].m_flags != 0)
                    {
                        num3.Write(mBuffer[l].m_posZ);
                    }
                }
                num3.EndWrite();
                */ 
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, "TreeManager");
                if (Mod.DEBUG_LOG_ON && Mod.DEBUG_LOG_LEVEL > 1) { Logger.dbgLog("replaced seralizer completed."); }
                if (Loader.LastSaveList != null)
                {
                    if (Mod.DEBUG_LOG_ON && Mod.DEBUG_LOG_LEVEL > 1) { Logger.dbgLog("Cleaning up last save flags and list object."); }
                    Loader.LastSaveList.Clear();
                    Loader.LastSaveUsedPacking = false;
                    Loader.LastSaveList = null;
                }

            }
        }


        internal static class Helper
        {
            [Flags]
            internal enum SaveFlags :ushort
            {
                none = 0,
                packed = 1,
            }
            internal static int TreeLimit
            {
                get
                {
                    if (!LimitTreeManager.Helper.UseModifiedTreeCap)
                    {
                        return Mod.DEFAULT_TREE_COUNT ; // 262144
                    }
                    return Mod.SCALED_TREE_COUNT;
                    //return 1048576;  //1048576
                }
            }


            internal static bool UseModifiedTreeCap
            {
                get
                {
                    if (!Mod.IsEnabled)
                    {
                        return false;
                    }
                    SimulationManager.UpdateMode mUpdateMode = Singleton<SimulationManager>.instance.m_metaData.m_updateMode;
//9-25-2015         Mod.LastMode = mUpdateMode; //probably can ditch this now was used for debugging.

                    if (mUpdateMode == SimulationManager.UpdateMode.LoadGame || mUpdateMode == SimulationManager.UpdateMode.LoadMap 
                        || mUpdateMode == SimulationManager.UpdateMode.NewGame || mUpdateMode == SimulationManager.UpdateMode.NewMap)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            /// <summary>
            /// Called to check or initialize if needed the increasing of the Tree buffer array size.
            /// </summary>
            /// <param name="caller">The fuction that is calling this guy, 1 = deserialize(),2=custom deserialize() 3=UpdateData()</param>
            internal static void EnsureInit(byte caller)
            {
                uint num;
                object[] objArray = new object[] { (Mod.IsEnabled ? "enabled" : "disabled"), (LimitTreeManager.Helper.UseModifiedTreeCap ? "actived" : "not-actived"), caller.ToString() };
                if (Mod.DEBUG_LOG_ON) { Logger.dbgLog(string.Format("EnsureInit({2}) This mod is {0}. Tree unlimiter mode is {1}.", objArray));}

                if (!LimitTreeManager.Helper.UseModifiedTreeCap)
                {
                    if (Mod.DEBUG_LOG_ON)
                    {
                        Logger.dbgLog(string.Format(string.Concat("EnsureInit({2}) UseModifiedTreeCap = False  TreeLimit = ", LimitTreeManager.Helper.TreeLimit), objArray));
                    }
                    /* 9-25-2015    if (Mod.DEBUG_LOG_ON)
                    {
                        Debug.LogFormat(string.Concat("[TreeUnlimiter::EnsureInit({2})] LastLoadmode = ", Mod.LastMode.ToString()), objArray);
                    }
                    */
                    return;
                }

                if ((int)Singleton<TreeManager>.instance.m_trees.m_buffer.Length != LimitTreeManager.Helper.TreeLimit)
                {
                    int length = (int)Singleton<TreeManager>.instance.m_trees.m_buffer.Length;
                    string str1 = length.ToString();
                    int treeLimit = LimitTreeManager.Helper.TreeLimit;
                    Logger.dbgLog(string.Format(string.Concat("EnsureInit({2}) Updating TreeManager's ArraySize from ",
                        str1, " to ", treeLimit.ToString()), objArray));
               
        //9-25-2015 if (Mod.DEBUG_LOG_ON) {Debug.LogFormat(string.Concat("[TreeUnlimiter::EnsureInit({2})] LastLoadmode=", Mod.LastMode.ToString()), objArray); }
                    Singleton<TreeManager>.instance.m_trees = new Array32<TreeInstance>((uint)LimitTreeManager.Helper.TreeLimit); 
                    Singleton<TreeManager>.instance.m_updatedTrees = new ulong[Mod.SCALED_TREEUPDATE_COUNT]; //16384
                    Singleton<TreeManager>.instance.m_trees.CreateItem(out num);
                }
            }
        }
    }
}