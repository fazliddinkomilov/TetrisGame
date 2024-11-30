using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class spawn hold all existing cube groups and
 * can spawn a random one
 */
public class GroupSpawner : MonoBehaviour
{
    public GameObject[] groups;
    private Movement movement;

    void Start()
    {
        movement = GetComponent<Movement>();
    }

    public GameObject spawnNext()
    {
        int i = Random.Range(0, groups.Length);
        float spawnX = movement.currentPlatform == BalanceSystem.Platform.Left ? 2 : 20; // 17 для правой платформы
        return Instantiate(groups[i], new Vector3(spawnX, 14), Quaternion.identity);
    }
}