using KAMI.Core.Cameras;
using System;

namespace KAMI.Core.Games
{
    public interface IGame
    {
        public void InjectionStart();
        public void UpdateCamera(int diffX, int diffY);
        public float SensModifier { get; set; }
    }

    public abstract class Game<TCamera> : IGame where TCamera : ICamera, new()
    {
        protected IntPtr m_ipc;
        protected TCamera m_camera;
        public float SensModifier { get; set; } = 0.003f;

        public Game(IntPtr ipc)
        {
            m_ipc = ipc;
            m_camera = new TCamera();
        }

        public virtual void InjectionStart()
        {
        }

        public abstract void UpdateCamera(int diffX, int diffY);
    }
}
