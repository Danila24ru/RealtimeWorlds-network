using NetcodeIO.NET;
using PacketIO;

public enum EOpCodes : ushort
{
    //C2S
    SEND_INPUT,

    //S2C
    SEND_PLAYER_POSITION,
    SPAWN_PLAYER,
    PLAYER_DISCONNECTED
}

public interface IServerPacketHandler
{
    void HandlePacket(RemoteClient client, PacketReader stream);
}

public interface IClientPacketHandler
{
    void HandlePacket(PacketReader stream);
}
