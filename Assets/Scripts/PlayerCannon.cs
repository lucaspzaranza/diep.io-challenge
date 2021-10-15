using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCannon : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;

    Vector3 GetCannonSpawnPosition() => transform.GetChild(0).position;

    public GameObject GetBullet()
    {
        Vector3 bulletPosition = GetCannonSpawnPosition();
        GameObject newBullet = Instantiate(bulletPrefab, bulletPosition, transform.rotation);
        return newBullet;
    }
}
