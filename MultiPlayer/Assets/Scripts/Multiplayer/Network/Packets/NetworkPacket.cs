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

public struct ConnectionRequestData {
    public long clientSalt;
}

public class ConnectionRequestPacket : NetworkPacket<ConnectionRequestData>
{
    public ConnectionRequestPacket() : base(PacketType.ConnectionRequest) { }
    public override void OnDeserialize(Stream stream) {}
    public override void OnSerialize(Stream stream) {}
}

public struct ChallengeRequestData {
    public long clientId;
    public long clientSalt;
    public long serverSalt;
}

public class ChallengeRequestPacket : NetworkPacket<ChallengeRequestData>
{
    public ChallengeRequestPacket() : base(PacketType.ChallengeRequest) { }
    public override void OnDeserialize(Stream stream) {}
    public override void OnSerialize(Stream stream) {}
}


public struct ChallengeResponseData {
    public long result;
}

public class ChallengeResponsePacket : NetworkPacket<ChallengeResponseData>
{
    public ChallengeResponsePacket() : base(PacketType.ChallengeResponse) { }
    public override void OnDeserialize(Stream stream) {}
    public override void OnSerialize(Stream stream) {}
}
