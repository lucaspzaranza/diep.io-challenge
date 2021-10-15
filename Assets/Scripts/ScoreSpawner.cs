using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Incomplete
public class ScoreSpawner : NetworkBehaviour
{
    [SerializeField] GameObject scorePrefab;
    [SerializeField] Transform canvasTransform;

    public override void OnStartClient()
    {
        base.OnStartClient();

        CmdInstantiateScore();
    }

    [Command(requiresAuthority = false)]
    void CmdInstantiateScore()
    {
        if(netIdentity.isLocalPlayer)
            RpcInstantiateScore();
    }

    [ClientRpc]
    void RpcInstantiateScore()
    {
        GameObject score = Instantiate(scorePrefab);
        score.transform.SetParent(canvasTransform, false);
        NetworkServer.Spawn(score, connectionToClient);
    }
}
