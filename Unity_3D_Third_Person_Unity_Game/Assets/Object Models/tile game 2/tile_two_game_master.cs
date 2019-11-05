using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tile_two_game_master : MonoBehaviour
{
    // Start is called before the first frame update
    // this script will set the images if the tiles for us
    // and is responsible for all calls to flip the tiles and
    // keeps score, might later include a timer

    public bool tileIsCurrentlyFlipping = false;

    public int pairsMatched;
    public int[][] tilePairs;
    public int tile1 = 0;
    public int tile2 = 0;

    //public Material[] materials;
    public GameObject[] tiles; // check if prefab will keep track if its children

    void Start()
    {

        tilePairs = new int[8][];
        for (int i = 0; i < 8; i++)
        {
            tilePairs[i] = new int[2];
        }

        for(int i = 0; i < 16; i++)
        {
            tiles[i].GetComponent<tile_game_two_tile_script>().SetID(i + 1);
        }

        // blue
        tilePairs[0][0] = 1;
        tilePairs[0][1] = 7;
        // red
        tilePairs[1][0] = 6;
        tilePairs[1][1] = 9;
        // green
        tilePairs[2][0] = 3;
        tilePairs[2][1] = 2;
        // orange
        tilePairs[3][0] = 16;
        tilePairs[3][1] = 4;
        // pink
        tilePairs[4][0] = 5;
        tilePairs[4][1] = 15;
        // dark blue
        tilePairs[5][0] = 8;
        tilePairs[5][1] = 10;
        // yellow
        tilePairs[6][0] = 11;
        tilePairs[6][1] = 13;
        // purple
        tilePairs[7][0] = 14;
        tilePairs[7][1] = 12;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool AllowedToFlip(int ID)
    {
        //Debug.Log(tile1 + " " + tile2);
        // ID (1-16)
        // this will only return true if no other tiles are currently flipping
        // thus only one tile can flip at a time
        // it also uses the id to keep track of flipped tiles
        if (!tileIsCurrentlyFlipping)
        {
            tileIsCurrentlyFlipping = true;
            if (tile1 == 0)
            {
                // first tile being flipped in a sequence
                tile1 = ID;
            }
            else
            {
                // second tile being flipped in a sequence
                tile2 = ID;
            }
            Debug.Log(tile1 + " " + tile2);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CheckIfPairMatched()
    {
        // this function is called after the end of each tile flip, if has a list
        // of currently flipped and solved tiles and will disable the game if a winner

        // check if a pair is matched
        if (tile1 != 0 && tile2 != 0)
        {
            // two tiles have been selected, we can check them
            bool pairFound = false;
            for (int i = 0; i < 8; i++)
            {
                if (tilePairs[i][0] == tile1 && tilePairs[i][1] == tile2)
                {
                    // we have a pair
                    pairsMatched++;
                    CheckIfSolved();
                    pairFound = true;
                }
                else if (tilePairs[i][0] == tile2 && tilePairs[i][1] == tile1)
                {
                    // we have a pair
                    pairsMatched++;
                    CheckIfSolved();
                    pairFound = true;
                }
            }
            if (!pairFound)
            {
                // not a pair, reset the tiles
                tiles[tile1 - 1].GetComponent<tile_game_two_tile_script>().ResetTile();
                tiles[tile2 - 1].GetComponent<tile_game_two_tile_script>().ResetTile();
                tileIsCurrentlyFlipping = true;
            }
            tile1 = 0;
            tile2 = 0;
        }
    }

    public void CheckIfSolved()
    {
        if (pairsMatched >= 8)
        {
            for(int i = 0; i <tiles.Length; i++)
            {
                tiles[i].GetComponent<tile_game_two_tile_script>().DisableTrigger();
            }
        }
    }
}
