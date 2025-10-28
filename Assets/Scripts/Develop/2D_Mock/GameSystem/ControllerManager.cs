using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    [Header("���Y���V�X�e��")]
    [SerializeField] private RythemManager rythemManager;

    [Header("�v���C���[�̃X�e�[�^�X")]
    [SerializeField] private GameObject Player;
    [Space]
    [SerializeField] private float playerSpeed;
    [SerializeField] private float playerJumpPower;


    private Transform PlayerTransform;
    private Rigidbody2D PlayerRB;

    private float Timer;

    void Start()
    {
        PlayerTransform = Player.GetComponent<Transform>();
        PlayerRB = Player.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        if (moveHorizontal != 0)
        {
           PlayerRB.linearVelocity = new Vector2 (moveHorizontal * playerSpeed, PlayerRB.linearVelocity.y);
        }

        if (moveVertical == 1)
        {
            PlayerRB.linearVelocity = new Vector2(PlayerRB.linearVelocity.x, 0);
            PlayerRB.linearVelocity = new Vector2(PlayerRB.linearVelocity.x, playerJumpPower);
            Debug.Log("JumpTrue");
        }

        if(Input.GetMouseButtonDown(0))
        {
            rythemManager.RedNotesSpawn();
        }

        /* �N�[���_�E���̃R�[�h
        if (Timer == 0)
            {
                rythemManager.RedNotesSpawn();
                Timer = 60 / rythemManager.BPM;
            }
        }
        if (Timer > 0)
        {
            Timer -= Time.deltaTime;
            if (Timer < 0)
            {
                Timer = 0;
            }
        } 
        */
    }
}
