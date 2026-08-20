using KillChord.Runtime.Utility.Identity;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Playables;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     クリップの再生区間に合わせて、指定タレットへ弾幕コマンドを送ります。
    /// </summary>
    [Serializable]
    public sealed class BarrageFireBehaviour : PlayableBehaviour
    {
        /// <summary>
        ///     区間の開始時に発射開始を命令します。
        /// </summary>
        /// <param name="playable"> 再生中のPlayableです。 </param>
        /// <param name="info"> 現在フレームの情報です。 </param>
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            SendCommand(BarrageCommandKind.Start);
        }

        /// <summary>
        ///     区間の終了時に発射停止を命令します。
        /// </summary>
        /// <remarks> 途中停止やスクラブで区間を抜けた場合にも呼ばれます。 </remarks>
        /// <param name="playable"> 再生中のPlayableです。 </param>
        /// <param name="info"> 現在フレームの情報です。 </param>
        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            SendCommand(BarrageCommandKind.Stop);
        }

        [SerializeField, SourceDataCollection(TurretId.COLLECTION_KEY, true),
         Tooltip("弾幕を命令するタレットのIDです。TurretAuthoring側と同じ文字列を設定します。")]
        private DataID _targetTurretId;

        /// <summary>
        ///     コマンド用のEntityを生成します。
        /// </summary>
        /// <param name="kind"> 送信するコマンドの種類です。 </param>
        private void SendCommand(BarrageCommandKind kind)
        {
            // エディタでのスクラブ中はEntity Worldへ副作用を出さない。
            if (!Application.isPlaying) { return; }

            World world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) { return; }

            EntityManager entityManager = world.EntityManager;
            Entity commandEntity = entityManager.CreateEntity(ComponentType.ReadWrite<BarrageFireCommand>());
            entityManager.SetComponentData(commandEntity, new BarrageFireCommand
            {
                TargetTurretId = _targetTurretId.Id,
                Kind = kind,
            });
        }
    }
}
