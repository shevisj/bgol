using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState
{
    INACTIVE,
    PRIMED,
    ACTIVE,
    ANTIPRIMED,
}

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
    public NodeState state = NodeState.INACTIVE;
    public NodeState nextState = NodeState.INACTIVE;
    public bool goal = false;
    public Dictionary<NodeState, NodeState> stateTransitionMap = new Dictionary<NodeState, NodeState>() {
        {NodeState.INACTIVE, NodeState.INACTIVE},
        {NodeState.PRIMED, NodeState.ACTIVE},
        {NodeState.ACTIVE, NodeState.ACTIVE},
        {NodeState.ANTIPRIMED, NodeState.INACTIVE},
    };
    public Dictionary<NodeState, Color> stateColorMap = new Dictionary<NodeState, Color>() {
        {NodeState.INACTIVE, Color.clear},
        {NodeState.PRIMED, Color.cyan},
        {NodeState.ACTIVE, Color.blue},
        {NodeState.ANTIPRIMED, Color.gray},
    };
    public Dictionary<NodeState, Color> goalColorMap = new Dictionary<NodeState, Color>() {
        {NodeState.INACTIVE, Color.yellow},
        {NodeState.PRIMED, Color.yellow},
        {NodeState.ACTIVE, Color.green},
        {NodeState.ANTIPRIMED, Color.gray},
    };
    private Renderer rend;

    void Awake()
    {
        rend = gameObject.GetComponent<Renderer>();
        Recolor();
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
        Dictionary<NodeState, int> counter = new Dictionary<NodeState, int>() {
            {NodeState.INACTIVE, 0},
            {NodeState.PRIMED, 0},
            {NodeState.ACTIVE, 0},
            {NodeState.ANTIPRIMED, 0}
        };
        foreach (GridNode node in nodes) {
            if (!(node is null)) {
                counter[node.state]++;
            }
        }

        if (counter[NodeState.ACTIVE] == 3) {
            // Birth
            nextState = NodeState.ACTIVE;
        } else if (counter[NodeState.ACTIVE] <= 1 || counter[NodeState.ACTIVE] >= 4) {
            // Death
            nextState = NodeState.INACTIVE;
        } else {
            nextState = state;
        }
        
    }

    public void Activate() {
        state = NodeState.ACTIVE;
        Recolor();
    }

    public void Deactivate() {
        state = NodeState.INACTIVE;
        Recolor();
    }

    public void Recolor() {
        if (goal) {
            rend.material.SetColor("_Color", goalColorMap[state]);
        } else {
            rend.material.SetColor("_Color", stateColorMap[state]);
        }
    }

    public void TransitionToNextState() {
        state = nextState;
        Recolor();
    }
}
