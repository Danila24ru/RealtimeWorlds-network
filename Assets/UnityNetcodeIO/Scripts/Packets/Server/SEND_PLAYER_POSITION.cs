using PacketIO;
using UnityEngine;

public class SEND_PLAYER_POSITION : IClientPacketHandler
{
#if (!CLIENT)
    public static void Send(ulong clientID, Vector3 position, Quaternion rotation)
    {
        PacketWriter pw = new PacketWriter(EOpCodes.SEND_PLAYER_POSITION);

        pw.Write(clientID);
        pw.Write(position.x);
        pw.Write(position.y);
        pw.Write(position.z);
        pw.Write(rotation.x);
        pw.Write(rotation.y);
        pw.Write(rotation.z);
        pw.Write(rotation.w);

        Server.SendMessageBroadcast(pw.GetBytes(), ReliableNetcode.QosType.Unreliable);
    }
#endif

    public void HandlePacket(PacketReader stream)
    {
        ulong clientID = stream.ReadUInt64();
        Vector3 position = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        Quaternion rotation = new Quaternion(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());

        if (!NetworkManager.NM.players.ContainsKey(clientID))
            return;

        NetworkManager.NM.players[clientID].transform.position = position;
        NetworkManager.NM.players[clientID].transform.rotation = rotation;
    }
}