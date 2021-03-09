using System;

namespace KAMI.Cameras
{
    public class HVecVACamera : ICamera
    {
        public float HorX { get; set; }
        public float HorY { get; set; }
        public float Vert { get; set; }

        public void Update(float diffX, float diffY)
        {
            double horAngle = Math.Atan2(HorX, HorY);
            horAngle += diffX;
            Vert += diffY;
            HorX = (float)Math.Cos(horAngle);
            HorY = (float)Math.Sin(horAngle);
        }
    }
}
