using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static TurretAI;

public class Projectile : MonoBehaviour
{

    public TurretAI.TurretType type = TurretAI.TurretType.Single;
    public Transform target;
    public bool lockOn;
    public float speed = 1;
    public float turnSpeed = 1;
    public bool catapult;

    public float knockBack = 0.1f;
    public float boomTimer = 1;

    public ParticleSystem explosion;
    private GameObject pooledExplosionEffect;
    private void Start()
    {
        if (catapult)
        {
            lockOn = true;
        }

        if (type == TurretAI.TurretType.Single)
        {
            Vector3 direction = target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Update()
    {
        boomTimer -= Time.deltaTime;

        if (transform.position.y < -0.2F || boomTimer < 0 || target == null)
        {
            Explosion();
            return;
        }
        ProjectileBehaviour();
    }

    private void ProjectileBehaviour()
    {
        switch (type)
        {
            case TurretType.Single:
                float singleSpeed = speed * Time.deltaTime;
                transform.Translate(transform.forward * singleSpeed * 2, Space.World);
                break;

            case TurretType.Dual:
                if (target != null)
                {
                    Vector3 direction = target.position - transform.position;
                    Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, Time.deltaTime * turnSpeed, 0.0f);
                    Debug.DrawRay(transform.position, newDirection, Color.red);
                    transform.Translate(Vector3.forward * Time.deltaTime * speed);
                    transform.rotation = Quaternion.LookRotation(newDirection);
                }
                break;

            case TurretType.Catapult:
                if (lockOn)
                {
                    Vector3 Vo = CalculateCatapult(target.transform.position, transform.position, 1);
                    transform.GetComponent<Rigidbody>().velocity = Vo;
                    lockOn = false;
                }
                break;
        }
    }

    Vector3 CalculateCatapult(Vector3 target, Vector3 origen, float time)
    {
        Vector3 distance = target - origen;
        Vector3 distanceXZ = distance;
        distanceXZ.y = 0;

        float Vxz = distanceXZ.magnitude / time;
        float Vy = distance.y / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

        Vector3 result = distanceXZ.normalized;
        result *= Vxz;
        result.y = Vy;

        return result;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 direction = other.transform.position - transform.position;
            Vector3 knockBackPos = other.transform.position + (direction.normalized * knockBack);
            knockBackPos.y = 1;
            other.transform.position = knockBackPos;
            Explosion();
        }
    }

    public void Explosion()
    {
        GameObject pooledExplosionEffect = BulletPool.Instance.SpawnFromPool("Explosion", transform.position, transform.rotation);
        gameObject.SetActive(false);
        if (pooledExplosionEffect != null)
        {
            StartCoroutine(DisableAfterDelay(pooledExplosionEffect, 1f));
        }
    }

    private IEnumerator DisableAfterDelay(GameObject explosionEffect, float delay)
    {
        yield return new WaitForSeconds(delay);
        explosionEffect.SetActive(false);
    }
}