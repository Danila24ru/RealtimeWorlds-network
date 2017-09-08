using UnityEngine;
using System.Collections;
using PacketIO;
using System;
using NetcodeIO.NET;

public class PLAYER_DISCONNECTED : IClientPacketHandler
{
#if (!CLIENT)
    public static void Send(RemoteClient client)
    {
        PacketWriter pw = new PacketWriter(EOpCodes.PLAYER_DISCONNECTED);

        pw.Write(client.ClientID);

        UnityEngine.Object.Destroy(Server.players[client].gameObject);
        Server.players.Remove(client);
        Server.endpointsByClient[client].Reset();
        Server.endpointsByClient.Remove(client);

        Server.SendMessageNonOwner(client.ClientID, pw.GetBytes(), ReliableNetcode.QosType.Reliable);
    }
#endif

    public void HandlePacket(PacketReader stream)
    {
        ulong clientID = stream.ReadUInt64();

        if (NetworkManager.NM.players.ContainsKey(clientID))
        {
            UnityEngine.Object.Destroy(NetworkManager.NM.players[clientID].gameObject);
            Debug.Log("Player " + clientID + "disconnected.");
            NetworkManager.NM.players.Remove(clientID);
        }
    }
}
