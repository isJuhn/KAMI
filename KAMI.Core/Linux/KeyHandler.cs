#if Linux
using System;

namespace KAMI.Core
{
    public class KeyHandler : IKeyHandler
    {
        public event KeyPressHandler OnKeyPress;

        public KeyHandler()
        {
        }

        public void SetHotKey(KeyType keyType, int? key)
        {
            throw new NotImplementedException();
        }

        public void SetEnableMouseHook(bool enabled)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
#endif
