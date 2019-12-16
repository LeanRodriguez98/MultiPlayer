using System.IO;
using UnityEngine;
public class ClientBullet : ReliableOrder<float[]>
{
    private bool interpolation = false;
    private Vector2 interpolationDestinationPosition = Vector2.zero;
    void OnEnable()
    {
        PacketManager.Instance.AddListener(ObjectsID.bulletObjectID, OnReceivePacket);
    }

    void OnDisable()
    {
        PacketManager.Instance.RemoveListener(ObjectsID.bulletObjectID);
    }

    void OnReceivePacket(ushort type, Stream stream)
    {
        switch (type)
        {
            case (ushort)UserPacketType.Bullet:
                BulletPacket bulletPacket = new BulletPacket();
                idReceived = bulletPacket.Deserialize(stream);
                OnFinishDeserializing(SetBulletPosition, bulletPacket.payload);
                break;
        }
    }
    bool a = true;
    private void SetBulletPosition(float[] finalPosition)
    {
        transform.position = new Vector2(finalPosition[0], finalPosition[1]);
        if (a)
        {
            Debug.Log("Bullet " + finalPosition[0] + "   -   " + finalPosition[1]);
            a = false;
        }

        if (TanksManagers.Instance.interpolateBullets)
        {
            float delayTime = Mathf.Abs(TanksManagers.Instance.GetClientTime() - finalPosition[2]);
            interpolationDestinationPosition = transform.position;
            interpolationDestinationPosition += new Vector2(finalPosition[0], finalPosition[1]) * delayTime;
            interpolation = true;
        }
    }

    private void FixedUpdate()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == GameManager.Instance.player.gameObject)
        {
            GameManager.Instance.player.LoseOneLife();
        }
    }
}
