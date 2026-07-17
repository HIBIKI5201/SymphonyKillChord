using UnityEngine;

namespace KillChord.Runtime.Utility.OutGame
{
    /// <summary>
    ///     UI ToolkitのUSSクラス名の定数を保持するクラス。
    /// </summary>
    public class UssClassNameConstants
    {
        // ---スキルツリー関連---

        // スキルノード
        public const string USS_CLASS_SKILL_NODE = "skill-node";
        public const string USS_CLASS_SKILL_NODE_NODE = "skill-node-node";
        public const string USS_CLASS_SKILL_NODE_PATH = "skill-node-path";
        public const string USS_CLASS_SKILL_NODE_LOCKED = "skill-node-locked";
        public const string USS_CLASS_SKILL_NODE_UNLOCKED = "skill-node-unlocked";
        public const string USS_CLASS_SKILL_NODE_SELECTED = "skill-node-selected";
        public const string USS_CLASS_SKILL_NODE_NOT_SELECTED = "skill-node-not-selected";

        // スキルノードの接続線
        public const string USS_CLASS_SKILL_NODE_CONN = "skill-node-conn";
        public const string USS_CLASS_SKILL_NODE_CONN_PASSED = "skill-node-conn-passed";
        public const string USS_CLASS_SKILL_NODE_CONN_NOT_PASSED = "skill-node-conn-not-passed";
    }
}
