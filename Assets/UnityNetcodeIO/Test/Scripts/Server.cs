
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ReliableNetcode;
using UnityNetcodeIO;
using NetcodeIO.NET;
using Assets.UnityNetcodeIO.Scripts.Utils;
using PacketIO;

#if (!CLIENT)
public class Server : MonoBehaviour
{
	static readonly byte[] privateKey = new byte[]
	{
		0x60, 0x6a, 0xbe, 0x6e, 0xc9, 0x19, 0x10, 0xea,
		0x9a, 0x65, 0x62, 0xf6, 0x6f, 0x2b, 0x30, 0xe4,
		0x43, 0x71, 0xd6, 0x2c, 0xd1, 0x99, 0x27, 0x26,
		0x6b, 0x3c, 0x60, 0xf4, 0xb7, 0x15, 0xab, 0xa1,
	};
    private static ulong ProtocolID = 0x1122334455667788L;

    public Text outputText;
	public Text NumClientsText;

	public string PublicIP = "192.168.0.102";
	public int Port = 44444;
	public int MaxClients = 256;

	private NetcodeServer server;
    private int clients = 0;

    public static Dictionary<RemoteClient, Player> players = new Dictionary<RemoteClient, Player>();

    public static Dictionary<RemoteClient, ReliableEndpoint> endpointsByClient = new Dictionary<RemoteClient, ReliableEndpoint>();

    Dictionary<EOpCodes, IServerPacketHandler> packetHandlers = new Dictionary<EOpCodes, IServerPacketHandler>();


	private void Start()
	{
        RegisterPackets();
        
        server = UnityNetcode.CreateServer(PublicIP, Port, ProtocolID, MaxClients, privateKey);
        server.internalServer.LogLevel = NetcodeLogLevel.Debug;

        server.ClientConnectedEvent.AddListener(Server_OnClientConnected);
		server.ClientDisconnectedEvent.AddListener(Server_OnClientDisconnected);
		server.ClientMessageEvent.AddListener(Server_OnClientMessage);

        server.internalServer.Tickrate = 60;
        server.StartServer();

        logLine("Server started");
	}

	private void OnDestroy()
	{
		server.Dispose();

	}

	private void Server_OnClientMessage(RemoteClient client, ByteBuffer payload)
	{
        if(endpointsByClient.ContainsKey(client))
            endpointsByClient[client].ReceivePacket(payload.InternalBuffer, payload.Length);
	}

    /// <summary>
    /// Сюда будут приходить готовые пакеты.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="size"></param>
    private void MessageHandler(RemoteClient client, byte[] buffer, int size)
    {
        PacketReader pr = new PacketReader(buffer);

        if (packetHandlers.ContainsKey(pr.header))
            packetHandlers[pr.header].HandlePacket(client, pr);
        else
            Debug.Log("wrong packet");

        pr.Close();
    }

    IEnumerator updateSender()
    {
        while(true)
        {
            foreach(var plr in players)
            {
                SEND_PLAYER_POSITION.Send(plr.Key.ClientID, plr.Value.transform.position, plr.Value.transform.rotation);
            }
            yield return new WaitForSeconds(0.032f); //60 gz = 0.016
        }
    }

	private void Server_OnClientConnected(RemoteClient client)
	{
		logLine("Client connected: " + client.RemoteEndpoint.ToString());
		clients++;
		NumClientsText.text = clients.ToString() + "/" + MaxClients.ToString();
        
        //**** ***** **** ****
        ReliableEndpoint reliableEndpoint = new ReliableEndpoint();

        reliableEndpoint.TransmitCallback = (payload, size) =>
        {
            client.SendPayload(payload, size);
        };
        reliableEndpoint.ReceiveCallback = (message, messageSize) =>
        {
            MessageHandler(client, message, messageSize);
        };
        endpointsByClient.Add(client, reliableEndpoint);
        //**** ** **** *** ****

        //NetworkSpawnPlayer(client);
        SPAWN_PLAYER.Send(client, client.ClientID, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));

        //if have 1 player -> start send position data to clients
        if(players.Count == 1)
            StartCoroutine(updateSender());
	}

	private void Server_OnClientDisconnected(RemoteClient client)
	{
		logLine("Client disconnected: " + client.RemoteEndpoint.ToString());

		clients--;
		NumClientsText.text = clients.ToString() + "/" + MaxClients.ToString();

        /*Destroy(players[client].gameObject);
        players.Remove(client);
        endpointsByClient[client].Reset();
        endpointsByClient.Remove(client);
        */
        PLAYER_DISCONNECTED.Send(client);

        //if less than 1 player -> stop send position data to clients
        if (players.Count < 1)
            StopCoroutine(updateSender());
	}

    public static void SendMessageSingle(RemoteClient client, byte[] message, QosType qos)
    {
        endpointsByClient[client].SendMessage(message, message.Length, qos);
    }
    public static void SendMessageBroadcast(byte[] message, QosType qos)
    {
        foreach(var endpoint in endpointsByClient)
        {
            endpoint.Value.SendMessage(message, message.Length, qos);
        }
    }
    public static void SendMessageNonOwner(ulong clientID, byte[] message, QosType qos)
    {
        foreach(var endpoint in endpointsByClient)
        {
            if (clientID != endpoint.Key.ClientID)
                endpoint.Value.SendMessage(message, message.Length, qos);
        }
    }
    public static void SendMessageNonOwner(RemoteClient client, byte[] message, QosType qos)
    {
        foreach (var endpoint in endpointsByClient)
        {
            if (client.ClientID != endpoint.Key.ClientID)
                endpoint.Value.SendMessage(message, message.Length, qos);
        }
    }

 /*   void NetworkSpawnPlayer(RemoteClient client)
    {
        GameObject playerGO = Resources.Load<GameObject>("Character/Player");
        GameObject instPlayerObj = Instantiate(playerGO);

        Player player = instPlayerObj.GetComponent<Player>();
        player.clientID = client.ClientID;
        instPlayerObj.transform.position = new Vector3(0, 0, 0);

        //добавляем клиента в словарь и спавним его
        players.Add(client, player);

        if (players.Count > 1)
        {
            foreach (var plr in players)
            {
                SPAWN_PLAYER.Send(client, plr.Key.ClientID, plr.Value.transform.position, plr.Value.transform.rotation);
            }
            SPAWN_PLAYER.SendNonOwner(client.ClientID, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
        }
        else
        {
            SPAWN_PLAYER.Send(client, client.ClientID, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
        }
    }*/

    void Update()
    {
        if(endpointsByClient.Count > 0)
            foreach (var endpoint in endpointsByClient)
                endpoint.Value.Update();
    }


    void RegisterPackets()
    {
        packetHandlers.Add(EOpCodes.SEND_INPUT, new SEND_INPUT());
    }

	protected void log(string text)
	{
		outputText.text += text;
	}
	protected void logLine(string text)
	{
		log(text + "\n");
	}
}
#endif