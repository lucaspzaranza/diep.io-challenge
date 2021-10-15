using Mirror;
using Netcode.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    #region Props

    [SerializeField] LayerMask layerMask;

    [SerializeField] float speed = 3f;
    public float Speed => speed;

    [SerializeField] float shotDuration = 10f; 
    public float ShotDuration => shotDuration;

    [SerializeField] int damage = 1;
    public int Damage => damage;

    #endregion

    #region Fields

    [SerializeField] SpriteRenderer bulletSR;

    #endregion

    #region Local

    void Start()
    {
        Invoke(nameof(CmdDestroySelf), ShotDuration);

        if(hasAuthority)
        {
            PlayerTank localPlayer = FindObjectsOfType<PlayerTank>().FirstOrDefault(player => player.isLocalPlayer);
            UpdateBulletColor(localPlayer.GetPlayerColor());
        }
    }

    void Update()
    {
        transform.Translate(Vector3.right * Speed * Time.deltaTime);
    }

    void LogPlayerAndBulletConnectionIDs(NetworkIdentity playerNetWorkID)
    {
        print("Player ID: " + playerNetWorkID.connectionToClient.connectionId + " Bullet Connection: " + connectionToClient.connectionId);
    }

    void UpdateBulletColor(Color newColor)
    {
        bulletSR.color = new Color(
            newColor.r,
            newColor.g,
            newColor.b);
    }

    #endregion

    #region Client

    [ClientRpc]
    void RpcDestroySelf() => NetworkServer.Destroy(gameObject);

    #endregion

    #region Server

    [Command(requiresAuthority = false)]
    void CmdDestroySelf() => NetworkServer.Destroy(gameObject);

    //[ServerCallback]
    //void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.tag == "Player")
    //    {
    //        NetworkIdentity playerNetWorkID = collision.GetComponent<NetworkIdentity>();
    //        if(playerNetWorkID.connectionToClient.connectionId != connectionToClient.connectionId)
    //        {
    //            PlayerTank playerTank = collision.gameObject.GetComponent<PlayerTank>();
    //            playerTank.CmdTakeDamage(Damage);
    //            CmdDestroySelf();
    //        }
    //    }
    //}

    [ServerCallback]
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            NetworkIdentity playerNetWorkID = collision.gameObject.GetComponent<NetworkIdentity>();
            if (playerNetWorkID.connectionToClient.connectionId != connectionToClient.connectionId)
            {
                PlayerTank playerTank = collision.gameObject.GetComponent<PlayerTank>();
                playerTank.CmdTakeDamage(Damage);
                CmdDestroySelf();
            }
        }
    }

    #endregion   
}
