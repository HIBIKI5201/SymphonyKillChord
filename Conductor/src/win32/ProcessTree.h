#pragma once

#include <windows.h>

namespace skc::conductor::win32
{
    /// <summary>
    ///     子プロセスと、その子が起こした孫プロセスを、まとめて止められるようにする（ジョブオブジェクト）。
    ///     時間切れで子を止めるとき、孫（git submodule の sh.exe など）が残らないようにする。
    ///     止めるのは Terminate を呼んだときだけ。普通に終わったときは、残った常駐プロセスに手を出さない。
    /// </summary>
    class ProcessTree
    {
    public:
        ProcessTree();
        ~ProcessTree();

        ProcessTree(const ProcessTree&) = delete;
        ProcessTree& operator=(const ProcessTree&) = delete;

        /// <summary>
        ///     子をまとめる対象に入れる。子は CREATE_SUSPENDED で起こし、入れてから ResumeThread する。
        /// </summary>
        /// <param name="process">子プロセス。</param>
        /// <returns>入れられたら true。入れられなくても子はそのまま動かしてよい（止めるときは子だけになる）。</returns>
        bool Assign(HANDLE process);

        /// <summary>
        ///     子と孫をまとめて止める。まとめられていなければ子だけを止める。
        /// </summary>
        /// <param name="process">子プロセス。</param>
        void Terminate(HANDLE process);

    private:
        /// <summary>ジョブオブジェクト。作れなければ nullptr。</summary>
        HANDLE m_job;

        /// <summary>子をジョブに入れられたか。</summary>
        bool m_isAssigned;
    };
}
