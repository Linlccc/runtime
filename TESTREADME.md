# 测试描述

## 变量描述
```text
<> - 包裹表示变量
[] - 包裹表示可选参数

<RepoRoot> - 当前仓库根目录
<CoreRoot> - Core_Root 目录
<corerun> - corerun 可执行文件,在 Core_Root 目录下

<OS> - 操作系统类型，如：osx、linux、windows
<Arch> - 架构类型，如：x64、arm64
<Configuration> - 编译配置类型，如：Debug、Release

<TargetFramework> - 目标框架，如：net8.0、net10.0
```

## 基础信息
### 目录结构
```text
<RepoRoot>
├── artifacts               构建输出目录
├── docs                    项目设计、开发、构建、贡献等文档说明
├── eng                     CI/CD 脚本、构建配置、代码风格检查等自动化相关内容
├── src                     源代码目录
│   ├── coreclr             CoreCLR 运行时代码（clr）
│   ├── libraries           .NET 基础类库代码（bcl）
│   ├── mono                Mono 运行时代码
│   └── tests               测试代码
│       └── build.sh/cmd    测试构建脚本
├── build.sh/cmd            构建脚本，用于构建运行时库
├── dotnet.sh/cmd           脚本文件，指向运行时库使用的 dotnet
└── global.json             版本配置文件
```

### 脚本
#### `dotnet.sh`
| 用于运行 .NET 命令的脚本，指向运行时库使用的 dotnet,可以直接当作 dotnet cli 使用

```bash
# 使用 debug 配置构建一个项目
./dotnet.sh build -c Debug project.csproj
```

#### `build.sh`

`--subset (-s)` 子集标志,主要子集
- Clr ：完整的 CoreCLR 运行时，由运行时本身和 CoreLib 组件组成。
- Libs ：所有库组件（不包括其测试）。这包括库的原生部分、引用、源程序集及其软件包和测试基础架构。
- Packs ：共享框架包、档案、捆绑包、安装程序和框架包测试。
- Host ：.NET 主机、包、托管库及其测试。
- Mono ：Mono 运行时及其 CoreLib。

构建配置
- Debug ：代码未优化。断言已启用。此配置运行速度最慢。为调试提供最佳体验。
- Checked ：（CoreCLR 运行时独占）代码已优化。断言已启用。
- Release ：优化已代码。断言已禁用。运行速度最佳，适合进行性能分析,但是影响调试体验，因为编译器优化使得调试器显示的内容（相对于源代码而言）更加难理解。

配置
- runtimeConfiguration (-rc) ：CoreCLR 构建配置
- librariesConfiguration (-lc) ：库构建配置
- hostConfiguration (-hc) ：主机构建配置
- configuration (-c) ：构建配置，上面三个未配置时使用的配置

##### 使用
```bash
# 显示帮助
./build.sh -h
# 显示所有子集
./build.sh -s help

# 恢复所有依赖项
./build.sh -s AllSubsets -r

# 使用 debug 配置 clr，release 配置 libs
./build.sh -s clr+libs -rc Debug -lc Release
```

#### `src/tests/build.sh`

