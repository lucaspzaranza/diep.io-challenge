using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotation : NetworkBehaviour
{
    [SerializeField] Transform transformToLock;
    [SyncVar] Quaternion initRotation;

    public override void OnStartServer()
    {
        base.OnStartServer();
        initRotation = transform.rotation;
    }

    void Update()
    {
        transformToLock.rotation = initRotation;
    }
}
