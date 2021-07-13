namespace KAMI.Core.Cameras
{
    /// <summary>
    /// Horizontal Angle in Radians, Vertical Angle in Radians Camera
    /// </summary>
    public class HAVACamera : ICamera
    {
        public float Hor { get; set; }
        public float Vert { get; set; }

        public void Update(float diffX, float diffY)
        {
            Hor += diffX;
            Vert += diffY;
        }
    }
}
