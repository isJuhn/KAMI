using System;
using System.Collections.Generic;
using System.Text;

namespace KAMI.Cameras
{
    public interface ICamera
    {
        public void Update(float diffX, float diffY);
    }
}
