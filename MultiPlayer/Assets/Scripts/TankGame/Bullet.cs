using UnityEngine;
public class Bullet : MonoBehaviour
{
    private Tank enemyTank;

    public void Update()
    {
        MessageManager.Instance.SendPosition(transform.position, ObjectsID.bulletObjectID);
    }
    public void OnEnable()
    {
        if (GameManager.instance != null)
        {
            enemyTank = GameManager.instance.GetEnemyTank();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("WorldLimits"))
        {
            GameManager.instance.OnEndTurn();
            gameObject.SetActive(false);
        }

        /*if (col.gameObject == enemyTank.gameObject)
        {
            enemyTank.SetDamage(1);
            GameManager.instance.OnEndTurn();
            gameObject.SetActive(false);
        }*/
    }


}
