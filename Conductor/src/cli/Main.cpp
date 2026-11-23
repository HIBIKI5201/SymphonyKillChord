// conductor（CLI）の入口。引数を UTF-8 にそろえてコマンドへ渡す。
#include "Commands.h"

#include <string>
#include <vector>

#if defined(_WIN32)
#include "TextConvert.h"

#include <windows.h>

/// <summary>
///     UTF-16 の引数を UTF-8 にしてコマンドを動かす。
/// </summary>
int wmain(int argc, wchar_t** argv)
{
    std::vector<std::string> args;
    for (int i = 1; i < argc; ++i) { args.push_back(skc::conductor::win32::WideToUtf8(argv[i])); }

    return skc::conductor::cli::RunCommand(args);
}
#else
/// <summary>
///     引数（UTF-8）でコマンドを動かす。
/// </summary>
int main(int argc, char** argv)
{
    std::vector<std::string> args(argv + 1, argv + argc);
    return skc::conductor::cli::RunCommand(args);
}
#endif
