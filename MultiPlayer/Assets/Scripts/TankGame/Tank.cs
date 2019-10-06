using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Tank : MonoBehaviour {

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
    [HideInInspector] public bool isYourTurn = false;
    private bool usedShoot = false;
    private Rigidbody2D rb;

	void Start ()
    {
        rb = GetComponent<Rigidbody2D>();
        torretPivot.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
    }


    public void SetData(Tank t, bool isClient)
    {
        controls.addAngleKey = t.controls.addAngleKey;
        controls.removeAngleKey = t.controls.removeAngleKey;
        controls.addBulletVelocity = t.controls.addBulletVelocity;
        controls.removeBulletVelocity = t.controls.removeBulletVelocity;
        controls.rightMovement = t.controls.rightMovement;
        controls.leftMovement = t.controls.leftMovement;
        controls.shoot = t.controls.shoot;
        
        movementSpeed = t.movementSpeed;
        rotationSpeed = t.rotationSpeed;
        maxBulletVelocity = t.maxBulletVelocity;
        bulletVelocityMultiplyer = t.bulletVelocityMultiplyer;
        lifes = t.lifes;
        torretPivot = transform.Find("TorretPivot").gameObject;
        torretPivot.gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
        rb = GetComponent<Rigidbody2D>();
        bulletToShoot.bulletRb = bulletToShoot.bullet.GetComponent<Rigidbody2D>();
        bulletToShoot.spriteRenderer = bulletToShoot.bullet.GetComponent<SpriteRenderer>();
        bulletToShoot.bulletColor = GetComponent<SpriteRenderer>().color;
        isYourTurn = isClient;
    }
    void Update ()
    {
        if (true /*isYourTurn*/)
        {
            if (!usedShoot)
            {
                if (!GameManager.Instance.gameOver)
                {
                    Movement();
                    RotateTorret();
                    ModifyBulletVelocity();
                    Shoot();
                    
                }
            }
        }

        MessageManager.Instance.SendPosition(transform.position, ObjectsID.tankObjectID);
        MessageManager.Instance.SendRotation(torretPivot.transform.rotation, ObjectsID.tankObjectID);
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
        if (Input.GetKey(controls.addAngleKey) && AuxAngle < 90)
        {
            torretPivot.transform.eulerAngles += new Vector3(0, 0, rotationSpeed) * Time.deltaTime;
            AuxAngle += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(controls.removeAngleKey) && AuxAngle > -90)
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
        if (Input.GetKeyDown(controls.shoot))
        {
            bulletToShoot.bullet.SetActive(true);
            bulletToShoot.bulletRb.velocity = Vector2.zero;
            bulletToShoot.bulletRb.angularVelocity = 0.0f;
            bulletToShoot.bullet.transform.position = transform.position;
            bulletToShoot.spriteRenderer.color = bulletToShoot.bulletColor;
            bulletToShoot.bulletRb.AddForce(torretPivot.transform.up * bulletVelocity * 100);
            usedShoot = true;
        }
    }

    public void SetDamage(int damage)
    {
        lifes -= damage;
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
