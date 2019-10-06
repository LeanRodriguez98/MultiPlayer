using System.IO;
using UnityEngine;

public class ClientPlayerUIData : MonoBehaviour
{

    public UI_Canvas.PlayerUIData uiToUpdate;

    
    public void Init()
    {
        PacketManager.Instance.Awake();
        PacketManager.Instance.AddListener(ObjectsID.playerUIObjectID, OnReceivePacket);
    }

    void OnDisable()
    {
        PacketManager.Instance.RemoveListener(ObjectsID.playerUIObjectID);
    }
    void OnReceivePacket(uint packetId, ushort type, Stream stream)
    {
        switch (type)
        {
            case (ushort)UserPacketType.PlayerUI:
                PlayerUIPacket playerUIPacket = new PlayerUIPacket();
                playerUIPacket.Deserialize(stream);

                string[] texts = playerUIPacket.payload.Split(new char[] { '?' });

                uiToUpdate.playerHealt.text = texts[0];// playerUIPacket.payload.playerHealt.text;
                uiToUpdate.bulletSpeed.text = texts[1];// playerUIPacket.payload.bulletSpeed.text;
                uiToUpdate.cannonAngle.text = texts[2];// playerUIPacket.payload.cannonAngle.text;
                break;
        }
    }
}
