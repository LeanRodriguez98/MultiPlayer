using UnityEngine;
public class Bullet : MonoBehaviour
{
    public void Update()
    {
        MessageManager.Instance.SendPosition(transform.position, ObjectsID.bulletObjectID);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("WorldLimits"))
        {
            GameManager.Instance.OnEndTurn();
            gameObject.SetActive(false);
        }

    }
}
