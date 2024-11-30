using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CubeArray : MonoBehaviour
{
    [System.Serializable]
    public class PlatformBoundaries
    {
        public int leftPlatformStart = 0;
        public int leftPlatformEnd = 6;
        public int rightPlatformStart = 16;
        public int rightPlatformEnd = 25;

        public int maxHeight = 17;

        public int GetTotalWidth()
        {
            return rightPlatformEnd + 1; // +1 because array indices are 0-based
        }
    }

    [SerializeField] private PlatformBoundaries boundaries;
    private bool[,] isCube;

    private void Start()
    {
        if (boundaries == null)
        {
            boundaries = new PlatformBoundaries();
        }
    }

    public bool getCubePositionFromScene()
    {
        // Initialize array with size to accommodate both platforms
        isCube = new bool[boundaries.GetTotalWidth(), boundaries.maxHeight];

        foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
        {
            int x = Mathf.RoundToInt(cube.transform.position.x);
            int y = Mathf.RoundToInt(cube.transform.position.y);

            // Check if position is within valid platform bounds
            bool isValidPosition = IsValidPosition(x, y);

            if (!isValidPosition)
            {
                return false;
            }

            // Check if position is already occupied
            if (isCube[x, y])
            {
                return false;
            }

            isCube[x, y] = true;
        }
        return true;
    }

    private bool IsValidPosition(int x, int y)
    {
        // Check height bounds
        if (y < 0 || y >= boundaries.maxHeight)
        {
            return false;
        }

        // Check if position is within either platform's bounds
        bool isInLeftPlatform = x >= boundaries.leftPlatformStart && x <= boundaries.leftPlatformEnd;
        bool isInRightPlatform = x >= boundaries.rightPlatformStart && x <= boundaries.rightPlatformEnd;

        return isInLeftPlatform || isInRightPlatform;
    }

    public void checkForFullLine()
    {
        // Check left platform
        CheckPlatformLines(boundaries.leftPlatformStart, boundaries.leftPlatformEnd);
        // Check right platform
        CheckPlatformLines(boundaries.rightPlatformStart, boundaries.rightPlatformEnd);
    }

    private void CheckPlatformLines(int startX, int endX)
    {
        List<int> fullLines = new List<int>();

        // Find full lines
        for (int y = 0; y < boundaries.maxHeight; y++)
        {
            if (IsLineFull(y, startX, endX))
            {
                fullLines.Add(y);
            }
        }

        if (fullLines.Count > 0)
        {
            // Add points
            gameObject.GetComponent<Highscore>().addPointsForLines(fullLines.Count);

            // Clear lines and move blocks down
            ClearLinesAndMoveBlocks(fullLines, startX, endX);

            // Play sound
            var audioSource = GameObject.Find("FullLine")?.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    private bool IsLineFull(int y, int startX, int endX)
    {
        for (int x = startX; x <= endX; x++)
        {
            if (!isCube[x, y])
            {
                return false;
            }
        }
        return true;
    }

    private void ClearLinesAndMoveBlocks(List<int> fullLines, int startX, int endX)
    {
        foreach (int lineY in fullLines.OrderBy(y => y))
        {
            // Clear the line
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Cube"))
            {
                Vector3 cubePos = cube.transform.position;
                int cubeX = Mathf.RoundToInt(cubePos.x);
                int cubeY = Mathf.RoundToInt(cubePos.y);

                // Only process cubes in current platform
                if (cubeX >= startX && cubeX <= endX)
                {
                    if (cubeY == lineY)
                    {
                        // Destroy cube or parent if it's the last cube
                        if (cube.transform.parent.childCount == 1)
                        {
                            Destroy(cube.transform.parent.gameObject);
                        }
                        else
                        {
                            Destroy(cube);
                        }
                    }
                    else if (cubeY > lineY)
                    {
                        // Move cube down
                        cube.transform.position += Vector3.down;
                    }
                }
            }
        }
    }
}