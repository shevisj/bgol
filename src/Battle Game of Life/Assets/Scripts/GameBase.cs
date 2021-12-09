using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState {
    PREGAME,
    ACTIVE,
    LOSE,
    WIN
}

public class GameBase : MonoBehaviour
{
    public int rows = 9;
    public int columns = 10;
    public KeyCode[] left_controls = new KeyCode[4] {KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F};
    public KeyCode[] right_controls = new KeyCode[4] {KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon};

    public GameObject cellPrefab;

    public Vector2[] victoryPoints = new Vector2[] {new Vector2(2, 4), new Vector2(1, 4)};
    private List<List<Vector3>> gridPoints = new List<List<Vector3>>();
    private List<List<GameObject>> cubeMap = new List<List<GameObject>>();
    private List<List<GridNode>> nodeMap = new List<List<GridNode>>();
    private List<GridNode> goalNodes = new List<GridNode>();
    private Renderer r;

    [Range(0f, 3f)]
    public float stepDelaySeconds = 1f;
    private int cursor = 0;
    public bool timeBased = false;
    public Vector4 start = new Vector4(0.2f, 0.2f, 1f, 1f);
    public Vector4 end = new Vector4(0.4f, 0.4f, 1f, 1f);

    public GameState state = GameState.PREGAME;
    private int stepCount = 0;
    public Text stepCounter = null;


    private void GenerateGrid(bool renderGizmos=false) {
        foreach (List<Vector3> row in gridPoints) {
            row.Clear();
        }
        gridPoints.Clear();
        float XOffset = -5f * transform.localScale.x;
        float ZOffset = -5f * transform.localScale.z;
        float XTarget = 5f * transform.localScale.x;
        float ZTarget = 5f * transform.localScale.z;
        Gizmos.color = Color.yellow;
        float initialX = transform.position.x + XOffset;
        float targetX = transform.position.x - XOffset;
        float initialZ = transform.position.z + ZOffset;
        float targetZ = transform.position.z - ZOffset;
        float xSize = (2 * Mathf.Abs(XOffset)) / (float)rows;
        float zSize = (2 * Mathf.Abs(ZOffset)) / (float)columns;
        float innerXOffest = .5f * xSize;
        float innerZOffest = .5f * zSize;
        for (float x = initialX; x < (XOffset - innerXOffest + (rows * xSize)); x += xSize)
        {
            List<Vector3> subList = new List<Vector3>();
            for (float z = initialZ; z < (ZOffset - innerZOffest + (columns * zSize)); z += zSize)
            {
                Vector3 gridPoint = new Vector3(x + innerXOffest, 0f, z + innerZOffest);
                if (renderGizmos) {
                    Gizmos.DrawSphere(gridPoint, 0.1f);
                }
                subList.Add(gridPoint);
            }
            gridPoints.Add(subList);
        }
    }

    private void DestryCubes() {
        nodeMap.Clear();
        goalNodes.Clear();
        foreach (List<GameObject> row in cubeMap) {
            foreach (GameObject cube in row) {
                GameObject.Destroy(cube);
            }
            row.Clear();
        }
        cubeMap.Clear();
    }

    private void SpawnCubes() {
        for (int i = 0; i < rows; ++i) {
            cubeMap.Add(new List<GameObject>());
            nodeMap.Add(new List<GridNode>());
            for (int j = 0; j < columns; ++j) {
                cubeMap[i].Add(Instantiate(cellPrefab, gridPoints[i][j], Quaternion.identity, transform));
                nodeMap[i].Add(cubeMap[i][j].GetComponent<GridNode>());
                if (Array.IndexOf(victoryPoints, new Vector2(i, j)) > -1) {
                    // Victory node is green and remains active
                    nodeMap[i][j].goal = true;
                    nodeMap[i][j].Recolor();
                    goalNodes.Add(nodeMap[i][j]);
                }
            }
        }
    }

