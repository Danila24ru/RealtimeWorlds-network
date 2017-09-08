using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.UnityNetcodeIO.Scripts.Utils;
using ReliableNetcode;
using NetcodeIO.NET.Utils;

public class Player : MonoBehaviour {

    public ulong clientID;

    public string Nickname { get; set; }
    
    public float health { get; set; }
    public int money { get; set; }

    public int level { get; private set; }
    public float XP { get; set; }

    public bool isLocalPlayer = false;

    public TextMesh nameText;

    void Awake()
    {

    }
    // Use this for initialization
    void Start () {
        Nickname = "";
        health = 100;
        money = 0;
        level = 1;

        if (NetworkManager.NM != null)
        {
            isLocalPlayer = NetworkManager.NM.localClientID == clientID;
            Debug.LogError("ID IN nm: " + NetworkManager.NM.localClientID + " ID in Player: " + clientID);
        }
    }
	
	// Update is called once per frame
	void Update () {

        nameText.text = Nickname;

        if (!isLocalPlayer)
            return;


        var objectPos = Camera.main.WorldToScreenPoint(transform.position);
        var dir = Input.mousePosition - objectPos;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90));



        if (Input.GetKey(KeyCode.W))
            SEND_INPUT.Send(KeyCode.W, true);
        if (Input.GetKeyUp(KeyCode.W))
            SEND_INPUT.Send(KeyCode.W, false);

        if (Input.GetKey(KeyCode.A))
            SEND_INPUT.Send(KeyCode.A, true);
        if (Input.GetKeyUp(KeyCode.A))
            SEND_INPUT.Send(KeyCode.A, false);

        if (Input.GetKey(KeyCode.S))
            SEND_INPUT.Send(KeyCode.S, true);
        if (Input.GetKeyUp(KeyCode.S))
            SEND_INPUT.Send(KeyCode.S, false);

        if (Input.GetKey(KeyCode.D))
            SEND_INPUT.Send(KeyCode.D, true);
        if (Input.GetKeyUp(KeyCode.D))
            SEND_INPUT.Send(KeyCode.D, false);
    }
}
