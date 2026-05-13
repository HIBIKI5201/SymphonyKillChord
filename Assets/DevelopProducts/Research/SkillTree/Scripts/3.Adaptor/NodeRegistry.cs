using System;
using System.Collections.Generic;

namespace DevelopProducts.SkillTree
{
    /// <summary>
    ///     スキルIDとCanLockViewModelの対応を管理するレジストリクラス。
    /// </summary>
    public class NodeRegistry
    {
        /// <summary>
        ///     スキルIDに対応するViewModelを登録するメソッド。
        /// </summary>
        /// <param name="skillId">スキルID。</param>
        /// <param name="viewModel">登録するViewModel。</param>
        /// <exception cref="ArgumentNullException">viewModelがnullの場合。</exception>
        public void Register(int skillId, ICanUnlockVM viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            _viewModels[skillId] = viewModel;
        }

        /// <summary>
        ///     スキルIDに対応するViewModelの登録を解除するメソッド。
        /// </summary>
        /// <param name="skillId">スキルID。</param>
        /// <returns>解除できた場合はtrue。</returns>
        public bool Unregister(int skillId)
        {
            return _viewModels.Remove(skillId);
        }

        /// <summary>
        ///     スキルIDに対応するViewModelを取得するメソッド。
        /// </summary>
        /// <param name="skillId">スキルID。</param>
        /// <param name="viewModel">取得したViewModel。</param>
        /// <returns>取得できた場合はtrue。</returns>
        public bool TryGet(int skillId, out ICanUnlockVM viewModel)
        {
            return _viewModels.TryGetValue(skillId, out viewModel);
        }

        /// <summary>
        ///     登録済みのViewModelをすべて削除するメソッド。
        /// </summary>
        public void Clear()
        {
            _viewModels.Clear();
        }

        private readonly Dictionary<int, ICanUnlockVM> _viewModels = new Dictionary<int, ICanUnlockVM>();
    }
}