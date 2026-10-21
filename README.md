# PCL-CE-Uncut

> 本仓库旨在恢复原版 PCL CE 中被删减的功能。

PCL CE（PCL Community Edition）是社区基于 PCL2 代码维护的版本。从 **v2.15.1** 开始，它加入了「中国大陆地区禁用 Minecraft 安装」的地区限制：下载、补全、安装、修复、重置、整合包安装等入口都会被拦下。

本仓库基于 PCL CE v2.15.1（commit `1a765312`）重新构建，把这道门禁恢复成「始终放行」。除此之外，代码与官方 v2.15.1 相同。

<br/>

## 恢复了什么

官方 v2.15.1 里的两个相关提交：

| 提交 | 说明 |
|---|---|
| `8cd9d374` | fix(install): 中国大陆地区禁用 Minecraft 安装功能 (#3673) |
| `1a765312` | fix(install): 读取“设备设置区域” (#3676) |

判定逻辑在 `PCL.Core/App/Region/RegionPolicy.cs`，满足任意一条就算中国大陆：

- 系统区域名是 `CN` 或 `156`
- 设备区域（OOBE 时选择，注册表 `DeviceRegion`）是 GeoID 45
- 中国时区，且区域或语言是 `zh-CN`

而且 **取不到区域信息（Unknown）也会被当作受限**，属于 fail-closed。

被拦截的入口一共 12 处调用点：

- Minecraft 客户端下载、文件补全
- Minecraft 安装、加载器安装
- 实例修复、实例重置
- 整合包安装（本地手动、在线、启动时自动）
- 启动时发现缺文件后的自动补全

提示文案是：「Minecraft 下载、补全与整合包安装功能在你所在的国家或地区不可用。」

<br/>

## 恢复原理

只改一个文件：`Plain Craft Launcher 2/Modules/Minecraft/RestrictedFeaturePolicy.cs`。

```csharp
internal static class RestrictedFeaturePolicy
{
    internal static bool IsRestricted => false;

    internal static bool Check(string feature, bool showHint = true) => true;

    internal static void ThrowIfRestricted(string feature)
    {
    }
}
```

原来这里会根据 `RegionPolicy` 的判定结果拦截，现在三个入口一律放行。上层 12 个调用点一行没动，`RegionPolicy` 本身也保留下来（不再被功能门禁使用），这样以后上游改调用点时不会牵连。

<br/>

## 与官方版本的差异

| 项 | 官方 v2.15.1 | 本仓库 |
|---|---|---|
| 地区限制 | 中国大陆 / 未知 一律拦截 | 不拦截 |
| 其余代码 | — | 相同 |
| 构建方式 | .NET 10、单文件、框架依赖 | 相同 |

没有改版本号，程序内仍显示 2.15.1——它对应的就是官方这个版本。

<br/>

## 构建

需要 .NET 10 SDK：

```bash
dotnet publish "Plain Craft Launcher 2/Plain Craft Launcher 2.csproj" \
  -c Release -p:Platform=x64 -p:DeleteExistingFiles=true -o artifact --no-self-contained
```

产物是 `Plain Craft Launcher 2.exe`，单文件，**运行需要 .NET 10 Desktop Runtime**。

推送到 `main` 会触发 `.github/workflows/build.yml`，在 Ubuntu 上交叉构建 x64 与 ARM64 两个版本。

<br/>

## 已知缺陷

**没有官方构建时注入的密钥。** 官方 CI 通过仓库 secrets 往程序里写入微软登录 client id、CurseForge API key 等，本仓库没有这些 secrets，因此：

- 微软账号登录可能不可用或受限
- CurseForge 相关功能可能受限
- 官方使用的遥测、联机等依赖同一批密钥的服务也不会带上

这几项与官方构建不等价，其余功能不受影响。

**自动更新仍然指向官方版本。** 程序里的更新检查请求的是 PCL CE 官方源，官方发布新版本后点「更新」会下载官方构建，把本仓库的修改覆盖掉。想保留 Uncut 就不要点更新，等本仓库跟进新版本。

<br/>

## 许可证与声明

- 本仓库是**第三方基于 PCL 独立进行的二次创作**，不是 PCL 官方或 PCL CE 官方产物，与原作者、PCL CE 团队无关。
- `Plain Craft Launcher 2/` 目录沿用上游的《PCL 分发有限许可》；其余目录沿用上游的 Apache-2.0。
- 关于页面里的原作者署名与赞助链接保持原样，未做改动。
- 原版限制相关功能是上游出于合规考虑所做的明确表态。本仓库不代表其立场，也不为其背书。
- 仅供个人学习与自用，按「现状」提供，不提供任何明示或暗示的担保。
