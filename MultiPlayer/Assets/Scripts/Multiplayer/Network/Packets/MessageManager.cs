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
        PacketManager.Instance.SendPacket(packet, objectId);
    }

    public void SendTankTurn(bool turnState, uint objectId)
    {
        BoolPackage packet = new BoolPackage();

        packet.payload = turnState;

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

    public void SendPlayerUI(/*UI_Canvas.PlayerUIData*/string playerUIData, uint objectId)
    {
        PlayerUIPacket packet = new PlayerUIPacket();

        packet.payload = playerUIData;

        PacketManager.Instance.SendPacket(packet, objectId);
    }

    public void SendTurnSing(string singContent, uint objectId)
    {
        TurnSignPacket packet = new TurnSignPacket();
        packet.payload = singContent;
        PacketManager.Instance.SendPacket(packet, objectId);
    }

    public void SendClockSing(string clockContent, uint objectId)
    {
        ClockSignPacket packet = new ClockSignPacket();
        packet.payload = clockContent;
        PacketManager.Instance.SendPacket(packet, objectId);
    }
}