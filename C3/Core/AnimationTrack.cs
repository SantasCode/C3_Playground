using BCnEncoder.Shared.ImageFiles;
using C3.Elements;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Core
{
    public class AnimationFrame
    {
        public required int FrameNumber { get; init; }
        public required Matrix Matrix { get; init; }
    }
    public class AnimationTrack
    { 
        private readonly ILogger _logger;

        private readonly float _timePerFrame;
        //An animation track will contain the animations for each node that belong to a common track.
        //An AnimationTrack will share one gltf time sampler.

        private Dictionary<int, Queue<AnimationFrame>> nodeAnimations = new();

        public AnimationTrack(ILogger logger, float timePerFrame =  33f / 1000f) { _logger = logger; _timePerFrame = timePerFrame; }

        public void EnqueueFrame(int nodeIdx, int frameNumber, Matrix matrix)
        {
            if(!nodeAnimations.ContainsKey(nodeIdx)) 
                nodeAnimations.Add(nodeIdx, new Queue<AnimationFrame>());
            
            nodeAnimations[nodeIdx].Enqueue(new AnimationFrame() { FrameNumber = frameNumber, Matrix = matrix});
        }

        public bool DequeueFrame(int nodeIdx, int frameNumber, out Matrix frame)
        {
            frame = Matrix.Identity;
            if(!nodeAnimations.ContainsKey(nodeIdx))
            {
                _logger.LogWarning($"Node {nodeIdx} is not contained in this animation track.");
                return false;
            }

            if (nodeAnimations[nodeIdx].Peek().FrameNumber != frameNumber)
            {
                _logger.LogDebug($"Animation Track node does not have the requested frame number. nodeIdx: {nodeIdx} frame: {frameNumber}");
                return false;
            }

            frame = nodeAnimations[nodeIdx].Dequeue().Matrix;
            return true;
        }
        public bool DequeueNextFrame(int nodeIdx, out Matrix frame)
        {
            frame = Matrix.Identity;
            if (!nodeAnimations.ContainsKey(nodeIdx))
            {
                _logger.LogWarning($"Node {nodeIdx} is not contained in this animation track.");
                return false;
            }

            frame = nodeAnimations[nodeIdx].Dequeue().Matrix;
            return true;
        }

        /// <summary>
        /// Check to see if each node has a common time track
        /// </summary>
        /// <returns></returns>
        public bool IsCommonTime()
        {
            int numFrames = -1;
            int frameIdx = -1;
            foreach(var node in nodeAnimations)
            {
                if (numFrames == -1) numFrames = node.Value.Count();
                if (frameIdx == -1) frameIdx = node.Key;
                else if(!MatchingFrameTime(frameIdx, node.Key))
                {
                    _logger.LogDebug("The animated nodes in the track don't share a common time track");
                    return false;
                }

                if (numFrames != node.Value.Count())
                {
                    _logger.LogDebug("Animation Track does not have the same number of frames per node, inconsistent track time.");
                    return false;
                }
            }

            return true;
        }

        public bool MatchingFrameTime(int nodeAIdx, int nodeBIdx)
        {
            List<int> framesA = GetFrameIndices(nodeAIdx);
            List<int> framesB = GetFrameIndices(nodeBIdx);
            if (framesA.Count != framesB.Count) 
                return false;

            for(int i = 0; i < framesA.Count ;i++)
            {
                if (framesA[i] != framesB[i])
                    return false;
            }
            return true;
        }

        public List<int> GetFrameIndices(int nodeIdx)
        {
            if(nodeAnimations.TryGetValue(nodeIdx, out var nodeQueue))
            {
                return nodeQueue.ToList().Select(p=>p.FrameNumber).ToList();
            }
            return new();
        }

        public void PopulateFrames(int nodeIdx, C3Motion moti)
        {
            if (moti.BoneKeyFrames.Count() < 1)
            {
                _logger.LogError("No key frames contained in the provided motion for node {0}", nodeIdx);
                return;
            }
            
            if (moti.BoneKeyFrames[0].Matricies.Count() > 1)
            {
                _logger.LogError("More than one bone appears to be present in the provided motion for node {0}", nodeIdx);
                return;
            }
            foreach(var keyFrame in moti.BoneKeyFrames)
            {
                //Should only be a single matrix per frame.
                EnqueueFrame(nodeIdx, (int)keyFrame.FrameNumber, keyFrame.Matricies[0]);
            }

        }
        public void PopulateFrames(List<int> nodeIdx, C3Motion moti)
        {
            if (moti.BoneKeyFrames.Count() < 1)
            {
                _logger.LogError("No key frames contained in the provided motion for node {0}", nodeIdx);
                return;
            }

            if (moti.BoneCount != nodeIdx.Count)
            {
                _logger.LogError("Provided motion has more bones than nodes provided Has {0}, Provided {1}", moti.BoneCount, nodeIdx.Count);
                return;
            }
            foreach (var keyFrame in moti.BoneKeyFrames)
            {
                for (int i = 0; i < moti.BoneCount; i++)
                {
                    EnqueueFrame(nodeIdx[i], (int)keyFrame.FrameNumber, keyFrame.Matricies[i]);
                }
            }
        }

        public List<int> GetNodeIndices()
        {
            return nodeAnimations.Select(p => p.Key).ToList();
        }
        public int NodeCount => nodeAnimations.Count;
        public int FrameCount()
        {
            var node = nodeAnimations.FirstOrDefault();
            if (node.Value.Count > 0)
                return node.Value.Count;
            return 0;
        }
        public int FrameCount(int nodeIdx)
        {
            if(nodeAnimations.TryGetValue(nodeIdx, out var nodeQueue))
                return nodeQueue.Count; 

            return 0;
        }

        public Dictionary<int, int> GetCommonTimeMap()
        {
            Dictionary<int, int> result = new();
            List<int> keys = nodeAnimations.Keys.ToList();
            for(int i = 0; i < nodeAnimations.Count; i++)
            {
                for(int j = 0; j < nodeAnimations.Count; j++)
                {
                    if (MatchingFrameTime(keys[i], keys[j]))
                    {
                        result.Add(keys[i], keys[j]);
                        break;
                    }
                }
            }
            return result;
        }

        public float FrameTime => _timePerFrame;
    }
}
