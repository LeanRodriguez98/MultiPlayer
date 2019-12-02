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
        PacketManager.Instance.SendPacket(packet, objectId, false);
    }

    public void SendPosition(Vector3 position, uint objectId)
    {
        PositionPacket packet = new PositionPacket();


        packet.payload = position;

        PacketManager.Instance.SendPacket(packet, objectId, packet.reliable);
    }

    public void SendRotation(Quaternion rotation, uint objectId)
    {
        RotationPacket packet = new RotationPacket();

        packet.payload = rotation;

        PacketManager.Instance.SendPacket(packet, objectId, false);
    }
    public void SendClockSing(string clockContent, uint objectId)
    {
        ClockSignPacket packet = new ClockSignPacket();
        packet.payload = clockContent;
        PacketManager.Instance.SendPacket(packet, objectId, false);
    }

    public void SendPlayerUI(/*UI_Canvas.PlayerUIData*/string playerUIData, uint objectId)
    {
        PlayerUIPacket packet = new PlayerUIPacket();
        packet.payload = playerUIData;
        PacketManager.Instance.SendPacket(packet, objectId, false);
    }

    public void SendTurnSing(string singContent, uint objectId)
    {
        TurnSignPacket packet = new TurnSignPacket();
        packet.payload = singContent;
        PacketManager.Instance.SendPacket(packet, objectId, false);
    }


    public void SendBallPosition(float[] ballPosition, uint objectId, uint id)
    {
        BulletPacket packet = new BulletPacket();
        packet.payload = ballPosition;
        PacketManager.Instance.SendPacket(packet, objectId, packet.reliable, id);
    }

    public void SendTankData(float[] tankData, uint objectId, uint id)
    {
        TankPacket packet = new TankPacket();
        packet.payload = tankData;
        PacketManager.Instance.SendPacket(packet, objectId, packet.reliable, id);
    }

    public void SendInt(int number, uint objectId)
    {
        IntPacket packet = new IntPacket();
        packet.payload = number;
        PacketManager.Instance.SendPacket(packet, objectId, false);
    }


  
}