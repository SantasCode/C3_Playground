using C3.Elements;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3_Playground.Preview.Model
{
    internal class Motion
    {
        private Dictionary<uint, List<Matrix>> motionFrames;

        private uint FrameIdx = 0;

        public uint BoneCount { get; init; }
        public uint FrameCount { get; init; }

        public Motion(C3Motion c3Motion, C3.Core.Matrix initialMatrix)
        {
            BoneCount = c3Motion.BoneCount;
            FrameCount = c3Motion.FrameCount;

            motionFrames = new((int)c3Motion.FrameCount);

            if (c3Motion.BoneKeyFrames != null)
            {

                foreach (var keyFrame in c3Motion.BoneKeyFrames)
                {
                    if (keyFrame.Matricies != null) 
                    {
                        motionFrames.Add(keyFrame.FrameNumber, new());
                        foreach (var m in keyFrame.Matricies)
                        {
                            var iM = new Matrix()
                            {
                                M11 = initialMatrix.M11,
                                M12 = initialMatrix.M12,
                                M13 = initialMatrix.M13,
                                M14 = initialMatrix.M14,
                                M21 = initialMatrix.M21,
                                M22 = initialMatrix.M22,
                                M23 = initialMatrix.M23,
                                M24 = initialMatrix.M24,
                                M31 = initialMatrix.M31,
                                M32 = initialMatrix.M32,
                                M33 = initialMatrix.M33,
                                M34 = initialMatrix.M34,
                                M41 = initialMatrix.M41,
                                M42 = initialMatrix.M42,
                                M43 = initialMatrix.M43,
                                M44 = initialMatrix.M44
                            };
                            var fM = new Matrix()
                            {
                                M11 = m.M11,
                                M12 = m.M12,
                                M13 = m.M13,
                                M14 = m.M14,
                                M21 = m.M21,
                                M22 = m.M22,
                                M23 = m.M23,
                                M24 = m.M24,
                                M31 = m.M31,
                                M32 = m.M32,
                                M33 = m.M33,
                                M34 = m.M34,
                                M41 = m.M41,
                                M42 = m.M42,
                                M43 = m.M43,
                                M44 = m.M44
                            };
                            var rM = Matrix.Multiply(iM, fM);
                            motionFrames[keyFrame.FrameNumber].Add(rM);

                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BoneIdx"></param>
        /// <returns>Transform matrix for the give BoneIdx and current frame</returns>
        /// <exception cref="Exception"></exception>
        public Matrix GetMatrix(uint BoneIdx)
        {
            if (motionFrames == null) throw new Exception($"No frames at idx {BoneIdx}");

            if (motionFrames.ContainsKey(FrameIdx)) return motionFrames[FrameIdx][(int)BoneIdx];
            

            //Frame doesn't exist, calculate and store.
            motionFrames[FrameIdx] = InterpolateFrame();

            return motionFrames[FrameIdx][(int)BoneIdx];
        }
        public bool NextFrame()
        {
            uint oldIdx = FrameIdx;
            FrameIdx = (FrameIdx + 1) % FrameCount;
            return oldIdx != FrameIdx;
        }

        private List<Matrix> InterpolateFrame()
        {
            //Do we interpolate between the highestFrame and the LowestFrame (does it loop?) - assuming no based on eu client.
            //Trying linear interpolation(eu client) - may not interpolate rotation correctly.

            uint? previousIdx = PreviousFrameIdx();
            uint? nextIdx = NextFrameIdx();

            if (previousIdx == null && nextIdx == null) throw new Exception($"Unable to interpolate frame idx {FrameIdx}");

            uint nIdx = nextIdx ?? default(uint);
            uint pIdx = previousIdx ?? default(uint);

            if (previousIdx == null)
                return motionFrames[nIdx];
            if (nextIdx == null)
                return motionFrames[pIdx];

            List<Matrix> result = new((int)BoneCount);
            
            float step = ((float)(FrameIdx - pIdx)) / ((float)(nIdx - pIdx));

            for (int i = 0; i < (int)BoneCount; i++)
            {
                result.Add(
                    Matrix.Add(
                        motionFrames[pIdx][i],
                        Matrix.Multiply(
                            Matrix.Subtract(
                                motionFrames[nIdx][i],
                                motionFrames[pIdx][i]
                                ),
                            step
                            )
                        )
                    );
            }
            return result;

        }
        private uint? PreviousFrameIdx()
        {
            if (motionFrames == null) return null;

            if(FrameCount <= 1) return null;

            int Idx = (int)FrameIdx - 1;

            if (Idx < 0) return null;

            for(uint i = (uint)Idx; i >= 0; i--)
            {
                if (motionFrames.ContainsKey(i))
                    return i;
            }
            return null;
        }
        private uint? NextFrameIdx()
        {
            if (motionFrames == null) return null;

            if (FrameCount <= 1) return null;

            int Idx = (int)FrameIdx + 1;

            if (Idx > motionFrames.Count) return null;

            for(uint i = (uint)Idx; i < motionFrames.Count; i++)
            {
                if (motionFrames.ContainsKey(i))
                    return i;
            }
            return null;
        }

    }
}
