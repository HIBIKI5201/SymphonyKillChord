cmake_minimum_required(VERSION 3.25)

# KillChord Conductor（プロジェクト管理アプリ）。
# - core: 設定の読込・コマンドの組立・出力の整形・JSON の出し入れ。OS に依存しないので、どの OS でもテストできる。
# - conductor（CLI）: どの OS でもビルドできる。Windows はコンソールのプログラム。
# - KillChordConductor（GUI）: Windows のときだけ作る。画面は ui/ の HTML を WebView2 で出し、自分では何も実行せず conductor を呼ぶ。
project(Conductor LANGUAGES CXX)
if(WIN32)
    enable_language(RC)
endif()

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# VC++ 再頒布パッケージを入れていない PC でも動くよう、CRT は静的リンクにする。
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

if(MSVC)
    add_compile_options(/utf-8 /W4 /permissive- /EHsc)
else()
    add_compile_options(-Wall -Wextra -Wpedantic)
endif()

add_library(ConductorCore STATIC
    src/core/Actions.cpp
    src/core/CliArgs.cpp
    src/core/CliJson.cpp
    src/core/CommandLine.cpp
    src/core/HomeConfig.cpp
    src/core/OutputText.cpp
    src/core/ProjectStatus.cpp
    src/core/RepositoryRoot.cpp
    src/core/ScriptConfig.cpp
    src/core/Settings.cpp
)
target_include_directories(ConductorCore
    PUBLIC src/core
    PRIVATE third_party
)

enable_testing()
add_executable(ConductorCoreTests tests/CoreTests.cpp)
target_link_libraries(ConductorCoreTests PRIVATE ConductorCore)
add_test(NAME CoreTests COMMAND ConductorCoreTests ${CMAKE_CURRENT_SOURCE_DIR}/launcher-scripts.json ${CMAKE_CURRENT_SOURCE_DIR}/home.json)

set(WINDOWS_DEFINITIONS UNICODE _UNICODE WIN32_LEAN_AND_MEAN NOMINMAX _WIN32_WINNT=0x0A00 WINVER=0x0A00 NTDDI_VERSION=0x0A000006)

if(WIN32)
    add_library(ConductorWin32 STATIC src/win32/PipeReader.cpp src/win32/TextConvert.cpp)
    target_include_directories(ConductorWin32 PUBLIC src/win32)
    target_compile_definitions(ConductorWin32 PUBLIC ${WINDOWS_DEFINITIONS})
    target_link_libraries(ConductorWin32 PUBLIC ConductorCore)
    set(CLI_PLATFORM_SOURCE src/cli/PlatformWindows.cpp)
else()
    set(CLI_PLATFORM_SOURCE src/cli/PlatformPosix.cpp)
endif()

# CLI。exe 名は conductor（Windows は conductor.exe）。
add_executable(conductor
    src/cli/Commands.cpp
    src/cli/Main.cpp
    ${CLI_PLATFORM_SOURCE}
    $<$<BOOL:${WIN32}>:src/cli/conductor.rc>
)
target_link_libraries(conductor PRIVATE ConductorCore)
if(WIN32)
    target_link_libraries(conductor PRIVATE ConductorWin32 shell32)
    if(MSVC)
        target_link_options(conductor PRIVATE /MANIFEST:NO)
    else()
        target_link_options(conductor PRIVATE -municode -static)
    endif()
endif()

# CLI を実際のリポジトリで動かすテスト（登録ファイル・ホーム画面・設定の読み書き）。
add_test(NAME CliScriptsList COMMAND conductor scripts list --json --root ${CMAKE_CURRENT_SOURCE_DIR}/..)
add_test(NAME CliHome COMMAND conductor home --json --role sound --root ${CMAKE_CURRENT_SOURCE_DIR}/..)
add_test(NAME CliUnknownCommand COMMAND conductor nothing)
set_tests_properties(CliUnknownCommand PROPERTIES WILL_FAIL TRUE)

if(WIN32)
    # GUI。conductor.exe と同じフォルダに置いて使う。
    # 画面は Conductor/ui の HTML を WebView2 で出す。WebView2Loader.dll は実行時に読む（静的リンクしない）。
    add_executable(KillChordConductor WIN32
        src/gui/CliClient.cpp
        src/gui/HostWindow.cpp
        src/gui/Main.cpp
        src/gui/ProcessRunner.cpp
        src/gui/KillChordConductor.rc
    )
    target_include_directories(KillChordConductor PRIVATE third_party third_party/webview2/include)
    if(NOT MSVC)
        # MinGW には EventToken.h が無いので、同じ中身の小さなヘッダーで補う。
        target_include_directories(KillChordConductor PRIVATE third_party/webview2/mingw)
        target_compile_options(KillChordConductor PRIVATE -Wno-unknown-pragmas)
    endif()
    target_link_libraries(KillChordConductor PRIVATE ConductorWin32 shell32 ole32 user32 advapi32 dwmapi)
    if(MSVC)
        # manifest は .rc で埋め込むので、リンカーの自動生成は止める。
        target_link_options(KillChordConductor PRIVATE /MANIFEST:NO)
    else()
        target_link_options(KillChordConductor PRIVATE -municode -static)
    endif()
endif()
