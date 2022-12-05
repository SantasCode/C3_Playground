using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace C3.Exports.GLTF.Schema
{
    internal class Camera : IndexedItem
    {
        public CameraOrthographic? Orthographic { get; set; }
        public CameraPerspective? Perspective { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required TypeEnum Type { get; set; }

        public string? Name { get; set; }

        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }



        public enum TypeEnum
        {
            perspective,
            orthographic,
        }
    }
}
