﻿using C3.Core;


namespace C3.Elements
{
    public class C3Motion
    {
        public uint BoneCount { get; set; }
        public uint FrameCount { get; set; }
        
        public string Type { get; set; }

        public uint KeyFramesCount { get; set; }
        public C3KeyFrame[] BoneKeyFrames { get; set; }

        public uint MorphCount { get; set; }
        public float[] morph { get; set; }

        
    }
}
