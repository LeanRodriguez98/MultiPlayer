using System.IO;
using UnityEngine;
public class ClientBullet : MonoBehaviour
{
    void OnEnable()
    {
        PacketManager.Instance.Awake();
        PacketManager.Instance.AddListener(ObjectsID.bulletObjectID, OnReceivePacket);
    }

    void OnDisable()
    {
        PacketManager.Instance.RemoveListener(ObjectsID.bulletObjectID);
    }

    void OnReceivePacket(uint packetId, ushort type, Stream stream)
    {
        switch (type)
        {
            case (ushort)UserPacketType.Position:
                PositionPacket positionPacket = new PositionPacket();
                positionPacket.Deserialize(stream);
                transform.position = positionPacket.payload;
                break;
        }
    }
}
