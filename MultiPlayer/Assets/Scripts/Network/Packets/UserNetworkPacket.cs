using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum UserPacketType
{
    Message,
    Position,
    Int,
    Float,
    Player,
    Bullet,
    Rotation,
    Clock,
    PlayerUI,
    TurnSign,
    Count
}

public class UserPacketHeader : ISerializePacket
{
    public uint senderId;
    public uint objectId;

    public ushort packetType { get; set; }

    public void Serialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);

        binaryWriter.Write(senderId);
        binaryWriter.Write(objectId);
        binaryWriter.Write(packetType);

        OnSerialize(stream);
    }

    public void Deserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);

        senderId = binaryReader.ReadUInt32();
        objectId = binaryReader.ReadUInt32();
        packetType = binaryReader.ReadUInt16();

        OnDeserialize(stream);
    }

    virtual protected void OnSerialize(Stream stream) { }
    virtual protected void OnDeserialize(Stream stream) { }
}