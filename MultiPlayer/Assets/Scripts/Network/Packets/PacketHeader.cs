using System.IO;

public class PacketHeader : ISerializePacket
{
    public uint protocolId;
    public PacketType packetType { get; set; }

    public void Serialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(protocolId);
        binaryWriter.Write((ushort)packetType);
        OnSerialize(stream);
    }

    public void Deserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        protocolId = binaryReader.ReadUInt32();
        packetType = (PacketType)binaryReader.ReadUInt16();
        OnDeserialize(stream);
    }

    virtual protected void OnSerialize(Stream stream) { }
    virtual protected void OnDeserialize(Stream stream) { }
}