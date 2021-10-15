using Mirror;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawner : NetworkBehaviour
{
    public static BoxSpawner instance;

    [SerializeField] int maxBoxLimit;
    [SerializeField] List<GameObject> boxList;
    [SerializeField] float nextBoxTimeInterval;
    [SerializeField] float overlapRadius;
    [SerializeField] LayerMask destructibleLayer;

    [SyncVar] float timeCounter;
    [SyncVar] int boxCounter;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);
    }

    [ServerCallback]
    void Update()
    {
        timeCounter += Time.deltaTime;

        if(timeCounter >= nextBoxTimeInterval)
        {
            timeCounter = 0;
            print(boxList.Count);
            if (boxCounter < maxBoxLimit)
                CmdInstantiateBox(GetRandomBoxIndex());
        }
    }

    int GetRandomBoxIndex() => Random.Range(0, boxList.Count);

    Vector2 GetRandomPosition()
    {
        int x = Random.Range(0, Screen.width);
        int y = Random.Range(0, Screen.height);

        Vector2 screenPoint = new Vector2(x, y);

        return Camera.main.ScreenToWorldPoint(screenPoint);
    }

    bool CollideWithSomePlayer(Vector3 pos)
    {
        return Physics2D.OverlapCircle(pos, overlapRadius, destructibleLayer);
    }

    #region Server

    [Command(requiresAuthority = false)]
    void CmdInstantiateBox(int boxIndex)
    {
        Vector2 randomPos = GetRandomPosition();

        while(CollideWithSomePlayer(randomPos))
        {
            randomPos = GetRandomPosition();
        }

        GameObject newBox = Instantiate(boxList[boxIndex], randomPos, Quaternion.identity);
        NetworkServer.Spawn(newBox);
        boxCounter++;
    }

    [Command(requiresAuthority = false)]
    public void CmdDecrementBoxAmount()
    {
        boxCounter--;
    }

    #endregion
}
