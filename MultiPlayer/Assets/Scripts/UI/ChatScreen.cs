using UnityEngine.UI;
using System.IO;
using UnityEngine;

/* Esta clase envía los packets recibidos a las clases que requieran esa informacion */
public class ChatScreen : MBSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;

    protected override void Initialize()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
       PacketManager.Instance.AddListener(0, OnReceivePacket);
    }

    void OnDisable()
    {
        PacketManager.Instance.RemoveListener(0);
    }

    void OnReceivePacket(ushort type, Stream stream)
    {
        switch (type)
        {
            case (ushort)UserPacketType.Message:
                MessagePacket messagePacket = new MessagePacket();
                messagePacket.Deserialize(stream);

                if (ConnectionManager.Instance.isServer)
                    MessageManager.Instance.SendString(messagePacket.payload, 0);

                messages.text += messagePacket.payload + System.Environment.NewLine;
                break;

            case (ushort)UserPacketType.Position:
                PositionPacket positionPacket = new PositionPacket();
                positionPacket.Deserialize(stream);

                if (ConnectionManager.Instance.isServer)
                    MessageManager.Instance.SendPosition(positionPacket.payload, 0);

                // Aca enviaria el payload a donde sea necesario
            break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (inputMessage && inputMessage.text != "")
            {
                if (NetworkManager.IsAvailable() && ConnectionManager.Instance.isServer)
                    messages.text += inputMessage.text + System.Environment.NewLine;

                MessageManager.Instance.SendString(inputMessage.text, 0);

                inputMessage.ActivateInputField();
                inputMessage.Select();
                inputMessage.text = "";
            }
        }
    }
}
