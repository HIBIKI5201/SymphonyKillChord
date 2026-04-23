using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetStunned", story: "被弾硬直を開始する [State] [Battle]", category: "Action", id: "459e141cce9d40aaaebad1a7c2283299")]
public partial class GetStunnedAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
    [SerializeReference] public BlackboardVariable<EnemyBattleAIFacade> Battle;

    private Animator m_Animator;
    private GameObject m_TextObject;
    private GameObject m_TextMeshAsset;
    private static readonly int s_Talking = Animator.StringToHash("Talking"); 
    protected override Status OnStart()
    {
        if (State?.Value?.gameObject == null
            || Battle?.Value == null) return Status.Failure;

        GameObject agent = State.Value.gameObject;
        EnemyStateFacade state = State.Value;
        EnemyBattleAIFacade battle = Battle.Value;

        // 【TODO START】文字表示処理
        if (m_TextMeshAsset == null)
        {
            m_TextMeshAsset = Resources.Load<GameObject>("TextMesh Speech Preset");
        }
        CreateTextObject("CRITICAL!!", agent);

        m_Animator = agent.GetComponent<Animator>();
        if (m_Animator != null)
        {
            m_Animator.SetBool(s_Talking, true);
        }
        // 【TODO End】

        battle.CancelAttack();
        state.Stunned();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // 【TODO START】文字表示処理
        m_TextObject.transform.rotation = GetTextLookRotation();
        // 【TODO END】

        if (State.Value.IsStunned) return Status.Running;
        return Status.Success;
    }

    protected override void OnEnd()
    {
        // 【TODO START】文字表示処理
        if (m_TextObject != null)
        {
            UnityEngine.Object.Destroy(m_TextObject);
            m_TextObject = null;
        }
        if (m_Animator != null)
        {
            m_Animator.SetBool(s_Talking, false);
        }
        // 【TODO End】
    }

    // 【TODO START】文字表示処理
    public void CreateTextObject(string text, GameObject parent)
    {
        Vector3 pos = GetBoundsOffset(parent);
        m_TextObject = UnityEngine.Object.Instantiate(m_TextMeshAsset, parent.transform, true);
        m_TextObject.GetComponent<TMPro.TextMeshPro>().text = text;

        m_TextObject.transform.localPosition = pos;
        m_TextObject.transform.rotation = GetTextLookRotation();
    }

    private Vector3 GetBoundsOffset(GameObject gameObject)
    {
        MeshFilter parentMesh = gameObject.GetComponent<MeshFilter>();
        if (parentMesh != null)
        {
            return GetBoundsOffset(parentMesh.mesh.bounds);
        }
        Collider parentCollider = gameObject.GetComponent<Collider>();
        if (parentCollider != null)
        {
            return GetBoundsOffset(parentCollider.bounds);
        }
        return Vector3.zero;
    }

    private Vector3 GetBoundsOffset(Bounds bounds)
    {
        return new Vector3(0.0f, bounds.max.y + 0.05f);
    }

    private Quaternion GetTextLookRotation()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0.0f;
        cameraForward.Normalize();
        return Quaternion.LookRotation(cameraForward);
    }
    // 【TODO End】
}

