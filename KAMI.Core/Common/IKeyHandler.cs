using System;

namespace KAMI.Core.Common
{
    public enum KeyType
    {
        InjectionToggle = 0xCA7,
        Mouse1,
        Mouse2,
    }

    public delegate void KeyPressHandler(object sender);
    public interface IKeyHandler : IDisposable
    {
        public event KeyPressHandler OnKeyPress;
        public void SetHotKey(KeyType keyType, int? key);
        public void SetEnableMouseHook(bool enabled);
    }
}
