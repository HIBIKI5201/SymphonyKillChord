using KillChord.Runtime.Adaptor.Persistent.Environment;
using R3;
using System;
using UnityEngine.InputSystem;

namespace KillChord.Runtime.View.Persistent.Input
{
    /// <summary>
    ///     ゲームパッドの決定・キャンセルの配置を、環境設定に合わせて入力バインドへ反映するView。
    ///     入力アセットは日本式（決定=右ボタン、キャンセル=下ボタン）で定義しているため、
    ///     海外式のときだけ右ボタンと下ボタンを入れ替える上書きをかける。
    /// </summary>
    public sealed class GamepadButtonLayoutView : IDisposable
    {
        /// <summary>
        ///     配置の反映を開始する。
        /// </summary>
        /// <param name="actions"> 上書きをかける入力アセット。 </param>
        /// <param name="environmentSettingsViewModel"> 環境設定のViewModel。nullの場合は既定の海外式で固定する。 </param>
        public GamepadButtonLayoutView(InputActionAsset actions, IEnvironmentSettingsViewModel environmentSettingsViewModel)
        {
            _actions = actions ?? throw new ArgumentNullException(nameof(actions));

            if (environmentSettingsViewModel == null)
            {
                Apply(false);
                return;
            }

            _subscription = environmentSettingsViewModel.IsJapaneseButtonLayout.Subscribe(Apply);
        }

        /// <summary>
        ///     購読を解除し、上書きを外してアセットの定義へ戻す。
        /// </summary>
        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
            Apply(true);
        }

        private const string GAMEPAD_EAST_PATH = "<Gamepad>/buttonEast";
        private const string GAMEPAD_SOUTH_PATH = "<Gamepad>/buttonSouth";

        /// <summary> 決定・キャンセルとして扱うアクション。「マップ名/アクション名」の形式。 </summary>
        private static readonly string[] TARGET_ACTION_PATHS =
        {
            InputMapNames.OutGame + "/Submit",
            InputMapNames.OutGame + "/Cancel",
            InputMapNames.UI + "/Submit",
            InputMapNames.UI + "/Cancel",
            InputMapNames.Scenario + "/Advance",
            InputMapNames.Scenario + "/FastForward",
        };

        private readonly InputActionAsset _actions;
        private IDisposable _subscription;

        /// <summary>
        ///     対象アクションのゲームパッドの右ボタン・下ボタンのバインドを、配置に合わせて上書きする。
        /// </summary>
        /// <param name="isJapaneseButtonLayout"> 日本式の場合はtrue。上書きを外してアセットの定義どおりにする。 </param>
        private void Apply(bool isJapaneseButtonLayout)
        {
            foreach (string actionPath in TARGET_ACTION_PATHS)
            {
                InputAction action = _actions.FindAction(actionPath);
                if (action == null)
                {
                    continue;
                }

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    string path = action.bindings[i].path;
                    if (path != GAMEPAD_EAST_PATH && path != GAMEPAD_SOUTH_PATH)
                    {
                        continue;
                    }

                    if (isJapaneseButtonLayout)
                    {
                        action.RemoveBindingOverride(i);
                        continue;
                    }

                    action.ApplyBindingOverride(i, path == GAMEPAD_EAST_PATH ? GAMEPAD_SOUTH_PATH : GAMEPAD_EAST_PATH);
                }
            }
        }
    }
}
