using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace STARFIRE.Backend
{
    internal class HotKeys
    {
        // this is import of libraries
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int vKey); // VK Keycode
    

        public static bool IsKeyPressed(System.Windows.Forms.Keys key)
        {
            return GetAsyncKeyState((int)key) != 0;
        }
    }
}
