public class Bullet : ReliableOrder<float[]>
{
    public void FixedUpdate()
    {
        float[] position = new float[3];
        position[0] = transform.position.x;
        position[1] = transform.position.y;
        position[2] = TanksManagers.Instance.GetClientTime();
        MessageManager.Instance.SendBallPosition(position, ObjectsID.bulletObjectID, ++lastIdSent);
    }
}
