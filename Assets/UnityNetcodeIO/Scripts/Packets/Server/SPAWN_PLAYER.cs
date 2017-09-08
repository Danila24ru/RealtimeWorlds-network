using NetcodeIO.NET;
using PacketIO;
using UnityEngine;


public class SPAWN_PLAYER : IClientPacketHandler
{
#if (!CLIENT)
    /// <summary>
    /// Отправить клиенту информацию о спавне персонажа по clientID
    /// </summary>
    /// <param name="client">Кому отправить</param>
    /// <param name="clientID">Кого спавнить</param>
    /// <param name="position">позиция</param>
    /// <param name="rotation">вращение</param>
    public static void Send(RemoteClient client, ulong clientID, Vector3 position, Quaternion rotation)
    {
        PacketWriter pw = new PacketWriter(EOpCodes.SPAWN_PLAYER);

        pw.Write(clientID);
        pw.Write(position.x);
        pw.Write(position.y);
        pw.Write(position.z);
        pw.Write(rotation.x);
        pw.Write(rotation.y);
        pw.Write(rotation.z);
        pw.Write(rotation.w);

        GameObject playerGO = Resources.Load<GameObject>("Character/Player");
        GameObject instPlayerObj = Object.Instantiate(playerGO);

        Player player = instPlayerObj.GetComponent<Player>();
        player.clientID = client.ClientID;
        instPlayerObj.transform.position = new Vector3(0, 0, 0);

        //добавляем клиента в словарь и спавним его
        Server.players.Add(client, player);

        //Если на серваке больше чем 1 чел, начинаем массовую рассылку
        if (Server.players.Count > 1)
        {
            //спавним всех существующих игроков у подключенного игрока
            foreach (var plr in Server.players)
            {
                PacketWriter writer = new PacketWriter(EOpCodes.SPAWN_PLAYER);

                writer.Write(plr.Value.clientID);
                writer.Write(plr.Value.transform.position.x);
                writer.Write(plr.Value.transform.position.y);
                writer.Write(plr.Value.transform.position.z);
                writer.Write(plr.Value.transform.rotation.x);
                writer.Write(plr.Value.transform.rotation.y);
                writer.Write(plr.Value.transform.rotation.z);
                writer.Write(plr.Value.transform.rotation.w);

                Server.SendMessageSingle(client, writer.GetBytes(), ReliableNetcode.QosType.Reliable);
                writer.Close();
            }
            Server.SendMessageNonOwner(client, pw.GetBytes(), ReliableNetcode.QosType.Reliable);
        }
        else
        {
            Server.SendMessageSingle(client, pw.GetBytes(), ReliableNetcode.QosType.Reliable);
        }
    }



#endif

    public void HandlePacket(PacketReader stream)
    {
        ulong clientID = stream.ReadUInt64();
        Vector3 position = new Vector3(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        Quaternion rotation = new Quaternion(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());

        GameObject playerGO = Resources.Load<GameObject>("Character/Player");
        GameObject instPlayerObj = UnityEngine.Object.Instantiate(playerGO);
        Player player = instPlayerObj.GetComponent<Player>();
        player.clientID = clientID;
        instPlayerObj.transform.position = position;
        instPlayerObj.transform.rotation = rotation;

        NetworkManager.NM.players.Add(player.clientID, player);
        Debug.Log("Instantiate player with ID:" + player.clientID);
    }
}

