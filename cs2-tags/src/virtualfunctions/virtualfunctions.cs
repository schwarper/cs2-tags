using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace Tags;

public static class VirtualFunctions
{
    public readonly static MemoryFunctionVoid<uint, CCSPlayerController, ulong, string, string, string, string, string> UTIL_SayText2FilterFunc =
        new(GameData.GetSignature("UTIL_SayText2Filter"));

    public readonly static Action<uint, CCSPlayerController, ulong, string, string, string, string, string> UTIL_SayText2Filter =
        UTIL_SayText2FilterFunc.Invoke;
}