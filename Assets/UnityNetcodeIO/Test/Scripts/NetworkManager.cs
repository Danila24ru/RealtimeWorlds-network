using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityNetcodeIO;
using NetcodeIO.NET;
using Assets.UnityNetcodeIO.Scripts.Utils;
using System.Net;
using UnityEngine.UI;
using ReliableNetcode;
using PacketIO;

public class NetworkManager : MonoBehaviour {

    //Singleton
    public static NetworkManager NM = null;

    public InputField ipServerText;
    public InputField idClientText;
    public InputField nameClientText;

    static byte[] privateKey = new byte[]
    {
        0x60, 0x6a, 0xbe, 0x6e, 0xc9, 0x19, 0x10, 0xea,
        0x9a, 0x65, 0x62, 0xf6, 0x6f, 0x2b, 0x30, 0xe4,
        0x43, 0x71, 0xd6, 0x2c, 0xd1, 0x99, 0x27, 0x26,
        0x6b, 0x3c, 0x60, 0xf4, 0xb7, 0x15, 0xab, 0xa1,
    };

    private static ulong ProtocolID = 0x1122334455667788L;

    public ulong localClientID = 1UL;

    public static string serverIP = "192.168.1.36";
    public static int serverPORT = 44444;

    IPEndPoint[] ipEndPoint = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse(serverIP), serverPORT)};

    bool isConnected = false;

    // <ClientID, Player>
    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

    Dictionary<EOpCodes, IClientPacketHandler> packetHandlers = new Dictionary<EOpCodes, IClientPacketHandler>();

    ReliableEndpoint reliableEndpoint = new ReliableEndpoint();

    private NetcodeClient client;

    void Awake()
    {
        if (NM == null)
        {
            DontDestroyOnLoad(gameObject);
            NM = this;
        }
        else
        {
            if (NM != this)
            {
                Destroy(gameObject);
            }
        }
    }
    
	void Start () {
        RegisterHandlers();

        UnityNetcode.QuerySupport((supportStatus) =>
        {
            if (supportStatus == NetcodeIOSupportStatus.Available)
            {
                Debug.LogError("Netcode.IO available and ready!");

                UnityNetcode.CreateClient(NetcodeIOClientProtocol.IPv4, (client) =>
                {
                    this.client = client;
                    client.SetTickrate(60);
                    //StartCoroutine(connectToServer());

                });
            }
            else if (supportStatus == NetcodeIOSupportStatus.Unavailable)
            {
                Debug.LogError("Netcode.IO not available");
            }
            else if (supportStatus == NetcodeIOSupportStatus.HelperNotInstalled)
            {
                Debug.LogError("Netcode.IO is available, but native helper is not installed");
            }
        });
	}
	
    //Connection...
	public void connectToServer()
    {
        localClientID = ulong.Parse(idClientText.text);
        serverIP = ipServerText.text;
         
        ipEndPoint = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse(serverIP), serverPORT) };

        Debug.LogError("Generating token on client...");
        TokenFactory factory = new TokenFactory(ProtocolID, privateKey);
        byte[] connectToken = factory.GenerateConnectToken(ipEndPoint,300, 100, 10UL, localClientID, new byte[256]);
        
        client.Connect(connectToken, () =>
        {
            Debug.LogError("Client connected to server !!!");
            
            StartCoroutine(updateStatus());
            client.AddPayloadListener(RecievePacket);

            reliableEndpoint.ReceiveCallback = (message, messageSize) =>
            {
                RecieveMessage(message, messageSize);
            };
            reliableEndpoint.TransmitCallback = (payload, size) =>
            {
                client.Send(payload, size);
            };

            isConnected = true;

        }, (error) =>
        {
            Debug.LogError("FAILED CONNECTION: " + error);
        });
    }


    #region PacketsWorkflow 
    void RecievePacket(NetcodeClient client, NetcodePacket packet)
    {
        reliableEndpoint.ReceivePacket(packet.PacketBuffer.InternalBuffer, packet.PacketBuffer.Length);
    }

    void RecieveMessage(byte[] message, int messageSize)
    {
        PacketReader pr = new PacketReader(message);
        
        if (packetHandlers.ContainsKey(pr.header))
            packetHandlers[pr.header].HandlePacket(pr);
        else
            Debug.Log("wrong packet");

        pr.Close();
    }

    public void SendPacket(byte[] data, QosType qos)
    {
        reliableEndpoint.SendMessage(data, data.Length, qos);
    }
    public void SendPacket(byte[] data, int size, QosType qos)
    {
        reliableEndpoint.SendMessage(data, size, qos);
    }
    #endregion

    void Update()
    {
        if(isConnected == true)
            reliableEndpoint.Update();
    }

    IEnumerator updateStatus()
    {
        while (true)
        {
            client.QueryStatus((status) =>
            {
                
            });

            yield return new WaitForSeconds(0.1f);
        }
    }

    void RegisterHandlers()
    {
        packetHandlers.Add(EOpCodes.PLAYER_DISCONNECTED, new PLAYER_DISCONNECTED());
        packetHandlers.Add(EOpCodes.SEND_PLAYER_POSITION, new SEND_PLAYER_POSITION());
        packetHandlers.Add(EOpCodes.SPAWN_PLAYER, new SPAWN_PLAYER());
    }

    private void OnDestroy()
    {
        if (client != null)
            UnityNetcode.DestroyClient(client);
    }

    void OnApplicationQuit()
    {
        if (client != null)
            UnityNetcode.DestroyClient(client);
    }

}
