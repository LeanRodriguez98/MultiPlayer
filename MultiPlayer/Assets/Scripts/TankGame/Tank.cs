using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Tank : ReliableOrder<float[]>
{

    [System.Serializable]
    public struct Controls
    {
        public KeyCode addAngleKey;
        public KeyCode removeAngleKey;
        public KeyCode addBulletVelocity;
        public KeyCode removeBulletVelocity;
        public KeyCode rightMovement;
        public KeyCode leftMovement;
        public KeyCode shoot;
    }

    [System.Serializable]
    public struct BulletToShoot
    {
        public GameObject bullet;
        public Color bulletColor;
        public Rigidbody2D bulletRb;
        public SpriteRenderer spriteRenderer;
    }

    public Controls controls;
    public BulletToShoot bulletToShoot;
    public GameObject torretPivot;
    public float movementSpeed;
    public float rotationSpeed;
    public float maxBulletVelocity;
    [HideInInspector] public float AuxAngle = 0.0f;
    public float bulletVelocity = 0.0f;
    public float bulletVelocityMultiplyer;
    public int lifes;
    [HideInInspector] public bool isYourTurn = true;
    private bool usedShoot = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        torretPivot.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
    }


    void FixedUpdate()
    {
        if (!GameManager.Instance.gameOver)
        {
            Movement();
            RotateTorret();
            ModifyBulletVelocity();
            Shoot();
        }

        float[] tankData = new float[7];
        tankData[0] = transform.position.x;
        tankData[1] = transform.position.y;
        tankData[2] = torretPivot.transform.rotation.x;
        tankData[3] = torretPivot.transform.rotation.y;
        tankData[4] = torretPivot.transform.rotation.z;
        tankData[5] = torretPivot.transform.rotation.w;
        tankData[6] = TanksManagers.Instance.GetClientTime();

        MessageManager.Instance.SendTankData(tankData, ObjectsID.tankObjectID, ++lastIdSent);// Se manda el paquete
    }

    public void Movement()
    {
        if (Input.GetKey(controls.rightMovement))
        {
            rb.AddForce((transform.right * movementSpeed * Time.deltaTime));
        }
        if (Input.GetKey(controls.leftMovement))
        {
            rb.AddForce((-transform.right * movementSpeed * Time.deltaTime));
        }
    }

    public void RotateTorret()
    {
        if (Input.GetKey(controls.addAngleKey) && AuxAngle < 45)
        {
            torretPivot.transform.eulerAngles += new Vector3(0, 0, rotationSpeed) * Time.deltaTime;
            AuxAngle += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(controls.removeAngleKey) && AuxAngle > -45)
        {
            torretPivot.transform.eulerAngles += new Vector3(0, 0, -rotationSpeed) * Time.deltaTime;
            AuxAngle -= rotationSpeed * Time.deltaTime;

        }
    }

    public void ModifyBulletVelocity()
    {
        if (Input.GetKey(controls.addBulletVelocity) && bulletVelocity < maxBulletVelocity)
        {
            bulletVelocity += Time.deltaTime * bulletVelocityMultiplyer;
            if (bulletVelocity > maxBulletVelocity)
            {
                bulletVelocity = maxBulletVelocity;
            }
        }

        if (Input.GetKey(controls.removeBulletVelocity) && bulletVelocity > 0)
        {
            bulletVelocity -= Time.deltaTime * bulletVelocityMultiplyer;
            if (bulletVelocity < 0)
            {
                bulletVelocity = 0;
            }
        }

    }

    public void Shoot()
    {
        if (!usedShoot)
        {
            if (Input.GetKeyDown(controls.shoot))
            {
                bulletToShoot.bullet.SetActive(true);
                bulletToShoot.bulletRb.velocity = Vector2.zero;
                bulletToShoot.bulletRb.angularVelocity = 0.0f;
                bulletToShoot.bullet.transform.position = transform.position;
                bulletToShoot.spriteRenderer.color = bulletToShoot.bulletColor;
                bulletToShoot.bulletRb.AddForce(torretPivot.transform.up * bulletVelocity * 100);
                usedShoot = true;
                Invoke("ResetShoot", 5.0f);
            }
        }
    }
    public void ResetShoot()
    {
        usedShoot = false;
    }
    public void LoseOneLife()
    {
        lifes--;
        if (lifes <= 0)
        {
            GameManager.Instance.OnGameOver();
        }
    }

    public void SetTurn()
    {
        isYourTurn = !isYourTurn;
        usedShoot = false;
    }

}