    private void ConnectGridNodes() {
        for (int i = 0; i < rows; ++i) {
            for (int j = 0; j < columns; ++j) {
                GridNode node = nodeMap[i][j];
                // down
                if (j > 0) {
                    GridNode downNode = nodeMap[i][j - 1];
                    node.down = downNode;
                    downNode.up = node;
                }
                // Wrap down
                //if (j == 0) {
                //    GridNode downNode = nodeMap[i][nodeMap[i].Count - 1];
                //    node.down = downNode;
                //    downNode.up = node;
                //}
                // down left
                if (j > 0 && i > 0) {
                    GridNode downLeftNode = nodeMap[i - 1][j - 1];
                    node.downLeft = downLeftNode;
                    downLeftNode.upRight = node;
                }
                // Wrap down left
                if (j > 0 && i == 0) {
                    GridNode downLeftNode = nodeMap[nodeMap.Count - 1][j - 1];
                    node.downLeft = downLeftNode;
                    downLeftNode.upRight = node;
                }
                // down right
                if (j > 0 && i < (rows-1)) {
                    GridNode downRightNode = nodeMap[i + 1][j - 1];
                    node.downRight = downRightNode;
                    downRightNode.upLeft = node;
                }
                // Wrap down right
                if (j > 0 && i == (rows-1)) {
                    GridNode downRightNode = nodeMap[0][j - 1];
                    node.downRight = downRightNode;
                    downRightNode.upLeft = node;
                }
                // left
                if (i > 0) {
                    GridNode leftNade = nodeMap[i - 1][j];
                    node.left = leftNade;
                    leftNade.right = node;
                }
                // Wrap Left
                if (i == 0) {
                    GridNode leftNade = nodeMap[nodeMap.Count - 1][j];
                    node.left = leftNade;
                    leftNade.right = node;
                }
            }
        }
    }

    private void ResetGrid() {
        GenerateGrid();
        DestryCubes();
        SpawnCubes();
        ConnectGridNodes();
        state = GameState.PREGAME;
        stepCount = 0;
        cursor = 0;
        r.material.SetColor("_Color", start);
    }

    private void CalculateNextGridState() {
        // First, calculate next step for each node
        foreach (List<GridNode> row in nodeMap) {
            foreach (GridNode node in row) {
                node.CalculateNextState();
            }
        }
    }

    private void TransitionGridToNextState() {
        // The execute the transition to the next step
        foreach (List<GridNode> row in nodeMap) {
            foreach (GridNode node in row) {
                node.TransitionToNextState();
            }
        }
    }

    private bool checkWin() {
        foreach (GridNode goalNode in goalNodes) {
            if (goalNode.state == NodeState.ACTIVE) {
                return true;
            }
        }
        return false;
    }

    private void EvolveGrid() {
        CalculateNextGridState();
        TransitionGridToNextState();
        if (checkWin()) {
            state = GameState.WIN;
        }
    }

    private void Start() {
        r = gameObject.GetComponent<Renderer>();
        ResetGrid();
    }

    private void FixedUpdate() {
        if (timeBased && state == GameState.ACTIVE) {
            ++cursor;
            int cursorLimit = Mathf.RoundToInt(stepDelaySeconds * 50f);
            float compPerc = (float)cursor / (float)cursorLimit;
            Color newColor = Vector4.Lerp(start, end, compPerc);
            r.material.SetColor("_Color", newColor);
            if (cursor >= cursorLimit) {
                EvolveGrid();
                cursor = 0;
                ++stepCount;
            }
        }
    }

    private void Update() {
        if (Input.GetKey("escape")) {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.Space) && timeBased) {
            ResetGrid();
        }
        if (state == GameState.WIN || state == GameState.LOSE) {
            return;
        }
        // Get user input
        for (int i = 0; i < left_controls.Length; ++i) {
            if (Input.GetKeyDown(left_controls[i]) || Input.GetKey(left_controls[i])) {
                //Debug.Log("Left down: "+i.ToString());
                state = GameState.ACTIVE;
                nodeMap[i][0].Activate();
                //CalculateNextGridState();
            } else if (Input.GetKeyUp(left_controls[i])) {
                //Debug.Log("Left up: "+i.ToString());
                nodeMap[i][0].Deactivate();
                //CalculateNextGridState();
            }
        }

        for (int i = 0; i < right_controls.Length; ++i) {
            int idx = cubeMap.Count - (right_controls.Length - i);
            if (Input.GetKeyDown(right_controls[i]) || Input.GetKey(right_controls[i])) {
                //Debug.Log("Right down: "+idx.ToString());
                state = GameState.ACTIVE;
                nodeMap[idx][0].Activate();
                //CalculateNextGridState();
            } else if (Input.GetKeyUp(right_controls[i])) {
                //Debug.Log("Right up: "+idx.ToString());
                nodeMap[idx][0].Deactivate();
                //CalculateNextGridState();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && !timeBased) {
            state = GameState.ACTIVE;
            EvolveGrid();
            ++stepCount;
        }
    }

    private void OnDrawGizmos()
    {
        GenerateGrid(true);
    }

    void OnGUI() {
        if (!(stepCounter is null)) {
            stepCounter.text = "Steps: " + stepCount.ToString();
        }
    }
}