using System.Collections.Generic;
using System.IO;
using System.Net;

public class PacketManager : MBSingleton<PacketManager>, IReceiveData
{
    Dictionary<uint, System.Action<uint, ushort, Stream>> onPacketReceived = new Dictionary<uint, System.Action<uint, ushort, Stream>>();
    uint currentPacketId = 0;

    protected override void Awake()
    {
        base.Awake();
        NetworkManager.Instance.OnReceiveEvent += OnReceiveData;
    }

    public void AddListener(uint ownerId, System.Action<uint, ushort, Stream> callback)
    {
        if (!onPacketReceived.ContainsKey(ownerId))
            onPacketReceived.Add(ownerId, callback);
    }

    public void RemoveListener(uint ownerId)
    {
        if (onPacketReceived.ContainsKey(ownerId))
            onPacketReceived.Remove(ownerId);
    }

    public void SendPacket<T>(NetworkPacket<T> packet, uint objectId, bool reliable = false)
    {
        byte[] bytes = Serialize(packet, objectId);

        if (ConnectionManager.Instance.isServer)
            NetworkManager.Instance.Broadcast(bytes);
        else
            NetworkManager.Instance.SendToServer(bytes);
    }

    public void SendPacket<T>(NetworkPacket<T> packet, bool reliable = false)
    {
        byte[] bytes = Serialize(packet);

        if (ConnectionManager.Instance.isServer)
            NetworkManager.Instance.Broadcast(bytes);
        else
            NetworkManager.Instance.SendToServer(bytes);
    }

    public void SendPacket<T>(NetworkPacket<T> packet, IPEndPoint ipEndPoint, bool reliable = false)
    {
        byte[] bytes = Serialize(packet);

        NetworkManager.Instance.SendToClient(bytes, ipEndPoint);
    }

    byte[] Serialize<T>(NetworkPacket<T> packet)
    {
        PacketHeader header = new PacketHeader();
        MemoryStream stream = new MemoryStream();

        header.protocolId = 0;
        header.packetType = packet.packetType;

        header.Serialize(stream);
        packet.Serialize(stream);

        stream.Close();

        return stream.ToArray();
    }

    byte[] Serialize<T>(NetworkPacket<T> packet, uint objectId)
    {
        PacketHeader header = new PacketHeader();
        UserPacketHeader userHeader = new UserPacketHeader();
        MemoryStream stream = new MemoryStream();

        header.protocolId = 0;
        header.packetType = packet.packetType;

        if (packet.packetType == PacketType.User)
        {
            userHeader.packetType = packet.userPacketType;
            userHeader.packetId = currentPacketId++;
            userHeader.senderId = NetworkManager.Instance.clientId;
            userHeader.objectId = objectId;
        }

        header.Serialize(stream);
        userHeader.Serialize(stream);
        packet.Serialize(stream);

        stream.Close();

        return stream.ToArray();
    }

    public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
    {
        PacketHeader header = new PacketHeader();
        MemoryStream stream = new MemoryStream(data);

        header.Deserialize(stream);

        if (header.packetType == PacketType.User)
        {
            UserPacketHeader userHeader = new UserPacketHeader();
            userHeader.Deserialize(stream);
            InvokeCallback(userHeader.objectId, userHeader.packetId, userHeader.packetType, stream);
        }
        else
        {
            ConnectionManager.Instance.OnReceivePacket(ipEndpoint, header.packetType, stream);
        }

        stream.Close();
    }

    void InvokeCallback(uint objectId, uint packetId, ushort packetType, Stream stream)
    {
        if (onPacketReceived.ContainsKey(objectId))
            onPacketReceived[objectId].Invoke(packetId, packetType, stream);
    }
}