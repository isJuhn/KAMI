namespace KAMI.Core.Games
{
    internal class MockGame : IGame
    {
        public float SensModifier { get; set; }

        public void InjectionStart()
        {
        }

        public void UpdateCamera(int diffX, int diffY)
        {
        }
    }
}
