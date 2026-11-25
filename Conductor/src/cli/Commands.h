#pragma once

#include <string>
#include <vector>

namespace skc::conductor::cli
{
    /// <summary>CLI の版。</summary>
    inline constexpr const char* CONDUCTOR_VERSION = "1.1.1";

    /// <summary>
    ///     CLI のコマンドを動かす。
    /// </summary>
    /// <param name="args">プログラム名を除いた引数（UTF-8）。</param>
    /// <returns>終了コード（ExitCodes.h）。scripts run はスクリプトの終了コード。</returns>
    int RunCommand(const std::vector<std::string>& args);
}
