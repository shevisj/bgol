using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBase : MonoBehaviour
{
    public int rows = 9;
    public int columns = 10;
    public KeyCode[] left_controls = new KeyCode[4] {KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F};
    public KeyCode[] right_controls = new KeyCode[4] {KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon};

    private int p_rows = 5;
    private int p_columns = 5;

    public GameObject cellPrefab;

    private List<List<Vector3>> gridPoints = new List<List<Vector3>>();
    private List<List<GameObject>> cubeMap = new List<List<GameObject>>();


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
            for (int j = 0; j < columns; ++j) {
                cubeMap[i].Add(Instantiate(cellPrefab, gridPoints[i][j], Quaternion.identity, transform));
                // Color
                var cubeRenderer = cubeMap[i][j].GetComponent<Renderer>();
                cubeRenderer.material.SetColor("_Color", Color.red);
                // Deactivate
                cubeMap[i][j].SetActive(false);
            }
        }
    }

    private void ConnectGridNodes() {
        for (int i = 0; i < rows; ++i) {
            for (int j = 0; j < columns; ++j) {
                var node = cubeMap[i][j].GetComponent<GridNode>();
                // down
                if (j > 0) {
                    var downNode = cubeMap[i][j-1].GetComponent<GridNode>();
                    node.down = downNode;
                    downNode.up = node;
                }
                // down left
                if (j > 0 && i > 0) {
                    var downLeftNode = cubeMap[i - 1][j-1].GetComponent<GridNode>();
                    node.downLeft = downLeftNode;
                    downLeftNode.upRight = node;
                }
                // down right
                if (j > 0 && i < (rows-1)) {
                    var downRightNode = cubeMap[i + 1][j - 1].GetComponent<GridNode>();
                    node.downRight = downRightNode;
                    downRightNode.upLeft = node;
                }
                // left
                if (i > 0) {
                    var leftNade = cubeMap[i - 1][j].GetComponent<GridNode>();
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
    }

    private void Start() {
        ResetGrid();
    }

    private void Update() {
        if (transform.hasChanged || p_rows != rows || p_columns != columns) {
            ResetGrid();
            transform.hasChanged = false;
            p_rows = rows;
            p_columns = columns;
        }
    
        // Get user input
        for (int i = 0; i < left_controls.Length; ++i) {
            if (Input.GetKeyDown(left_controls[i])) {
                //Debug.Log("Left down: "+i.ToString());
                cubeMap[i][0].SetActive(true);
            } else if (Input.GetKeyUp(left_controls[i])) {
                //Debug.Log("Left up: "+i.ToString());
                cubeMap[i][0].SetActive(false);
            }
        }

        for (int i = 0; i < right_controls.Length; ++i) {
            int idx = cubeMap.Count - (right_controls.Length - i);
            if (Input.GetKeyDown(right_controls[i])) {
                //Debug.Log("Right down: "+idx.ToString());
                cubeMap[idx][0].SetActive(true);
            } else if (Input.GetKeyUp(right_controls[i])) {
                //Debug.Log("Right up: "+idx.ToString());
                cubeMap[idx][0].SetActive(false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        GenerateGrid(true);
    }
}