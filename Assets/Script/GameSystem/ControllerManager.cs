using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    [SerializeField] private RythemManager rythemManager;

    [SerializeField] private GameObject Player;
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
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        {
            float moveHorizontal = Input.GetAxis("Horizontal");

            PlayerTransform.Translate(transform.right * moveHorizontal * Time.deltaTime * playerSpeed);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            PlayerRB.velocity = new Vector2(PlayerRB.velocity.x, 0);
            PlayerRB.velocity = new Vector2(PlayerRB.velocity.x, playerJumpPower);
            Debug.Log("JumpTrue");
        }

        if(Input.GetMouseButtonDown(0))
        {
            rythemManager.RedNotesSpawn();
        }

        /* クールダウンのコード
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
