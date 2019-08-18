﻿// Copyright (c) 2019 Timothé Lapetite - nebukam@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.ORCA
{

    public interface IRaycastProvider : IProcessor
    {
        NativeArray<RaycastData> outputRaycasts { get; }
        List<Raycast> lockedRaycasts { get; }
    }

    public class RaycastProvider : Processor<Unemployed>, IRaycastProvider
    {

        public AxisPair plane { get; set; } = AxisPair.XY;

        /// 
        /// Fields
        ///

        protected IRaycastGroup m_raycasts = null;
        protected List<Raycast> m_lockedRaycasts = new List<Raycast>();
        protected NativeArray<RaycastData> m_outputRaycast = new NativeArray<RaycastData>(0, Allocator.Persistent);

        /// 
        /// Properties
        ///

        public IRaycastGroup raycasts { get { return m_raycasts; } set { m_raycasts = value; } }
        public List<Raycast> lockedRaycasts { get { return m_lockedRaycasts; } }
        public NativeArray<RaycastData> outputRaycasts { get { return m_outputRaycast; } }

        protected override void InternalLock()
        {

            int count = m_raycasts == null ? 0 : m_raycasts.Count;

            m_lockedRaycasts.Clear();
            m_lockedRaycasts.Capacity = count;

            for (int i = 0; i < count; i++) { m_lockedRaycasts.Add(m_raycasts[i] as Raycast); }

        }

        protected override void Prepare(ref Unemployed job, float delta)
        {

            int raycastCount = m_lockedRaycasts.Count;

            if (m_outputRaycast.Length != raycastCount)
            {
                m_outputRaycast.Dispose();
                m_outputRaycast = new NativeArray<RaycastData>(raycastCount, Allocator.Persistent);
            }

            Raycast r;
            float3 pos, dir;

            if (plane == AxisPair.XY)
            {
                for (int i = 0; i < raycastCount; i++)
                {
                    r = m_lockedRaycasts[i];
                    pos = r.pos;
                    dir = r.m_dir;
                    m_outputRaycast[i] = new RaycastData()
                    {
                        position = float2(pos.x, pos.y), //
                        distance = r.m_distance,
                        worldPosition = pos,
                        baseline = pos.z,
                        direction = float2(dir.x, dir.y),
                        worldDir = dir,
                        layerIgnore = r.m_layerIgnore,
                        twoSided = r.twoSided
                    };
                }
            }
            else
            {
                for (int i = 0; i < raycastCount; i++)
                {
                    r = m_lockedRaycasts[i];
                    pos = r.pos;
                    dir = r.m_dir;
                    m_outputRaycast[i] = new RaycastData()
                    {
                        position = float2(pos.x, pos.z), //
                        distance = r.m_distance,
                        worldPosition = pos,
                        baseline = pos.y,
                        direction = float2(dir.x, dir.z),
                        worldDir = dir,
                        layerIgnore = r.m_layerIgnore,
                        twoSided = r.twoSided
                    };
                }
            }

        }

        protected override void Apply(ref Unemployed job)
        {

        }

        protected override void InternalUnlock()
        {

        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

            m_raycasts = null;
            m_outputRaycast.Dispose();
        }

    }
}