/*
Sparse Voxel Octrees Demo
Copyright (C) 2021 Alexander Goslin

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using SVO;
using UnityEngine;

namespace UnityTemplateProjects
{
    public class VoxelPlacer : MonoBehaviour
    {
        [Range(5, 12)]
        public int initialDepth = 5;
        public GameObject octreeObj;
        public GameObject selectionCube;
        public Material material;
        private Octree _octree;
        private RayHit? lastHit = null;
        private float depth = 5;
    
        // Start is called before the first frame update
        void Start()
        {
            _octree = new Octree();
            for (var x = -0.5f; x < 0.5f; x += 1 / Mathf.Pow(2f, initialDepth))
            {
                for (var z = -0.5f; z < 0.5f; z += 1 / Mathf.Pow(2f, initialDepth))
                {
                    _octree.SetVoxel(new Vector3(x, Mathf.PerlinNoise(x + 1000f, z + 100f) - 0.5f, z), initialDepth, Color.white, new int[0]);
                }
            }
            material.mainTexture = _octree.Apply();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                _octree.Rebuild();
                octreeObj.GetComponent<Renderer>().material.mainTexture = _octree.Apply();
            }
            
            depth += Input.mouseScrollDelta.x;
            depth = Mathf.Clamp(depth, 0, 21);

            var depthInt = Mathf.RoundToInt(depth);
            var size = 1f / Mathf.ClosestPowerOfTwo(Mathf.RoundToInt(Mathf.Exp(Mathf.Log(2) * depth)));
            
            var ray = new Ray(transform.position, transform.forward);
            if (_octree.CastRay(ray, octreeObj.transform, out var hit))
            {
                var p = (hit.objPos + Vector3.one * 0.5f + hit.faceNormal * 4.76837158203125e-7f) / size;
                p = new Vector3(Mathf.Floor(p.x) + 1, Mathf.Floor(p.y), Mathf.Floor(p.z)) * size - Vector3.one * 0.5f;
                selectionCube.transform.position =
                    octreeObj.transform.localToWorldMatrix * new Vector4(p.x, p.y, p.z, 1);
                selectionCube.transform.localScale = Vector3.one * (size * 256f);
                lastHit = hit;

                if (Input.GetMouseButton(0))
                {
                    _octree.SetVoxel(hit.objPos + hit.faceNormal * 4.76837158203125e-7f, depthInt, Color.white, new int[0]);
                    octreeObj.GetComponent<Renderer>().material.mainTexture = _octree.Apply();
                }

                if (Input.GetMouseButton(2))
                {
                    _octree.SetVoxel(hit.objPos - hit.faceNormal * 4.76837158203125e-7f, depthInt, Color.clear, new int[0]);
                    octreeObj.GetComponent<Renderer>().material.mainTexture = _octree.Apply();
                }
            }
            else
            {
                lastHit = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (lastHit != null)
            {
                Gizmos.DrawRay(lastHit.Value.worldPos, lastHit.Value.faceNormal);
                var pos = lastHit.Value.voxelObjPos + Vector3.one * lastHit.Value.voxelObjSize / 2;
                Gizmos.DrawWireCube(octreeObj.transform.localToWorldMatrix * new Vector4(pos.x, pos.y, pos.z, 1), 
                    octreeObj.transform.lossyScale * lastHit.Value.voxelObjSize);
            }
        }
    }
}