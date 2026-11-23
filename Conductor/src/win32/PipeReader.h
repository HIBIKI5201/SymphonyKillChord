#pragma once

#include <windows.h>

#include <string>
#include <vector>

namespace skc::conductor::win32
{
    /// <summary>
    ///     読むパイプ1本と、読んだものを入れる先。
    /// </summary>
    struct PipeTarget
    {
        /// <summary>読む側のパイプ。</summary>
        HANDLE Pipe;

        /// <summary>読んだバイト列を足す先。</summary>
        std::string* Output;
    };

    /// <summary>
    ///     子プロセスが終わるまでパイプを読む。
    ///     パイプの終わり（EOF）は待たない。子が起こした孫（git の常駐プロセスなど）が書く側を持ったままでも、
    ///     子が終わったら残りを読み切って戻る。
    /// </summary>
    /// <param name="process">子プロセス。</param>
    /// <param name="pipes">読むパイプ。</param>
    /// <param name="timeoutMs">待つ長さ（ミリ秒）。INFINITE なら限りなく待つ。</param>
    /// <returns>子が終わったら true。時間切れなら false（子はまだ動いている）。</returns>
    bool ReadPipesUntilExit(HANDLE process, const std::vector<PipeTarget>& pipes, DWORD timeoutMs);
}
