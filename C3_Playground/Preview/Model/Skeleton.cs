﻿using C3;
using C3.Elements;
using Microsoft.Xna.Framework;

namespace C3_Playground.Preview.Model
{
    internal class Skeleton
    {
        private readonly C3Phy _mesh;

        private readonly Dictionary<uint, List<(uint, float)>> _boneStore = new();


        public Dictionary<uint, List<(uint, float)>> BoneStore => _boneStore;

        public Skeleton(C3Phy mesh)
        {
            _mesh = mesh;

            for(uint i = 0; i < mesh.Vertices.Length; i++) 
            {
                foreach(var bone in mesh.Vertices[i].BoneWeights)
                {
                    if (!_boneStore.ContainsKey(bone.Joint))
                        _boneStore.Add(bone.Joint, new List<(uint, float)>());
                    
                    _boneStore[bone.Joint].Add((i, bone.Weight));
                }
            }
        }

        public bool TryGetBoneVertices(uint BoneId, out List<(uint, float)> Vertices)
        {
            if (_boneStore.ContainsKey(BoneId))
            {
                Vertices = _boneStore[BoneId];
                return true;
            }
            Vertices = new();
            return false;
        }
    }
}
