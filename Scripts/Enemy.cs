using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Queue<Tile> visited;        //Queue of Visited Tiles 
    private Queue<Tile> reachable;      //Queue of Tile to visit 
    private Stack<Tile> pathStack;      //Stack of Tiles that represent the path to take

    private Tile startTile = null;
    private Tile endTile = null;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        visited = new Queue<Tile>();
        reachable = new Queue<Tile>();
        pathStack = new Stack<Tile>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setStartTile(Tile tile)
    {
        startTile = tile;
    }

    public void validatePathfind()
    {
        //If another enemy is currently using the world nodes, enter the wait queue
        if (gameManager.pathfindingLocked())
        {
            gameManager.waitToPathfind(this);
        }

        //otherwise begin pathfinding
        else
        {
            gameManager.setPathfindingLock(true);
            pathfind();
        }
    }

    private void pathfind()
    {
        //Ensure All lists begin empty
        visited.Clear();
        reachable.Clear();
        pathStack.Clear();

        //Initialize Queues
        startTile.setVisited(true);
        visited.Enqueue(startTile);
        findReachableNodes_SENW(startTile);

        //BFS
        while(reachable.Count > 0)
        {
            //Check if the next tile is a destination
            if(reachable.Peek().transform.position.y == 0)
            {
                endTile = reachable.Peek();
                break;
            }

            reachable.Peek().setVisited(true);
            visited.Enqueue(reachable.Peek());
            findReachableNodes_SENW(reachable.Dequeue());
        }

        //Populate Path Stack
        if(endTile != null)
        {
            populatePathStack(endTile);
            endTile = null;
        }

        //Flush node data for next pathfind
        flushNodeData();

        //release pathfind lock and signal next pathfind
        gameManager.setPathfindingLock(false);
        gameManager.nextPathfind();

    }

    private void populatePathStack(Tile tile)
    {
        if(tile != null)
        {
            pathStack.Push(tile);
            populatePathStack(tile.getPrevious());
        }
    }

    private void findReachableNodes_SENW(Tile root)
    {
        //South
        if(root.getNeighborDown() != null)
        {
            if (root.getNeighborDown().isValidPathTile())
            {
                root.getNeighborDown().setPrevious(root);
                reachable.Enqueue(root.getNeighborDown());
            }
        }

        //East
        if (root.getNeighborRight() != null)
        {
            if (root.getNeighborRight().isValidPathTile())
            {
                root.getNeighborRight().setPrevious(root);
                reachable.Enqueue(root.getNeighborRight());
            }
        }

        //North
        if (root.getNeighborUp() != null)
        {
            if (root.getNeighborUp().isValidPathTile())
            {
                root.getNeighborUp().setPrevious(root);
                reachable.Enqueue(root.getNeighborUp());
            }
        }

        //West
        if (root.getNeighborLeft() != null)
        {
            if (root.getNeighborLeft().isValidPathTile())
            {
                root.getNeighborLeft().setPrevious(root);
                reachable.Enqueue(root.getNeighborLeft());
            }
        }
    }

    private void flushNodeData()
    {
        while(visited.Count > 0)
        {
            visited.Peek().setVisited(false);
            visited.Dequeue().setPrevious(null);
        }

        while(reachable.Count > 0)
        {
            reachable.Peek().setVisited(false);
            reachable.Dequeue().setPrevious(null);
        }
    }
    
}
