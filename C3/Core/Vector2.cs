namespace C3.Core
{
    public class Vector2
    {
        public float X { get; set; }
        
        public float Y { get; set; }

        public static Vector2 Zero => new Vector2 { X = 0, Y = 0 };
    }
}
