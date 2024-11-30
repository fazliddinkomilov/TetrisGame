using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
    [Header("Platform Boundaries")]
    public PlatformBoundaries platformBoundaries;

    [Header("Movement Settings")]
    [SerializeField] private float timestep = 0.2F;
    [SerializeField] private float moveDelay = 0.1f;

    public GameObject actualGroup;
    private float time;
    private bool isMoving = false;
    private CubeArray cubeArray;
    private BalanceSystem balanceSystem;
    public BalanceSystem.Platform currentPlatform = BalanceSystem.Platform.Left;

    [System.Serializable]
    public class PlatformBoundaries
    {
        public float leftPlatformStart = 0f;
        public float leftPlatformEnd = 6f;
        public float rightPlatformStart = 16f;
        public float rightPlatformEnd = 25f;
    }

    void Start()
    {
        cubeArray = Camera.main.GetComponent<CubeArray>();
        balanceSystem = FindObjectOfType<BalanceSystem>();
    }

    public void startGame()
    {
        actualGroup = GetComponent<GroupSpawner>().spawnNext();
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time > timestep)
        {
            time = 0;
            if (actualGroup != null)
            {
                move(Vector3.down);
            }
        }

        if (!isMoving)
        {
            checkForInput();
        }
    }

    void checkForInput()
    {
        if (actualGroup == null) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (balanceSystem != null)
            {
                balanceSystem.SwitchPlatform();
            }
        }

        // Rotation controls
        if (Input.GetKeyDown(KeyCode.R))
        {
            actualGroup.GetComponent<Rotation>().rotateRight();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            actualGroup.GetComponent<Rotation>().rotateLeft();
        }

        // Horizontal movement
        if (Input.GetKey(KeyCode.A))
        {
            if (CanMove(Vector3.left))
            {
                StartCoroutine(MoveWithDelay(Vector3.left));
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (CanMove(Vector3.right))
            {
                StartCoroutine(MoveWithDelay(Vector3.right));
            }
        }

        // Speed control
        if (Input.GetKey(KeyCode.S))
        {
            timestep = 0.05F;
        }
        else
        {
            setNewSpeed();
        }
    }

    private bool CanMove(Vector3 direction)
    {
        if (actualGroup == null) return false;

        foreach (Transform child in actualGroup.transform)
        {
            Vector3 newPos = child.position + direction;
            float minBound = GetCurrentPlatformMinBound();
            float maxBound = GetCurrentPlatformMaxBound();

            if (newPos.x < minBound || newPos.x > maxBound)
            {
                return false;
            }

            if (IsPositionOccupied(newPos))
            {
                return false;
            }
        }
        return true;
    }

    private float GetCurrentPlatformMinBound()
    {
        return currentPlatform == BalanceSystem.Platform.Left ?
            platformBoundaries.leftPlatformStart :
            platformBoundaries.rightPlatformStart;
    }

    private float GetCurrentPlatformMaxBound()
    {
        return currentPlatform == BalanceSystem.Platform.Left ?
            platformBoundaries.leftPlatformEnd :
            platformBoundaries.rightPlatformEnd;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            if (cube.transform.parent != actualGroup.transform &&
                Vector3.Distance(cube.transform.position, position) < 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator MoveWithDelay(Vector3 direction)
    {
        isMoving = true;
        move(direction);
        yield return new WaitForSeconds(moveDelay);
        isMoving = false;
    }

    void move(Vector3 pos)
    {
        if (actualGroup == null) return;

        Vector3 oldPosition = actualGroup.transform.position;
        actualGroup.transform.position += pos;

        if (!cubeArray.getCubePositionFromScene())
        {
            actualGroup.transform.position = oldPosition;

            if (pos == Vector3.down)
            {
                spawnNew();
            }
            else
            {
                var cantMoveAudio = GameObject.Find("CantMove")?.GetComponent<AudioSource>();
                if (cantMoveAudio != null)
                {
                    cantMoveAudio.Play();
                }
            }
        }
    }

    public void setNewSpeed()
    {
        timestep = ((10 - GetComponent<Highscore>().level) * 0.05F);
    }

    private void spawnNew()
    {
        if (actualGroup != null)
        {
            actualGroup.GetComponent<Rotation>().isActive = false;
        }

        actualGroup = GetComponent<GroupSpawner>().spawnNext();

        if (actualGroup != null)
        {
            actualGroup.GetComponent<Rotation>().isActive = true;

            if (!cubeArray.getCubePositionFromScene())
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                cubeArray.checkForFullLine();
            }
        }
    }
}