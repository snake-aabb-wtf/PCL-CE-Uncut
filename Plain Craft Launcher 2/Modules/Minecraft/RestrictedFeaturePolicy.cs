namespace PCL;

/// <summary>
/// 地区功能限制。
/// Uncut：不再按国家或地区拦截功能，所有检查一律放行。
/// 官方实现在 PCL CE v2.15.1 的 RestrictedFeaturePolicy 中（PR #3673）。
/// </summary>
internal static class RestrictedFeaturePolicy
{
    internal static bool IsRestricted => false;

    internal static bool Check(string feature, bool showHint = true) => true;

    internal static void ThrowIfRestricted(string feature)
    {
    }
}
