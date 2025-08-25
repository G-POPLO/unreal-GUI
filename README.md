# Unreal-GUI

![GitHub Release](https://img.shields.io/github/v/release/G-POPLO/unreal-GUI)
![GitHub License](https://img.shields.io/github/license/G-POPLO/unreal-GUI)

Unreal-GUI 是一款旨在简化虚幻引擎（Unreal Engine）相关操作流程的图形用户界面应用程序。它允许用户通过直观的图形界面执行复杂的终端命令，而无需直接与命令行交互。
原开发目的是辅助用户将繁杂的命令操作转成简单的GUI操作，现正考虑将其打造成集合各种开发小工具的统一平台。

## 目录

- [功能特性](#功能特性)
- [安装说明](#安装说明)
- [使用方法](#使用方法)
- [常见问题](#常见问题)
- [贡献指南](#贡献指南)
- [许可证](#许可证)
- [第三方库引用](#第三方库引用)

## 功能特性

1. **插件编译**: 用户可以通过选择指定的插件目录和输出文件夹来执行编译操作，例如将UE5的某个版本插件升级到另一个版本。
2. **重命名项目**: 使用 [renom](https://github.com/UnrealisticDev/Renom) 进行重命名操作，需要管理员权限，否则会弹出拒绝访问 os error 5。
3. **快速访问**: 方便用户打开UE5的插件目录等。
4. **缓存清理**: 清理项目文件的缓存，包括编译生成的中间文件和临时文件。同时可以查看DDC的缓存情况等。

## 安装说明
应用程序需安装[.NET 9 Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0/runtime)，否则会提示缺少依赖项。
由于应用程序部分使用了Win32 API，因此只提供在Windows 10(1809)及以上版本上的运行版本。

1. 从 [Releases](https://github.com/G-POPLO/unreal-GUI/releases) 页面下载最新版本的 Unreal-GUI。
2. 解压下载的文件到您想要安装的目录。
3. 运行 `Unreal-GUI.exe` 启动应用程序。

## 使用方法

1. 启动 Unreal-GUI 应用程序。
2. 根据界面提示选择您需要执行的操作。
3. 按照操作向导完成相应任务。

## 贡献指南

我们欢迎任何形式的贡献！如果您想为项目贡献代码，请遵循以下步骤：

1. Fork 本仓库
2. 创建您的特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启一个 Pull Request

## 许可证

本项目采用 MIT 许可证。详情请见 [LICENSE](LICENSE.txt) 文件。

## 第三方库引用

本项目引用了以下第三方库：

- [Playwright](https://github.com/microsoft/playwright-dotnet)
- [renom](https://github.com/UnrealisticDev/Renom)
- [ModernWpf](https://github.com/ModernWpf/ModernWpf)
- [SevenZipSharp](https://github.com/squid-box/SevenZipSharp)
- [MVVM Toolkit](https://github.com/CommunityToolkit/dotnet)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)

