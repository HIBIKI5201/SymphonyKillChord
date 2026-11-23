#pragma once

#include <windows.h>
#include <unknwn.h>

#include <atomic>
#include <functional>
#include <utility>

namespace skc::conductor::gui
{
    /// <summary>
    ///     WebView2 の完了・イベントの受け口（COM のインターフェース）を、関数1つで作る。
    ///     WRL を使わない（MinGW でもビルドできるようにする）ための小さな実装。
    /// </summary>
    /// <typeparam name="Interface">受け口のインターフェース。</typeparam>
    /// <typeparam name="Args">Invoke の引数。</typeparam>
    template <typename Interface, typename... Args>
    class ComHandler final : public Interface
    {
    public:
        /// <summary>
        ///     受け口を作る。参照の数は 1 から始まる。
        /// </summary>
        /// <param name="iid">インターフェースの IID。</param>
        /// <param name="invoke">呼ばれたときの処理。</param>
        ComHandler(const IID& iid, std::function<HRESULT(Args...)> invoke)
            : _iid(iid), _invoke(std::move(invoke))
        {
        }

        ULONG STDMETHODCALLTYPE AddRef() override { return ++_references; }

        ULONG STDMETHODCALLTYPE Release() override
        {
            ULONG remaining = --_references;
            if (remaining == 0) { delete this; }

            return remaining;
        }

        HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** result) override
        {
            if (result == nullptr) { return E_POINTER; }

            if (IsEqualIID(iid, __uuidof(IUnknown)) || IsEqualIID(iid, _iid))
            {
                *result = static_cast<Interface*>(this);
                AddRef();
                return S_OK;
            }

            *result = nullptr;
            return E_NOINTERFACE;
        }

        HRESULT STDMETHODCALLTYPE Invoke(Args... args) override { return _invoke(args...); }

    private:
        ~ComHandler() = default;

        IID _iid;
        std::function<HRESULT(Args...)> _invoke;
        std::atomic<ULONG> _references{ 1 };
    };
}
