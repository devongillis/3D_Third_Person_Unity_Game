using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tileMaster : MonoBehaviour
{
    // this script holds all the locations of the tiles in a 4x4 grid
    // and receives calls for movement permissions, it then checks if
    // the move is legal and sends a response
    public const int caveDoorOpenObjectiveID = 0;

    Vector3[][] grid;
    public GameObject[] tiles;
    public bool inuse = false;
    public GameObject masterObject;
    //public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        grid = new Vector3[4][];
        for (int i = 0; i < 4; i++)
        {
            grid[i] = new Vector3[4];
            
        }
        
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                grid[i][j] = new Vector3(-4 + (-8 + j * 8), (i * 4 + j) + 1, 4 + (8 - 8 * i));
            }
        }
        grid[3][3] = new Vector3(-4 + 16, 0, 4 + -16);


        AssignGrid();
        
        
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                /*
                if(i == 3 && j == 3)
                {

                }
                else
                {
                    tiles[(int)grid[i][j].y - 1].transform.localPosition = new Vector3(grid[i][j].x, 0, grid[i][j].z);
                    tiles[(int)grid[i][j].y - 1].GetComponent<tileMove>().AssignValues((int)grid[i][j].y, i * 4 + j);
                }
                */
                if(grid[i][j].y == 0)
                {

                }
                else
                {
                    tiles[(int)grid[i][j].y - 1].transform.localPosition = new Vector3(grid[i][j].x, 0, grid[i][j].z);
                    tiles[(int)grid[i][j].y - 1].GetComponent<tileMove>().AssignValues((int)grid[i][j].y, i * 4 + j);
                }
            }
        }

        //Debug.Log(MoveTile(12, 11, 2) + " " + new Vector2(grid[3][3].x, grid[3][3].z));
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void AssignGrid()
    {
        grid[0][0].y = 1;
        grid[0][1].y = 2;
        grid[0][2].y = 3;
        grid[0][3].y = 4;

        grid[1][0].y = 5;
        grid[1][1].y = 6;
        grid[1][2].y = 7;
        grid[1][3].y = 8;

        grid[2][0].y = 9;
        grid[2][1].y = 10;
        grid[2][2].y = 11;
        grid[2][3].y = 12;

        grid[3][0].y = 13;
        grid[3][1].y = 14;
        grid[3][2].y = 0;
        grid[3][3].y = 15; // 0 means empty
    }

    public void CheckIfSolved()
    {
        // this function will check if the solution is met, if so then all scripts and triggers should be
        // disabled
        bool solved = true;
        for(int i = 0; i < 4; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                //Debug.Log(grid[i][j].y + " " + i * 4 + j + 1);
                if(i == 3 && j == 3)
                {
                    // special case
                    if(grid[3][3].y != 0)
                    {
                        solved = false;
                    }
                }
                else if(grid[i][j].y != i * 4 + j + 1)
                {
                    solved = false;
                }
            }
        }

        if (solved)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<tileMove>().DisableTile();
            }
            // trigger a cut scene showing we won
            //TriggerCutScene();
            masterObject.GetComponent<masterScript>().ObjectiveCompleted(caveDoorOpenObjectiveID);
            // maybe a sound effect too
        }
    }
    /*
    public void TriggerCutScene()
    {
        Debug.Log("winner");
        //door.GetComponent<openDoor>().OpenDoor();
        masterObject.GetComponent<CutSceneManager>().StartOpenCaveDoorCutScene();
    }
    */
    public Vector3 MoveTile(int tileNumber, int position, int desiredDirection)
    {
        //Debug.Log(tileNumber + " " + position + " " + desiredDirection + " " + inuse);
        // tile number is 1-15
        // position 0-15
        // direction = 0 up, 1 right, 2 down, 3 left
        // first check if desired location is empty and if is within range
        int j = position % 4;
        int i = (position - j) / 4;
        if (!inuse)
        {
            inuse = true;
            if (desiredDirection == 0)
            {
                // up
                if (i - 1 >= 0 && i - 1 <= 3)
                {
                    // location exists
                    if (grid[i - 1][j].y == 0)
                    {
                        // move is allowed
                        // make the ID swap and then return the new location
                        grid[i - 1][j].y = tileNumber;
                        grid[i][j].y = 0;
                        return new Vector3(grid[i - 1][j].x, ((i - 1) * 4 + j), grid[i - 1][j].z);
                    }
                }
            }
            else if (desiredDirection == 1)
            {
                // up
                if (j + 1 >= 0 && j + 1 <= 3)
                {
                    // location exists
                    if (grid[i][j + 1].y == 0)
                    {
                        // move is allowed
                        // make the ID swap and then return the new location
                        grid[i][j + 1].y = tileNumber;
                        grid[i][j].y = 0;
                        return new Vector3(grid[i][j + 1].x, ((i * 4) + (j + 1)), grid[i][j + 1].z);
                    }
                }
            }
            else if (desiredDirection == 2)
            {
                // up
                if (i + 1 >= 0 && i + 1 <= 3)
                {
                    // location exists
                    if (grid[i + 1][j].y == 0)
                    {
                        // move is allowed
                        // make the ID swap and then return the new location
                        grid[i + 1][j].y = tileNumber;
                        grid[i][j].y = 0;
                        return new Vector3(grid[i + 1][j].x, ((i + 1) * 4 + j), grid[i + 1][j].z);
                    }
                }
            }
            else if (desiredDirection == 3)
            {
                // up
                if (j - 1 >= 0 && j - 1 <= 3)
                {
                    // location exists
                    if (grid[i][j - 1].y == 0)
                    {
                        // move is allowed
                        // make the ID swap and then return the new location
                        grid[i][j - 1].y = tileNumber;
                        grid[i][j].y = 0;
                        return new Vector3(grid[i][j - 1].x, ((i * 4) + (j - 1)), grid[i][j - 1].z);
                    }
                }
            }
        }
        return new Vector3(grid[i][j].x, position, grid[i][j].z);
    }
}
