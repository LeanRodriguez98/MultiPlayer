using System.IO;

public interface ISerializePacket
{
    void Serialize(Stream stream);
    void Deserialize(Stream stream);
}