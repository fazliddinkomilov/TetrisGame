using UnityEngine;
using UnityEngine.UI;

public class BalanceSystem : MonoBehaviour
{
    public Transform leftPlatform;
    public Transform rightPlatform;
    public Slider balanceSlider;
    private float maxPlatformDepth = 5f;
    private float weightMultiplier = 0.2f;
    public Platform currentPlatform = Platform.Left;
    private Movement movementComponent;
    private Highscore highscoreComponent;
    [SerializeField] private float platformOffset = 16f; // Distance between platforms
    [SerializeField] private float balanceThreshold = 0.05f; // Threshold for balance

    private float balanceTimer = 0f; // Timer to track balanced time

    public enum Platform
    {
        Left,
        Right
    }

    void Start()
    {
        balanceSlider.minValue = -1f;
        balanceSlider.maxValue = 1f;
        movementComponent = FindObjectOfType<Movement>();
        highscoreComponent = FindObjectOfType<Highscore>();
    }

    void Update()
    {
        UpdatePlatformWeights();
    }

    private void UpdatePlatformWeights()
    {
        int leftCount = CountBlocksOnPlatform(leftPlatform);
        int rightCount = CountBlocksOnPlatform(rightPlatform);

        float balanceRatio = (leftCount - rightCount) /
            (float)Mathf.Max(leftCount + rightCount, 1);

        balanceSlider.value = balanceRatio;

        // Check if platforms are balanced
        if (Mathf.Abs(balanceRatio) <= balanceThreshold)
        {
            balanceTimer += Time.deltaTime;
            if (balanceTimer >= 1f)
            {
                highscoreComponent.addPointsForCubes();
                balanceTimer = 0f; // Reset timer after adding points
            }
        }
        else
        {
            balanceTimer = 0f; // Reset timer if unbalanced
        }

        // Update platform positions based on weight
        Vector3 leftTargetPos = new Vector3(
            leftPlatform.position.x,
            -balanceRatio * weightMultiplier * maxPlatformDepth,
            leftPlatform.position.z
        );

        Vector3 rightTargetPos = new Vector3(
            rightPlatform.position.x,
            balanceRatio * weightMultiplier * maxPlatformDepth,
            rightPlatform.position.z
        );

        leftPlatform.position = Vector3.Lerp(
            leftPlatform.position,
            leftTargetPos,
            Time.deltaTime * 5f
        );

        rightPlatform.position = Vector3.Lerp(
            rightPlatform.position,
            rightTargetPos,
            Time.deltaTime * 5f
        );
    }

    private int CountBlocksOnPlatform(Transform platform)
    {
        int count = 0;
        foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            if (IsAbovePlatform(cube.transform.position, platform.position))
            {
                count++;
            }
        }
        return count;
    }

    private bool IsAbovePlatform(Vector3 cubePos, Vector3 platformPos)
    {
        float platformBoundary = 15f;
        return platformPos.x < platformBoundary ?
            cubePos.x < platformBoundary :
            cubePos.x >= platformBoundary;
    }

    public void SwitchPlatform()
    {
        GameObject currentGroup = movementComponent.actualGroup;
        if (currentGroup == null) return;

        Vector3 oldPosition = currentGroup.transform.position;
        Platform oldPlatform = currentPlatform;
        Vector3 newPosition = currentGroup.transform.position;

        // Calculate new position
        if (currentPlatform == Platform.Left)
        {
            newPosition.x += platformOffset;
            currentPlatform = Platform.Right;
        }
        else
        {
            newPosition.x -= platformOffset;
            currentPlatform = Platform.Left;
        }

        // Try moving to new position
        currentGroup.transform.position = newPosition;

        // Check if new position is valid
        if (!Camera.main.GetComponent<CubeArray>().getCubePositionFromScene())
        {
            // Revert if invalid
            currentGroup.transform.position = oldPosition;
            currentPlatform = oldPlatform;
            var cantMoveAudio = GameObject.Find("CantMove")?.GetComponent<AudioSource>();
            if (cantMoveAudio != null)
            {
                cantMoveAudio.Play();
            }
        }
        else
        {
            // Update movement component with new platform
            movementComponent.currentPlatform = currentPlatform;
        }
    }
}