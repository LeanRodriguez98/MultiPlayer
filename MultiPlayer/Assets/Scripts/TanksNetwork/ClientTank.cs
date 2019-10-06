using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class ClientTank : MonoBehaviour {
    private GameObject torretPivot;
    void OnEnable()
    {
        PacketManager.Instance.Awake();
        PacketManager.Instance.AddListener(ObjectsID.tankObjectID, OnReceivePacket);
    }

    void OnDisable()
    {
        PacketManager.Instance.RemoveListener(ObjectsID.tankObjectID);
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
            case (ushort)UserPacketType.Rotation:
                if (torretPivot == null)
                    torretPivot = transform.Find("TorretPivot").gameObject;
                RotationPacket rotationPacket = new RotationPacket();
                rotationPacket.Deserialize(stream);
                torretPivot.transform.rotation = rotationPacket.payload;
                break;
        }
    }
}
