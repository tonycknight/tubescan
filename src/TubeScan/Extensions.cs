using System.Diagnostics;

namespace TubeScan
{
    internal static class Extensions
    {
        [DebuggerStepThrough]
        public static int ToReturnCode(this bool value) => value ? 0 : 2;

    }
}
