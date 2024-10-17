using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurretAI : MonoBehaviour
{

    public enum TurretType
    {
        Single = 1,
        Dual = 2,
        Catapult = 3,
    }

    public GameObject currentTarget;
    public Transform turretHead;

    public float attackDist = 10.0f;
    public float attackDamage;
    public float shootCoolDown;
    private float timer;
    public float lookSpeed;

    //public Quaternion randomRot;
    public Vector3 randomRot;
    public Animator animator;

    [Header("[Turret Type]")]
    public TurretType turretType = TurretType.Single;

    public Transform muzzleMain;
    public Transform muzzleSub;
    public GameObject muzzleEffect;
    public GameObject bullet;
    private bool shootLeft = true;

    private Transform lockOnPos;

    //public TurretShoot_Base shotScript;

    void Start()
    {
        InvokeRepeating("CheckForTarget", 0, 0.5f);
        //shotScript = GetComponent<TurretShoot_Base>();

        if (transform.GetChild(0).GetComponent<Animator>())
        {
            animator = transform.GetChild(0).GetComponent<Animator>();
        }

        randomRot = new Vector3(0, Random.Range(0, 359), 0);
    }

    void Update()
    {
        if (currentTarget != null)
        {
            FollowTarget();

            float currentTargetDistance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (currentTargetDistance > attackDist)
            {
                currentTarget = null;
            }
        }
        else
        {
            IdleRotate();
        }

        timer += Time.deltaTime;
        if (timer >= shootCoolDown)
        {
            if (currentTarget != null)
            {
                timer = 0;

                if (animator != null)
                {
                    animator.SetTrigger("Fire");
                    ShootTrigger();
                }
                else
                {
                    ShootTrigger();
                }
            }
        }
    }

    private void CheckForTarget()
    {
        Collider[] collision = Physics.OverlapSphere(transform.position, attackDist);
        float distAway = Mathf.Infinity;

        for (int i = 0; i < collision.Length; i++)
        {
            if (collision[i].tag == "Player")
            {
                float dist = Vector3.Distance(transform.position, collision[i].transform.position);
                if (dist < distAway)
                {
                    currentTarget = collision[i].gameObject;
                    distAway = dist;
                }
            }
        }
    }

    private void FollowTarget() //todo : smooth rotate
    {
        Vector3 targetDirrection = currentTarget.transform.position - turretHead.position;
        targetDirrection.y = 0;
        //turreyHead.forward = targetDir;
        if (turretType == TurretType.Single)
        {
            turretHead.forward = targetDirrection;
        }
        else
        {
            turretHead.transform.rotation = Quaternion.RotateTowards(turretHead.rotation, Quaternion.LookRotation(targetDirrection), lookSpeed * Time.deltaTime);
        }
    }

    private void ShootTrigger()
    {
        //shotScript.Shoot(currentTarget);
        Shoot(currentTarget);
        //Debug.Log("We shoot some stuff!");
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 origen, float time)
    {
        Vector3 distance = target - origen;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float Sy = distance.y;
        float Sxz = distanceXZ.magnitude;

        float Vxz = Sxz / time;
        float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDist);
    }

    public void IdleRotate()
    {
        turretHead.rotation = Quaternion.RotateTowards(turretHead.rotation, Quaternion.Euler(randomRot), lookSpeed * Time.deltaTime * 0.2f);

        if (Quaternion.Angle(turretHead.rotation, Quaternion.Euler(randomRot)) < 0.1f)
        {
            randomRot = new Vector3(0, Random.Range(0, 359), 0);
        }
    }

    public void Shoot(GameObject target)
    {
        GameObject shootedBullet;

        if (turretType == TurretType.Dual && shootLeft)
        {
            shootedBullet = BulletPool.Instance.SpawnFromPool(turretType.ToString(), muzzleSub.transform.position, muzzleSub.rotation);
        }
        else
        {
            shootedBullet = BulletPool.Instance.SpawnFromPool(turretType.ToString(), muzzleMain.transform.position, muzzleMain.rotation);
        }

        if (turretType == TurretType.Dual)
        {
            shootLeft = !shootLeft;
        }

        if (shootedBullet != null)
        {
            Projectile projectile = shootedBullet.GetComponent<Projectile>();

            if (projectile != null && currentTarget != null)
            {
                projectile.target = currentTarget.transform;
            }
        }

        GameObject fireEffect = Instantiate(muzzleEffect, muzzleMain.transform.position, muzzleMain.rotation);
        Destroy(fireEffect, 1f);
    }
}