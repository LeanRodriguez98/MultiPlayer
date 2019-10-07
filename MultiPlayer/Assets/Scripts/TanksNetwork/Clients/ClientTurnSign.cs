using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class ClientTurnSign : MonoBehaviour {

    private Text turnSign;
    private Text clockSign;
    public void SetTurnSign(Text _turnSign)
    {
        turnSign = _turnSign;
    }
    public void SetClockSign(Text _clockSign)
    {
        clockSign = _clockSign;
    }
    public void Init()
    {
        PacketManager.Instance.AddListener(ObjectsID.turnSignObjectID, OnReceivePacket);
        PacketManager.Instance.AddListener(ObjectsID.clockSignObjectID, OnReceivePacket);
    }

    void OnDisable()
    {
        PacketManager.Instance.RemoveListener(ObjectsID.turnSignObjectID);
        PacketManager.Instance.RemoveListener(ObjectsID.clockSignObjectID);
    }
    void OnReceivePacket(uint packetId, ushort type, Stream stream)
    {
        switch (type)
        {
            case (ushort)UserPacketType.TurnSign:
                TurnSignPacket turnSignPacket = new TurnSignPacket();
                turnSignPacket.Deserialize(stream);
                turnSign.text = turnSignPacket.payload;
                break;
            case (ushort)UserPacketType.Clock:
                ClockSignPacket clockSignPacket = new ClockSignPacket();
                clockSignPacket.Deserialize(stream);
                clockSign.text = clockSignPacket.payload;
                break;
        }
    }
}