see:[构建测试](https://github.com/dotnet/runtime/blob/main/docs/workflow/testing/coreclr/testing.md#building-the-tests)
see:[创建测试项目](https://github.com/dotnet/runtime/blob/main/docs/workflow/testing/coreclr/test-configuration.md#creating-a-c-test-project)
```bash
# 显示帮助
./src/tests/build.sh -h

# 使用本机构建单个测试项目
./src/tests/build.sh -nativeaot -test:project.csproj

# 使用 debug 配置构建 nativeaot 和 GC/API/GC 子树测试
# 会构建 nativeaot 目录和 GC/API/GC 目录下的所有项目
./src/tests/build.sh -debug -tree:nativeaot -tree:GC/API/GC
```

### 部件说明
#### coreclr
| 是运行时存储库中最重要的组件之一，因为它是 .NET 产品的主要引擎之一
| 位于 `<RepoRoot>/src/coreclr` 目录下

##### 构建脚本
```bash
# 默认情况会以 Debug 配置构建
./build.sh -s clr [其他参数]
```
##### 构建目录结构
```text
<RepoRoot>/artifacts/bin/coreclr/<OS>.<Arch>.<Configuration>
├── corerun/.exe        # 命令行可执行文件，加载并启动 CoreCLR 运行时，接收要运行的托管程序作为参数（如 corerun hello.dll）
├── coreclr.dll/libcoreclr.dylib/.os  # CoreCLR 运行时本身
└── System.Private.CoreLib.dll  # 运行时核心库，包含 `Object` 的定义和基本功能
```

#### Core_Root
| 本质上是 测试运行时和托管程序集的“最小运行环境”，相当于 .net 的运行时工作目录，其中包含
- corerun ：轻量启动器，用来运行托管 DLL（类似 dotnet，但更底层，直接加载 CoreCLR）
- 运行时核心库 ：System.Private.CoreLib.dll、mscorlib.dll、BCL(基础类库)
- PDB 调试符号 ：方便调试测试

所在位置
```text
<RepoRoot>/artifacts/tests/coreclr/<OS>.<Arch>.<Configuration>/Tests/Core_Root
```
构建 Core_Root
```bash
# 1. 需要先构建 clr+libs
./build.sh -subset clr+libs -runtimeConfiguration Debug -librariesConfiguration Release

# 2. 构建 Core_Root
./src/tests/build.sh -generatelayoutonly [-debug]
```

## 执行生成的测试 dll
| 在自己构建的 runtime 中要执行生成的 dll 需要使用 corerun
| corerun 在不同系统中表现形式不同，osx 和 linux 下是一个可执行文件，而在 windows 下是一个 .exe 文件

### 使用 corerun
```bash
<corerun> [-p <key>=<value>] <dll> [<dllArgs>]

# 示例：
corerun -p System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization=true dllDir/hello.dll
```

### 使用生成目录中的脚本
| runtime 生成的测试目录中会有一个 <dllname>.sh 脚本文件
| 使用该脚本并传入 coreroot 路径即可
```bash
<dlldir>/<dllname>.sh -coreroot <CoreRoot>

# 示例：
cd <dllDir>
./hello.sh -coreroot <CoreRoot>

# 还可以将 CORE_ROOT 设置到当前环境变量中
export CORE_ROOT=<CoreRoot>
./hello.sh
```

## 库信息

### System.Private.CoreLib
| 运行时的核心库，核心管理库，包含 Object 的定义和基本功能

#### 位于 libs 中的 System.Private.CoreLib 是主要代码实现
```text
代码位置
<RepoRoot>/src/libraries/System.Private.CoreLib

构建位置（这里构建出来的是一个"引用程序集",只有元数据，没有实现）
<RepoRoot>/artifacts/bin/System.Private.CoreLib/ref/<Configuration>/<TargetFramework>/System.Private.CoreLib.dll
```

#### 位于 clr 中的 System.Private.CoreLib 是 CoreCLR 的实现
```text
代码位置
<RepoRoot>/src/coreclr/System.Private.CoreLib

构建位置（libs+clr 中构建的实现都在这里）
<RepoRoot>/artifacts/bin/coreclr/<OS>.<Arch>.<Configuration>/System.Private.CoreLib.dll
```

#### 位于 aot 中的 System.Private.CoreLib 是 AOT 编译的实现
```text
代码位置
<RepoRoot>/src/coreclr/nativeaot/System.Private.CoreLib

构建位置 (aot 独立实现的构建)
<RepoRoot>/artifacts/bin/coreclr/<OS>.<Arch>.<Configuration>/aotsdk/System.Private.CoreLib.dll
```

#### 位于 mono 中的 System.Private.CoreLib 是 Mono 的实现
```text
代码位置
<RepoRoot>/src/mono/System.Private.CoreLib

构建位置 (mono 独立实现的构建)
<RepoRoot>/artifacts/bin/mono/<OS>.<Arch>.<Configuration>/System.Private.CoreLib.dll
```

## 问题及处理

### 找不到 libjitinterface_arm64
```bash
# 重新构建 clr
./build.sh -s clr -c Debug
```

## 常用汇总

```bash
# 清理仓库构建(会清理所有构建产物 artifacts/)
./build.sh --clean

# 清理仓库
git clean -xdf
# 从远程仓库 main 分支拉取最新代码合并到当前分支
git pull upstream main

# 测试 aot 编译项目(test:后面可以是tests下的相对路径或者绝对路径)
./src/tests/build.sh -nativeaot -test:project.csproj
# 如果修改了 libs/aot 中的 System.Private.CoreLib 后最小需要构建以下项目后重新编译测试项目
./build.sh -s Clr.NativeAotLibs

# 测试依赖运行时项目(在项目生成的目录中)
export CORE_ROOT=<CoreRoot>
./project.sh
# 如果修改了 libs/clr 中的 System.Private.CoreLib 后最小需要构建以下项目后重新测试
./build.sh -s Clr.NativeCoreLib
./src/tests/build.sh -generatelayoutonly
```

#### 初次构建
```bash
# 安装依赖
./eng/common/native/install-dependencies.sh
# 全量构建（初次构建时推荐，也可根据需求构建）
./build.sh
# 运行所有测试（可选）
./build.sh -test

# 构建本地运行环境
# 构建过后在 <RepoRoot>/artifacts/tests/coreclr/osx.arm64.Debug/Tests/Core_Root
./build.sh -s clr+libs -rc Debug -lc Release
./src/tests/build.sh -generatelayoutonly

# 测试一个项目
./src/tests/build.sh -nativeaot -tree:nativeaot
# 使用运行时运行
cd <RepoRoot>/artifacts/tests/coreclr/osx.arm64.Debug/nativeaot/SmokeTests/DynamicGenerics/DynamicGenerics
export CORE_ROOT=<CoreRoot>
./DynamicGenerics.sh
# 运行 aot 编译的项目(是一个可运行程序，可以直接双击运行)
cd <RepoRoot>/artifacts/tests/coreclr/osx.arm64.Debug/nativeaot/SmokeTests/DynamicGenerics/DynamicGenerics/native
./DynamicGenerics
```
