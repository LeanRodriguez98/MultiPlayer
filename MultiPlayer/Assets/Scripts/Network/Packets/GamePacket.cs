using System.IO;
using UnityEngine;

public abstract class GamePacket<P> : NetworkPacket<P>
{
    public readonly bool reliable;
    public GamePacket(PacketType packetType, bool reliable = false) : base(packetType)
    {
        this.reliable = reliable;
    }
}

public abstract class OrderedGamePacket<P> : OrderedNetworkPacket<P>
{
    public readonly bool reliable;
    public OrderedGamePacket(PacketType packetType, bool reliable = false) : base(packetType)
    {
        this.reliable = reliable;
    }
}

public class MessagePacket : GamePacket<string>
{
    public MessagePacket() : base(global::PacketType.User, true)
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
    public RotationPacket() : base(PacketType.User)
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

public class FloatPacket : GamePacket<float>
{
    public FloatPacket() : base(global::PacketType.User)
    {
        userPacketType = (ushort)UserPacketType.Float;
    }

    public override void OnSerialize(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(payload);
    }

    public override void OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        payload = binaryReader.ReadSingle();
    }
}
public class BulletPacket : OrderedGamePacket<float[]> // Al ser uun ordered, si serializa y retorna un id
{
    public BulletPacket() : base(global::PacketType.User, true)
    {
        userPacketType = (ushort)UserPacketType.Bullet;
    }

    public override void OnSerialize(Stream stream, uint id)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(id);// lo primero que se escribe en el paquete es su ID
        binaryWriter.Write(payload[0]);
        binaryWriter.Write(payload[1]);
        binaryWriter.Write(payload[2]);
    }

    public override uint OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        uint id = binaryReader.ReadUInt32();
        payload = new float[3];
        payload[0] = binaryReader.ReadSingle();
        payload[1] = binaryReader.ReadSingle();
        payload[2] = binaryReader.ReadSingle();

        return id;//y deserializar retorna el ID
    }
}

public class TankPacket : OrderedGamePacket<float[]>
{
    public TankPacket() : base(global::PacketType.User, true)
    {
        userPacketType = (ushort)UserPacketType.Player;
    }

    public override void OnSerialize(Stream stream, uint id)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(id);
        binaryWriter.Write(payload[0]);//pos x
        binaryWriter.Write(payload[1]);//pos y
        binaryWriter.Write(payload[2]);//rot x
        binaryWriter.Write(payload[3]);//rot y
        binaryWriter.Write(payload[4]);//rot z
        binaryWriter.Write(payload[5]);//rot w
        binaryWriter.Write(payload[6]);//time
    }

    public override uint OnDeserialize(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        uint id = binaryReader.ReadUInt32();
        payload = new float[7];
        payload[0] = binaryReader.ReadSingle();
        payload[1] = binaryReader.ReadSingle();
        payload[2] = binaryReader.ReadSingle();
        payload[3] = binaryReader.ReadSingle();
        payload[4] = binaryReader.ReadSingle();
        payload[5] = binaryReader.ReadSingle();
        payload[6] = binaryReader.ReadSingle();

        return id;
    }
}



