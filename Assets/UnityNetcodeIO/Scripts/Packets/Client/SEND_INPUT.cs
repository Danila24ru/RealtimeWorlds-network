using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NetcodeIO.NET;
using PacketIO;
using UnityEngine;

public class SEND_INPUT
#if (!CLIENT)
    : IServerPacketHandler
    #endif
{

    public static void Send(KeyCode keyCode, bool isPressed)
    {
        PacketWriter pw = new PacketWriter(EOpCodes.SEND_INPUT);

        pw.Write((int)keyCode);
        pw.Write(isPressed);

        NetworkManager.NM.SendPacket(pw.GetBytes(), ReliableNetcode.QosType.Reliable);
    }

#if (!CLIENT)
    public void HandlePacket(RemoteClient client, PacketReader stream)
    {
        Player player = Server.players[client];

        KeyCode key = (KeyCode)stream.ReadInt32();
        bool isPressed = stream.ReadBoolean();
        
        if (isPressed)
        {
            switch (key)
            {
                case KeyCode.W:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4);
                    break;
                case KeyCode.A:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 4);
                    break;
                case KeyCode.S:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.down * 4);
                    break;
                case KeyCode.D:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 4);
                    break;
            }
        }
        else
        {
            switch (key)
            {
                case KeyCode.W:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 4);
                    break;
                case KeyCode.A:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 4);
                    break;
                case KeyCode.S:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.down * 4);
                    break;
                case KeyCode.D:
                    player.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 4);
                    break;
            }
        }
    }

#endif

}
