cmake_minimum_required(VERSION 3.25)

# スクリプト管理ツール。core（設定読込・コマンドの組立・出力の整形）は OS に依存しないので、どの OS でもテストできる。
# 画面（src/win32）は Windows のときだけ作る。
project(ScriptLauncher LANGUAGES CXX)
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

add_library(ScriptLauncherCore STATIC
    src/core/CommandLine.cpp
    src/core/OutputText.cpp
    src/core/RepositoryRoot.cpp
    src/core/ScriptConfig.cpp
)
target_include_directories(ScriptLauncherCore
    PUBLIC src/core
    PRIVATE third_party
)

enable_testing()
add_executable(ScriptLauncherCoreTests tests/CoreTests.cpp)
target_link_libraries(ScriptLauncherCoreTests PRIVATE ScriptLauncherCore)
add_test(NAME CoreTests COMMAND ScriptLauncherCoreTests ${CMAKE_CURRENT_SOURCE_DIR}/launcher-scripts.json)

if(WIN32)
    add_executable(ScriptLauncher WIN32
        src/win32/Main.cpp
        src/win32/MainWindow.cpp
        src/win32/ProcessRunner.cpp
        src/win32/TextConvert.cpp
        src/win32/ScriptLauncher.rc
    )
    target_compile_definitions(ScriptLauncher PRIVATE UNICODE _UNICODE WIN32_LEAN_AND_MEAN NOMINMAX _WIN32_WINNT=0x0A00 WINVER=0x0A00 NTDDI_VERSION=0x0A000006)
    target_link_libraries(ScriptLauncher PRIVATE ScriptLauncherCore comctl32 shell32 uxtheme)
    if(MSVC)
        # manifest は .rc で埋め込むので、リンカーの自動生成は止める。
        target_link_options(ScriptLauncher PRIVATE /MANIFEST:NO)
    else()
        target_link_options(ScriptLauncher PRIVATE -municode -static)
    endif()
endif()
