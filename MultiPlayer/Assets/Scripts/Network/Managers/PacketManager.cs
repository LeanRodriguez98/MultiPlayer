using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;

public class PacketManager : MBSingleton<PacketManager>, IReceiveData
{
    private Dictionary<uint, System.Action<ushort, Stream>> onPacketReceived = new Dictionary<uint, Action<ushort, Stream>>();
    public bool testCheckSum = false;
    public bool testReliable = false;
    protected override void Awake()
    {
        base.Awake();
        NetworkManager.Instance.OnReceiveEvent += OnReceiveData;
    }

    public void AddListener(uint ownerId, Action<ushort, Stream> callback)
    {
        if (!onPacketReceived.ContainsKey(ownerId))
        {
            onPacketReceived.Add(ownerId, callback);
        }
    }

    public void RemoveListener(uint ownerId)
    {
        if (onPacketReceived.ContainsKey(ownerId))
        {
            onPacketReceived.Remove(ownerId);
        }
    }

    public byte[] WrapCheckSumPacket(byte[] packet)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(stream);

        using (MD5 md5Hash = MD5.Create())
        {
            byte[] hash = md5Hash.ComputeHash(packet);
            MemoryStream streamHash = new MemoryStream(hash);
            BinaryReader hashReader = new BinaryReader(streamHash);
            int hash32 = hashReader.ReadInt32();
            binaryWriter.Write(hash32);
        }
        stream.Close();
        byte[] checkSum = stream.ToArray();
        byte[] wrappedBytes = new byte[checkSum.Length + packet.Length];
        checkSum.CopyTo(wrappedBytes, 0);
        packet.CopyTo(wrappedBytes, checkSum.Length);

