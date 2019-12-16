using System.Net;
using System.IO;

public enum PacketType
{
    ConnectionRequest,
    ChallengeRequest,
    ChallengeResponse,
    Connected,
    User,
    Count
}

public abstract class NetworkPacket<P> : ISerializePacket
{
    public P payload;

    public ushort userPacketType { get; set; }
    public PacketType packetType { get; set; }

    public NetworkPacket(PacketType packetType)
    {
        this.packetType = packetType;
    }

    public void Serialize(Stream stream)
    {
        OnSerialize(stream);
    }

    public void Deserialize(Stream stream)
    {
        OnDeserialize(stream);
    }

    abstract public void OnSerialize(Stream stream);
    abstract public void OnDeserialize(Stream stream);
}

public abstract class OrderedNetworkPacket<P>
{
    public P payload;

    public ushort userPacketType { get; set; }
    public PacketType packetType { get; set; }

    public OrderedNetworkPacket(PacketType packetType)
    {
        this.packetType = packetType;
    }

    public void Serialize(Stream stream, uint id)// la diferencia con el networkPacket comun es que se manda un ID
    {
        OnSerialize(stream, id);
    }

    public uint Deserialize(Stream stream)// y esta funcion lo recive
    {
        return OnDeserialize(stream);
    }

    abstract public void OnSerialize(Stream stream, uint id);
    abstract public uint OnDeserialize(Stream stream);
}

public struct ConnectionRequestData
{
    public ulong clientSalt;
}

public class ConnectionRequestPacket : NetworkPacket<ConnectionRequestData>
{
    public ConnectionRequestPacket() : base(PacketType.ConnectionRequest) { }
    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload.clientSalt = binaryReader.ReadUInt64();
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload.clientSalt);
    }
}

public struct ChallengeRequestData
{
    public uint clientId;
    public ulong clientSalt;
    public ulong serverSalt;
}

public class ChallengeRequestPacket : NetworkPacket<ChallengeRequestData>
{
    public ChallengeRequestPacket() : base(PacketType.ChallengeRequest) { }
    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload.clientId = binaryReader.ReadUInt32();
        payload.clientSalt = binaryReader.ReadUInt64();
        payload.serverSalt = binaryReader.ReadUInt64();
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload.clientId);
        binaryWriter.Write(payload.clientSalt);
        binaryWriter.Write(payload.serverSalt);
    }
}


public struct ChallengeResponseData
{
    public ulong result;
}

public class ChallengeResponsePacket : NetworkPacket<ChallengeResponseData>
{
    public ChallengeResponsePacket() : base(PacketType.ChallengeResponse) { }
    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload.result = binaryReader.ReadUInt64();
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload.result);
    }
}

public struct InitData
{

}

public class ConnectedPacket : NetworkPacket<InitData>
{
    public ConnectedPacket() : base(PacketType.Connected) { }
    public override void OnDeserialize(Stream stream) { }
    public override void OnSerialize(Stream stream) { }
}