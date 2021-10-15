using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace Netcode.Player
{
    public class PlayerTank : NetworkBehaviour
    {
        const int maxLevel = 3;

        #region Props

        [SerializeField]
        [Range(0, 10)]
        [SyncVar(hook = nameof(UpdateLife))]
        int life;
        public int Life
        {
            get => life;
            private set
            {
                life = value;
                if (life <= 0)
                    NetworkServer.Destroy(gameObject);
            }
        }

        [SerializeField] float speed;
        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        List<PlayerCannon> playerCannons;
        public List<PlayerCannon> PlayerCannons
        {
            get
            {
                if (playerCannons == null || playerCannons.Count == 0)
                {
                    playerCannons = transform.GetComponentsInChildren<PlayerCannon>()
                        .Where(cannon => cannon.gameObject.activeInHierarchy).ToList();
                }

                return playerCannons;
            }
            private set => playerCannons = value;
        }

        [SyncVar]
        [SerializeField] int score;
        public int Score
        {
            get => score;
            set
            {
                score = value;
                UpdatePlayerScore(score);
                // Level up a cada 30 pontos
                if (score % 30 == 0 && Level < maxLevel)
                    CmdLevelUp(Level + 1);
            }
        }

        [SerializeField] int ammo;
        public int Ammo
        {
            get => ammo;
            set
            {
                ammo = value;
                if (ammo == 0 && hasAuthority)
                    CmdLevelUp(1);
            }
        }

        [SyncVar] [SerializeField] int level;
        public int Level => level;

        #endregion

        #region Fields

        [SerializeField] [SyncVar(hook = nameof(UpdatePlayerName))] string playerName;
        [SerializeField] GameObject cannon;
        [SerializeField] GameObject cannonLvl2;
        [SerializeField] GameObject cannonLvl3;
        [SerializeField] GameObject outline;
        [SerializeField] Image lifebar;
        [SerializeField] Text playerNameTxtComp;
        [SerializeField] Color playerColor;
        [SerializeField] Color playerOutlineColor;

        Rigidbody2D rigidBody2D;

        public Text playerScore;
        int partialScore;
        #endregion

        #region Local

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            rigidBody2D = GetComponent<Rigidbody2D>();

            GetInputFieldName();
            UpdatePlayerColor();
        }

        private void Start()
        {
            // Not working properly
            playerScore = GameObject.FindGameObjectWithTag("ScoreHUD").GetComponent<Text>();
        }

        private void Update()
        {
            if(isLocalPlayer)
            {
                Move();
                Aim();

                if (Input.GetMouseButtonDown(0))
                    CmdSpawnShot();
            }
        }

        void Move()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            rigidBody2D.velocity = new Vector2(horizontal * Speed, vertical * Speed);
        }

        void Aim()
        {
            Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public string GetPlayerName() => playerName;

        void GetInputFieldName()
        {
            InputField inputField = FindObjectOfType<InputField>();
            CmdUpdatePlayerName(inputField.text);
            inputField.gameObject.SetActive(false);
        }

        void UpdatePlayerColor()
        {
            SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
            SpriteRenderer outlineSR = outline.GetComponent<SpriteRenderer>();

            playerSR.color = new Color(
                playerColor.r, 
                playerColor.g, 
                playerColor.b);

            outlineSR.color = new Color(
                playerOutlineColor.r,
                playerOutlineColor.g,
                playerOutlineColor.b);
        }

        public Color GetPlayerColor() => playerColor;

        [Client]
        void UpdatePlayerScore(int newScore)
        {
            playerScore.text = $"Pontos: {newScore}";
        }

        void ResetPlayerLevel()
        {
            level = 1;
            cannonLvl2.SetActive(false);
            cannonLvl3.SetActive(false);
            cannon.SetActive(true);
        }

        #endregion

        #region Client
                
        [ClientRpc]
        void RpcActivateLevelCannon(int newLevel)
        {
            switch (newLevel)
            {
                case 1:
                    ResetPlayerLevel();
                    break;

                case 2:
                    level = 2;
                    cannon.SetActive(false);
                    cannonLvl2.SetActive(true);
                    Ammo = 80;
                    break;

                case 3:
                    level = 3;
                    cannonLvl2.SetActive(false);
                    cannonLvl3.SetActive(true);
                    Ammo = 20;
                    break;
                default:
                    ResetPlayerLevel();
                    break;
            }

            PlayerCannons = null;
        }

        [ClientRpc]
        void UpdateLifebarImg(float value)
        {
            lifebar.fillAmount = value / 10;
        }

        [Client]
        void UpdatePlayerName(string oldName, string newName)
        {
            playerNameTxtComp.text = newName;
        }

        [TargetRpc]
        void RpcUpdatePlayerScore(NetworkConnection connection, int newScore)
        {
            //Incomplete...
        }

        #endregion

        #region Server

        [Command(requiresAuthority = false)]
        void CmdLevelUp(int newLevel)
        {
            RpcActivateLevelCannon(newLevel);
        }

        [ServerCallback]
        void UpdateLife(int oldLife, int newLife)
        {
            UpdateLifebarImg(newLife);
        }

        [Command(requiresAuthority = false)]
        public void CmdUpdatePlayerName(string newName) => playerName = newName;

        public void CmdTakeDamage(int damage) => Life -= damage;

        [Command]
        void CmdSpawnShot()
        {
            if (Level > 1 && Ammo == 0) return; // No ammo to lvl 2 or 3;

            foreach (PlayerCannon cannon in PlayerCannons)
            {
                GameObject bullet = cannon.GetBullet();
                NetworkServer.Spawn(bullet, connectionToClient);
                if(level > 1)
                    Ammo--;
            }
        }

        [Command]
        void CmdUpdatePlayerScore(int newScore)
        {
            RpcUpdatePlayerScore(connectionToClient, newScore);
        }
        #endregion
    }
}