namespace KAMI.Core.Common
{
    public interface IMouseHandler
    {
        public (int, int) GetCenterDiff();
        public void ConfineCursor();
        public void ReleaseCursor();
    }
}
