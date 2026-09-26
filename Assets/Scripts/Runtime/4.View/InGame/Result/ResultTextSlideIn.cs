using System;
using System.Collections.Generic;
using LitMotion;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Result
{
    /// <summary>
    ///     文字の描画頂点だけを動かし、レイアウトと入力領域を保持します。
    /// </summary>
    internal sealed class ResultTextSlideIn : IDisposable
    {
        /// <summary>
        ///     Timelineでの表示順を待つ文字を開始位置に準備します。
        /// </summary>
        public ResultTextSlideIn(TMP_Text text, ResultTextSlideInSetting setting)
        {
            _text = text;
            _setting = setting;
            _text.OnPreRenderText += HandlePreRenderText;
            RefreshMesh();
        }

        /// <summary>
        ///     待機中の文字を表示位置へ動かします。
        /// </summary>
        public void Play(List<MotionHandle> handles, Action onCompleted, float delay = 0f)
        {
            if (_setting.Duration <= 0f)
            {
                Dispose();
                onCompleted?.Invoke();
                return;
            }

            MotionHandle handle = LMotion.Create(0f, 1f, _setting.Duration)
                .WithDelay(delay)
                .WithEase(_setting.Ease)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() =>
                {
                    Dispose();
                    onCompleted?.Invoke();
                })
                .Bind(progress =>
                {
                    _progress = progress;
                    RefreshMesh();
                });
            handles.Add(handle);
        }

        /// <summary>
        ///     描画フックを解放し、途中終了でも元の文字表示へ戻します。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_text != null)
            {
                _text.OnPreRenderText -= HandlePreRenderText;
                RefreshMesh();
            }
        }

        private readonly TMP_Text _text;
        private readonly ResultTextSlideInSetting _setting;
        private float _progress;
        private bool _disposed;

        /// <summary>
        ///     TMPが生成した新しい頂点へ表示オフセットと透明度を適用します。
        /// </summary>
        private void HandlePreRenderText(TMP_TextInfo info)
        {
            Vector3 offset = Vector3.left * (_setting.Distance * (1f - _progress));
            float alpha = _setting.UseFade ? Mathf.Clamp01(_progress) : 1f;
            for (int index = 0; index < info.characterCount; index++)
            {
                TMP_CharacterInfo character = info.characterInfo[index];
                if (!character.isVisible)
                {
                    continue;
                }

                TMP_MeshInfo mesh = info.meshInfo[character.materialReferenceIndex];
                for (int corner = 0; corner < 4; corner++)
                {
                    int vertex = character.vertexIndex + corner;
                    mesh.vertices[vertex] += offset;
                    mesh.colors32[vertex].a = (byte)(mesh.colors32[vertex].a * alpha);
                }
            }
        }

        /// <summary>
        ///     元の文字情報から再生成し、オフセットの累積を防ぎます。
        /// </summary>
        private void RefreshMesh()
        {
            if (_text != null && _text.isActiveAndEnabled)
            {
                _text.ForceMeshUpdate();
            }
        }
    }
}