        return wrappedBytes;
    }

    public byte[] WrapReliabilityPacket(byte[] packet, bool reliable, AcknowledgeChecker acknowledgeChecker, uint acknowledgeId = 0)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(reliable);
        if (reliable)
        {
            binaryWriter.Write(acknowledgeId);
            uint lastAcknowledge;
            uint acknowledgeArray;
            bool hasToConfirm = acknowledgeChecker.GetAcknowledgeConfirmation(out lastAcknowledge, out acknowledgeArray);
            binaryWriter.Write(hasToConfirm);
            if (hasToConfirm)
            {
                binaryWriter.Write(lastAcknowledge);
                binaryWriter.Write(acknowledgeArray);
            }
        }
        stream.Close();
        byte[] reliability = stream.ToArray();
        byte[] wrappedBytes = new byte[reliability.Length + packet.Length];
        reliability.CopyTo(wrappedBytes, 0);
        packet.CopyTo(wrappedBytes, reliability.Length);

        wrappedBytes = WrapCheckSumPacket(wrappedBytes);

        return wrappedBytes;
    }

    public void SendPacket<T>(NetworkPacket<T> packet, uint objectId, bool reliable)
    {
        byte[] bytes = Serialize(packet, objectId);

        if (ConnectionManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(bytes, reliable);
        }
        else
        {
            AcknowledgeChecker acknowledgeChecker = ConnectionManager.Instance.OwnClient.acknowledgeChecker;
            uint acknowledgeId = acknowledgeChecker.NewAcknowledge;
            bytes = WrapReliabilityPacket(bytes, reliable, acknowledgeChecker, acknowledgeId);
            NetworkManager.Instance.SendToServer(bytes);
            if (reliable)
            {
                ConnectionManager.Instance.QueuePacket(bytes, acknowledgeId);
            }
        }
    }

    public void SendPacket<T>(OrderedNetworkPacket<T> packet, uint objectId, bool reliable, uint id)
    {
        byte[] bytes = Serialize(packet, objectId, id);

        if (ConnectionManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(bytes, reliable);
        }
        else
        {
            AcknowledgeChecker acknowledgeChecker = ConnectionManager.Instance.OwnClient.acknowledgeChecker;
            uint acknowledgeId = acknowledgeChecker.NewAcknowledge;
            bytes = WrapReliabilityPacket(bytes, reliable, acknowledgeChecker, acknowledgeId);
            NetworkManager.Instance.SendToServer(bytes);
            if (reliable)
            {
                ConnectionManager.Instance.QueuePacket(bytes, acknowledgeId);
            }
        }
    }

    public void SendPacket<T>(NetworkPacket<T> packet)
    {
        byte[] bytes = Serialize(packet);

        if (ConnectionManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(bytes, false);
        }
        else
        {
            bytes = WrapReliabilityPacket(bytes, false, ConnectionManager.Instance.OwnClient.acknowledgeChecker);
            NetworkManager.Instance.SendToServer(bytes);
        }
    }

    public void SendPacket<T>(NetworkPacket<T> packet, IPEndPoint ipEndPoint)
    {
        byte[] bytes = Serialize(packet);
        bytes = WrapReliabilityPacket(bytes, false, ConnectionManager.Instance.clients[ConnectionManager.Instance.ipToId[ipEndPoint]].acknowledgeChecker);

        NetworkManager.Instance.SendToClient(bytes, ipEndPoint);
    }

    public void SendPacket(byte[] packet)
    {
        if (ConnectionManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(packet);
        }
        else
        {
            NetworkManager.Instance.SendToServer(packet);
        }
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
            userHeader.senderId = NetworkManager.Instance.clientId;
            userHeader.objectId = objectId;
        }

        header.Serialize(stream);
        userHeader.Serialize(stream);
        packet.Serialize(stream);

        stream.Close();

        return stream.ToArray();
    }

    byte[] Serialize<T>(OrderedNetworkPacket<T> packet, uint objectId, uint id)
    {
        PacketHeader header = new PacketHeader();
        UserPacketHeader userHeader = new UserPacketHeader();
        MemoryStream stream = new MemoryStream();

        header.protocolId = 0;

        header.packetType = packet.packetType;

        if (packet.packetType == PacketType.User)
        {
            userHeader.packetType = packet.userPacketType;
            userHeader.senderId = NetworkManager.Instance.clientId;
            userHeader.objectId = objectId;
        }

        header.Serialize(stream);
        userHeader.Serialize(stream);
        packet.Serialize(stream, id);

        stream.Close();

        return stream.ToArray();
    }

    public void OnReceiveData(byte[] data, IPEndPoint ipEndpoint)
    {
        PacketHeader header = new PacketHeader();
        MemoryStream stream = new MemoryStream(data);

        BinaryReader binaryReader = new BinaryReader(stream);

        int hash32 = binaryReader.ReadInt32();

        byte[] dataWithoutHash = new byte[data.Length - 4];
        Array.Copy(data, 4, dataWithoutHash, 0, data.Length - 4);

        if (testCheckSum)
        {
            if (UnityEngine.Random.Range(0f, 100f) < 1f)
            {
                for (int i = 0; i < dataWithoutHash.Length; i++)
                {
                    if (dataWithoutHash[i] != 0)
                        dataWithoutHash[i] = 0;
                }
            }
        }
        int ourHash;
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] hash = md5Hash.ComputeHash(dataWithoutHash);
            MemoryStream hashStream = new MemoryStream(hash);
            BinaryReader hashReader = new BinaryReader(hashStream);
            ourHash = hashReader.ReadInt32();
            hashStream.Close();
        }

        if (hash32 == ourHash)
        {
            bool reliability = binaryReader.ReadBoolean();

            if (reliability)
            {
                if (testReliable)
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        stream.Close();
                        return;
                    }
                }

                uint packageAcknowledge = binaryReader.ReadUInt32();
                bool hasAcknowledge = binaryReader.ReadBoolean();
                if (hasAcknowledge)
                {
                    uint lastAcknowledge = binaryReader.ReadUInt32();
                    uint prevAcknowledgeArray = binaryReader.ReadUInt32();

                    if (ConnectionManager.Instance.isServer)
                    {
                        Client client = ConnectionManager.Instance.clients[ConnectionManager.Instance.ipToId[ipEndpoint]];
                        client.acknowledgeChecker.RegisterPackageReceived(packageAcknowledge);
                        client.acknowledgeChecker.ClearPackets(lastAcknowledge, prevAcknowledgeArray);
                    }
                    else
                    {
                        ConnectionManager.Instance.OwnClient.acknowledgeChecker.RegisterPackageReceived(packageAcknowledge);
                        ConnectionManager.Instance.OwnClient.acknowledgeChecker.ClearPackets(lastAcknowledge, prevAcknowledgeArray);
                    }
                }
            }

            header.Deserialize(stream);

            if (header.packetType == PacketType.User)
            {
                while (stream.Length - stream.Position > 0)
                {
                    UserPacketHeader userHeader = new UserPacketHeader();
                    userHeader.Deserialize(stream);
                    InvokeCallback(userHeader.objectId, userHeader.packetType, stream);
                }
            }
            else
            {
                ConnectionManager.Instance.OnReceivePacket(ipEndpoint, header.packetType, stream);
            }
        }

        stream.Close();
    }

    void InvokeCallback(uint objectId, ushort packetType, Stream stream)
    {
        if (onPacketReceived.ContainsKey(objectId))
        {
            onPacketReceived[objectId].Invoke(packetType, stream);
        }
    }
}