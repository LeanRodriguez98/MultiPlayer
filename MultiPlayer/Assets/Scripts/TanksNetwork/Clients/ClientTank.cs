using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class ClientTank : ReliableOrder<float[]>// tanto el que envia como el que recive heredan de lo mismo para usar la misma implementacion de "Deserialize" y "OnFinishDeserializing"
{
    private GameObject torretPivot;
    private bool interpolation = false;
    private Vector2 interpolationDestinationPosition = Vector2.zero;
    void OnEnable()
    {
        PacketManager.Instance.AddListener(ObjectsID.tankObjectID, OnReceivePacket);
    }

    void OnDisable()
    {
        PacketManager.Instance.RemoveListener(ObjectsID.tankObjectID);
    }

    void OnReceivePacket(ushort type, Stream stream)
    {
        switch (type)
        {
            case (ushort)UserPacketType.Player:
                TankPacket tankPacket = new TankPacket();
                idReceived = tankPacket.Deserialize(stream);// cuando llega el paquete, utiliza las implementaciones de la clase base
                OnFinishDeserializing(SetTankTransformations, tankPacket.payload);
                break;
                //case (ushort)UserPacketType.Position:
                //    PositionPacket positionPacket = new PositionPacket();
                //    positionPacket.Deserialize(stream);
                //    transform.position = positionPacket.payload;
                //    break;
                //case (ushort)UserPacketType.Rotation:
                //    if (torretPivot == null)
                //        torretPivot = transform.Find("TorretPivot").gameObject;
                //    RotationPacket rotationPacket = new RotationPacket();
                //    rotationPacket.Deserialize(stream);
                //    torretPivot.transform.rotation = rotationPacket.payload;
                //    break;
        }
    }
    private void FixedUpdate()
    {
        if (interpolation)
        {
            transform.position = Vector3.Lerp(transform.position, interpolationDestinationPosition, Time.fixedDeltaTime);
            if (new Vector2(transform.position.x, transform.position.y) == interpolationDestinationPosition)
                interpolation = false;
        }
    }

    bool a = true;
    public void SetTankTransformations(float[] transforms)
    {
        transform.position = new Vector2(transforms[0], transforms[1]);
        torretPivot = transform.Find("TorretPivot").gameObject;
        torretPivot.transform.rotation = new Quaternion(transforms[2], transforms[3], transforms[4], transforms[5]);
        if (a)
        {
            Debug.Log("Tank " + transforms[0] + "   -   " + transforms[1]);
            a = false;
        }
        if (TanksManagers.Instance.interpolateTanks)
        {
            float delayTime = Mathf.Abs(TanksManagers.Instance.GetClientTime() - transforms[6]);
            interpolationDestinationPosition = transform.position;
            interpolationDestinationPosition += new Vector2(transforms[0], transforms[1]) * delayTime;
            interpolation = true;
        }
    }
}
