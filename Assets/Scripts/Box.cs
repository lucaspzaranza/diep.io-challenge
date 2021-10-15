using Mirror;
using Netcode.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Box : NetworkBehaviour
{
    [SerializeField] [SyncVar(hook = (nameof(UpdateLifeStatus)))] int life;
    [SerializeField] int damageToDealOnPlayer;
    [SerializeField] int points;
    [SerializeField] float timeToUpdateSpeed;
    [SerializeField] float speed;
    [SerializeField] Rigidbody2D rigidBody2D;

    NetworkConnection hitBulletConn;
    Vector2 speedDirection;
    float timeCounter = 0;

    #region Local

    void FixedUpdate()
    {
        timeCounter += Time.fixedDeltaTime;

        if(timeCounter >= timeToUpdateSpeed)
        {
            timeCounter = 0f;
            speedDirection = GetNewSpeedDirection();
        }

        rigidBody2D.velocity = Vector2.zero;
        transform.Translate(speedDirection * speed * Time.fixedDeltaTime);
    }

    Vector2 GetNewSpeedDirection()
    {
        float negativeForce = speed * -1f;
        float randomX = Random.Range(negativeForce, speed);
        float randomY = Random.Range(negativeForce, speed);
        return new Vector2(randomX, randomY);
    }

    #endregion

    #region Client

    void UpdateLifeStatus(int oldLife, int newLife)
    {
        if(newLife <= 0)
            NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Server

    [Command(requiresAuthority = false)]
    void UpdateLife(int newLife)
    {
        life = newLife;
    }
    
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerTank playerTank = other.gameObject.GetComponent<PlayerTank>();
            playerTank.CmdTakeDamage(damageToDealOnPlayer);
        }
        else if (other.gameObject.tag == "Bullet")
        {
            Bullet bulletScript = other.gameObject.GetComponent<Bullet>();
            hitBulletConn = bulletScript.connectionToClient;

            UpdateLife(life - bulletScript.Damage);
            NetworkServer.Destroy(other.gameObject);
        }
    }

    void OnDestroy()
    {
        if(hitBulletConn != null)
        {
            PlayerTank player = FindObjectsOfType<PlayerTank>()
                .FirstOrDefault(player => player.connectionToClient.connectionId == hitBulletConn.connectionId);
            if (player != null)
                player.Score += points;
        }
        BoxSpawner.instance.CmdDecrementBoxAmount();
    }

    #endregion
}
