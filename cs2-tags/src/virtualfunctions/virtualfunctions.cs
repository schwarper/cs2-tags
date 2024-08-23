using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace Tags;

public static class VirtualFunctions
{
    public readonly static MemoryFunctionVoid<nint, nint, nint, nint, nint, nint, nint, nint> UTIL_SayText2FilterFunc =
        new(GameData.GetSignature("UTIL_SayText2Filter"));

    public readonly static Action<nint, nint, nint, nint, nint, nint, nint, nint> UTIL_SayText2Filter =
        UTIL_SayText2FilterFunc.Invoke;
}