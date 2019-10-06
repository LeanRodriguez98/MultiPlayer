using UnityEngine;
using System.IO;

public abstract class GamePacket<P> : NetworkPacket<P>
{
    public GamePacket(PacketType packetType) : base(packetType) { }
}

public class MessagePacket : GamePacket<string>
{
    public MessagePacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.Message;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload);
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload = binaryReader.ReadString();
    }
}

public class PositionPacket : GamePacket<Vector3>
{
    public PositionPacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.Position;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload.x);
        binaryWriter.Write(payload.y);
        binaryWriter.Write(payload.z);
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload.x = binaryReader.ReadSingle();
        payload.y = binaryReader.ReadSingle();
        payload.z = binaryReader.ReadSingle();
    }
}

public class RotationPacket : GamePacket<Quaternion>
{
    public RotationPacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.Rotation;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload.x);
        binaryWriter.Write(payload.y);
        binaryWriter.Write(payload.z);
        binaryWriter.Write(payload.w);
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload.x = binaryReader.ReadSingle();
        payload.y = binaryReader.ReadSingle();
        payload.z = binaryReader.ReadSingle();
        payload.w = binaryReader.ReadSingle();
    }
}

public class PlayerUIPacket : GamePacket</*UI_Canvas.PlayerUIData*/string>
{
    public PlayerUIPacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.PlayerUI;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        //string buffer = payload.playerHealt.text + '?' + payload.bulletSpeed.text + '?' + payload.cannonAngle.text;
        binaryWriter.Write(payload);
        //語
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload = binaryReader.ReadString();

        /*string buffer = binaryReader.ReadString();
        string[] texts = buffer.Split(new char[] { '?' });
        payload.
            playerHealt.
            text 
            = new string(texts[0].ToCharArray());
        payload.
            bulletSpeed.
            text 
            = new string(texts[1].ToCharArray());
        payload.
            bulletSpeed.
            text 
            = new string(texts[2].ToCharArray());*/
    }
}

public class TurnSignPacket : GamePacket<string>
{
    public TurnSignPacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.TurnSign;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload);
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload = binaryReader.ReadString();
    }
}

public class ClockSignPacket : GamePacket<string>
{
    public ClockSignPacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.Clock;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload);
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload = binaryReader.ReadString();
    }
}

public class IntPacket : GamePacket<int>
{
    public IntPacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.Int;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload);
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload = binaryReader.ReadInt32();
    }
}