using System;

namespace KAMI.Core.Cameras
{
    public class HVVecCamera : ICamera
    {
        private double m_scale;
        private double m_vertMin;
        private double m_vertMax;

        public HVVecCamera()
        {
            m_scale = 1;
            m_vertMin = -Math.PI / 2;
            m_vertMax = Math.PI / 2;
        }

        public HVVecCamera(double scale, double vertMin, double vertMax)
        {
            m_scale = scale;
            m_vertMin = vertMin;
            m_vertMax = vertMax;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public void Update(float diffX, float diffY)
        {
            double horAngle = Math.Atan2(Z, X);
            double vertAngle = Math.Asin(Y / m_scale);
            horAngle += diffX;
            vertAngle = Math.Clamp(vertAngle + diffY, m_vertMin, m_vertMax);
            X = (float)(Math.Cos(horAngle) * Math.Cos(vertAngle) * m_scale);
            Z = (float)(Math.Sin(horAngle) * Math.Cos(vertAngle) * m_scale);
            Y = (float)(Math.Sin(vertAngle) * m_scale);
        }
    }
}
