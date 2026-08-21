using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵のAIコントローターを保持し、管理するクラス。
    /// </summary>
    public class EnemyAIControllerRegistry
    {
        public EnemyAIControllerRegistry()
        {
            _activeControllers = new();
            _isBattleAIActivated = true;
        }

        /// <summary> 敵の戦闘AIが有効か否か </summary>
        public bool IsBattleAIActivated => _isBattleAIActivated;

        /// <summary>
        ///     敵のAIコントローラーを登録する。
        /// </summary>
        /// <param name="controller"></param>
        public void Register(EnemyAIController controller)
        {
            _activeControllers?.Add(controller);
            if (_isBattleAIActivated)
            {
                controller.StartBattleAI();
            }
            else
            {
                controller.StopBattleAI();
            }
        }

        /// <summary>
        ///     敵のAIコントローラーの登録を解除する。
        /// </summary>
        /// <param name="controller"></param>
        public void Unregister(EnemyAIController controller)
        {
            _activeControllers?.Remove(controller);
        }

        /// <summary>
        ///     敵のAI有効化を切り替える。
        /// </summary>
        /// <param name="isActivated">有効の場合はtrue、無効の場合はfalse</param>
        public void SetBattleAiActivated(bool isActivated)
        {
            if (isActivated == _isBattleAIActivated) return;
            _isBattleAIActivated = isActivated;
            foreach (EnemyAIController controller in _activeControllers)
            {
                if (isActivated)
                {
                    controller.StartBattleAI();
                }
                else
                {
                    controller.StopBattleAI();
                }
            }
        }

        private HashSet<EnemyAIController> _activeControllers;
        private bool _isBattleAIActivated;
    }
}
