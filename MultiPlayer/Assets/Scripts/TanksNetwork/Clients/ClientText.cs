using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ClientText : NotReliableOrder<string>
{
    public Text text;
    public uint objectID;

    public void AddListener()
    {
        PacketManager.Instance.AddListener(objectID, OnReceivePacket);
    }

    public void SetText(string text)
    {
        this.text.text = text;
        MessageManager.Instance.SendString(text, objectID);//(uint)playerPointsText.GetInstanceID());
    }


    void OnReceivePacket(ushort type, Stream stream)
    {
        switch (type)
        {
            case (ushort)UserPacketType.Message:
                MessagePacket messagePacket = new MessagePacket();
                messagePacket.Deserialize(stream);
                text.text = messagePacket.payload;
                break;
        }
    }
}

