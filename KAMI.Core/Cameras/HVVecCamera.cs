using System;

namespace KAMI.Cameras
{
    public class HVVecCamera : ICamera
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public void Update(float diffX, float diffY)
        {
            double horAngle = Math.Atan2(Z, X);
            double vertAngle = Math.Asin(Y);
            horAngle += diffX;
            vertAngle += diffY;
            X = (float)Math.Cos(horAngle) * (float)Math.Cos(vertAngle);
            Z = (float)Math.Sin(horAngle) * (float)Math.Cos(vertAngle);
            Y = (float)Math.Sin(vertAngle);
        }
    }
}
