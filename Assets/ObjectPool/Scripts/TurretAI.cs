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

    void Start()
    {
        InvokeRepeating("CheckForTarget", 0, 0.5f);

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

    private void FollowTarget()
    {
        Vector3 targetDirrection = currentTarget.transform.position - turretHead.position;
        targetDirrection.y = 0;
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
        Shoot(currentTarget);
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
            if (turretType == TurretType.Dual) shootLeft = !shootLeft;
        }

        if (shootedBullet != null)
        {
            Projectile projectile = shootedBullet.GetComponent<Projectile>();

            if (projectile != null && currentTarget != null) projectile.target = currentTarget.transform;
        }

        GameObject fireEffect = Instantiate(muzzleEffect, muzzleMain.transform.position, muzzleMain.rotation);
        Destroy(fireEffect, 1f);
    }
}