/*
 *  Unity Sparse Voxel Octrees
 *  Copyright (C) 2021  Alexander Goslin
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SVO
{
    public class OctreeRendererFeature : ScriptableRendererFeature
    {
        private class OctreeRenderPass : ScriptableRenderPass
        {
            /// <summary>
            /// Prepare for execution. Called before render pass executes.
            /// </summary>
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) { }
            
            /// <summary>
            /// Executes the render pass.
            /// </summary>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get();
                var volumeId = Shader.PropertyToID("OctreeVolume");
                
                foreach (var renderer in FindObjectsOfType<Renderer>())
                {
                    if (renderer.shadowCastingMode == ShadowCastingMode.Off)
                        continue;

                    foreach (var material in renderer.materials)
                    {
                        var v = material.GetTexture(volumeId) as Texture3D;
                        if (v == null) continue;
                    }
                }

                // execution
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            
            /// <summary>
            /// Cleanup any allocated resources that were created during the execution of this render pass.
            /// </summary>
            public override void FrameCleanup(CommandBuffer cmd) { }
        }

        private OctreeRenderPass _renderPass;

        public override void Create()
        {
            _renderPass = new OctreeRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }
        
        /// <summary>
        /// Inject render pass into camera. Called once per camera.
        /// </summary>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderingData.cameraData.requiresDepthTexture = true;
            renderer.EnqueuePass(_renderPass);
        }
    }
}