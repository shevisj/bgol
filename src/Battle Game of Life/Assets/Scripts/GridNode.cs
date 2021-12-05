using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode : MonoBehaviour
{
    public GridNode up = null;
    public GridNode down = null;
    public GridNode left = null;
    public GridNode right = null;
    public GridNode upLeft = null;
    public GridNode downLeft = null;
    public GridNode upRight = null;
    public GridNode downRight = null;
    private bool nextState = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // up
        //if (!(up is null) && !up.gameObject.activeInHierarchy) {
        //    up.gameObject.SetActive(true);
        //}
        //if (!(down is null) && !down.gameObject.activeInHierarchy) {
        //    gameObject.SetActive(false);
        //}
    }

    public void CalculateNextState() {
        GridNode[] nodes = {up, upRight, right, downRight, down, downLeft, left, upLeft};
        Dictionary<bool, int> counter = new Dictionary<bool, int>() {{true, 0}, {false, 0}};
        foreach (GridNode node in nodes) {
            if (!(node is null)) {
                counter[node.gameObject.activeInHierarchy]++;
            }
        }
        if (counter[true] == 3) {
            // Birth
            nextState = true;
            return;
        } else if (counter[true] <= 1 || counter[true] >= 4) {
            // Death
            nextState = false;
            return;
        }
        // survival
        nextState = gameObject.activeInHierarchy;
    }

    public void TransitionToNextState() {
        gameObject.SetActive(nextState);
    }
}
