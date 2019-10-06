using UnityEngine;

public class MessageManager : Singleton<MessageManager>
{
    protected override void Initialize()
    {
        base.Initialize();
    }

    public void SendString(string message, uint objectId)
    {
        MessagePacket packet = new MessagePacket();

        packet.payload = message;
        UnityEngine.Debug.Log(message + "   --> " + objectId);
        PacketManager.Instance.SendPacket(packet, objectId);
    }

    public void SendPosition(Vector3 position, uint objectId)
    {
        PositionPacket packet = new PositionPacket();

        packet.payload = position;

        PacketManager.Instance.SendPacket(packet, objectId);
    }

    public void SendRotation(Quaternion rotation, uint objectId)
    {
        RotationPacket packet = new RotationPacket();

        packet.payload = rotation;

        PacketManager.Instance.SendPacket(packet, objectId);
    }

    public void SendInt(int number, uint objectId)
    {
        IntPacket packet = new IntPacket();

        packet.payload = number;

        PacketManager.Instance.SendPacket(packet, objectId);
    }
}