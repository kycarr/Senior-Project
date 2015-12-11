﻿using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

// Class that holds a min and max for a count
public class Count {
    public int minimum;
    public int maximum;

    public Count(int min, int max) {
        minimum = min;
        maximum = max;
    }
}

// Class that holds a grid of available locations
public class Grid {
    // Possible locations to place tiles
    public List<Vector3> gridPositions = new List<Vector3>();
    public int columns, rows;

    // Clears the list of gridPositions and prepares it to generate a new tile
    public Grid(int col, int row) {
        gridPositions.Clear();
        columns = col;
        rows = row;
        initialiseList();
    }

    // Adds all the tiles to the grid
    public void initialiseList() {
        for (int x = 0; x < columns; x++) {
            for (int y = 0; y < rows; y++)
                gridPositions.Add(new Vector3(x, y, 0f));
        }
    }

    // # of positions in the grid
    public int count() {
        return gridPositions.Count;
    }

    // Finds the index of the tile with given coordinates in the grid
    public int findTile(List<Vector3> grid, float x, float y) {
        for (int i = 0; i < grid.Count; i++) {
            if (grid[i].x == x && grid[i].y == y)
                return i;
        }
        return -1;
    }

    // Removes the tile from the list of possible grid locations
    public void removeTile(float x, float y) {
        int find = findTile(gridPositions, x, y);
        if (find != -1)
            gridPositions.RemoveAt(find);
    }
    public void removeAt(int index) {
        gridPositions.RemoveAt(index);
    }

    // Adds the tile back into the list of possible grid locations
    public void addTile(float x, float y)
    {
        gridPositions.Add(new Vector3(x, y, 0));
    }

    // Remove all tiles that overlap with the object
    public void removeAdjacent(float locX, float locY, int sizeX, int sizeY) {
        for (int x = 1; x < sizeX; x++) {
            for (int y = 1; y < sizeY; y++) {
                removeTile(locX + x, locY);
                removeTile(locX - x, locY);
                removeTile(locX, locY + y);
                removeTile(locX, locY - y);
                removeTile(locX + x, locY + y);
                removeTile(locX + x, locY - y);
                removeTile(locX - x, locY + y);
                removeTile(locX - x, locY - y);
            }
        }
    }

    // Returns a random position from the grid where the object can be placed
    public Vector3 RandomPosition(int sizeX, int sizeY) {
        int index = Random.Range(0, gridPositions.Count);
        Vector3 pos = gridPositions[index];

        // Object goes off the edge of the tile grid
        while (pos.x - sizeX + 1 < 0 || pos.x + sizeX - 1 > (columns - 1) || pos.y - sizeY + 1 < 0 || pos.y + sizeY - 1 > (rows - 1) ||
               findTile(gridPositions, pos.x - sizeX + 1, pos.y) == -1 || findTile(gridPositions, pos.x + sizeX - 1, pos.y) == -1 ||
               findTile(gridPositions, pos.x, pos.y + sizeY - 1) == -1 || findTile(gridPositions, pos.x, pos.y - sizeY + 1) == -1) {
            index = Random.Range(0, gridPositions.Count);
            pos = gridPositions[index];
        }
        removeTile(pos.x, pos.y);
        removeAdjacent(pos.x, pos.y, sizeX, sizeY);
        return pos;
    }
}

public class TileManager : MonoBehaviour {
    // Constants
    private int columns = 10;       // # of columns in a tile
    private int rows = 10;          // # of rows in a tile
    
    // Tile information
    private String tileType;        // the type of tile this is
    private int tileRow, tileCol;   // location of this tile on the map
    private int bSizeX, bSizeY;     // size of buildings on this tile
    private Count buildingCount;    // # of buildings that can spawn on this tile
    private Count wallCount;        // # of walls that can spawn on this tile
    private Count npcCount;         // # of people that can spawn on this tile

    // Prefab objects
    public GameObject npc;               // prefab for an npc
    private GameObject exitTile;         // prefab for the exit tile
    private GameObject roadTile;         // prefab for the road tile
    private GameObject[] outerWallTiles; // prefab for the outer-wall tile
    private GameObject[] floorTiles;     // Array of floor prefabs
    private GameObject[] buildingTiles;  // Array of building prefabs
    private GameObject[] wallTiles;      // Array of wall prefabs

    // Grid information
    private Transform boardHolder;       // Reference to the transform of the tile
    public Grid grid;                   // Possible locations to place tiles

    // All objects on this tile
    public List<GameObject> floors = new List<GameObject>();        // prefabs for the floor
    public List<Vector3> floorLocations = new List<Vector3>();      // location of floors
    public List<GameObject> buildings = new List<GameObject>();     // prefabs for the buildings
    public List<Vector3> buildingLocations = new List<Vector3>();   // location of buildings
    public List<GameObject> walls = new List<GameObject>();         // prefabs for the walls
    public List<Vector3> wallLocations = new List<Vector3>();       // location of walls
    public List<GameObject> npcs = new List<GameObject>();          // prefabs for the npcs
    public List<Vector3> npcLocations = new List<Vector3>();        // location of npcs


    // Sets up the walls, exits and floor of the tile
    void BoardSetup() {
        boardHolder = new GameObject("Board " + tileRow + " " + tileCol).transform;

        for (int x = 0; x < columns; x++) {
            for (int y = 0; y < rows; y++) {
                // Create a random floor tile
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                // Edge of the map; create an impassable wall
                if ((x == 0 && tileCol == 0) || (x == (columns - 1) && tileCol == (columns - 1)) || (y == 0 && tileRow == 0) || (y == (rows - 1) && tileRow == (rows - 1))) {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                    grid.removeTile(x, y);
                }
                // Roads; don't allow anything to go here
                if (x == 4 || x == 5 || y == 4 || y == 5) {
                    grid.removeTile(x, y);
                }

                // Add the tile to the game
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
                instance.SetActive(false);
                floors.Add(instance);
                floorLocations.Add(new Vector3(x, y, 0f));
            }
        }
    }

    // Takes in an array of game objects and randomly places them on the map
    void LayoutObjectAtRandom(GameObject[] tileArray, int min, int max, int sizeX, int sizeY, List<GameObject> objs, List<Vector3> pos) {
        int objectCount = Random.Range(min, max + 1);

        // Nothing can be placed on the current tile
        if (grid.count() == 0)
            return;

        for (int i = 0; i < objectCount; i++) {
            // Where and what to place
            Vector3 randomPosition = grid.RandomPosition(sizeX, sizeY);
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            randomPosition.z = 10 - randomPosition.y;
            // Add the object
            GameObject instance = Instantiate(tileChoice, randomPosition, Quaternion.identity) as GameObject;
            instance.transform.SetParent(boardHolder);
            instance.SetActive(false);
            objs.Add(instance);
            pos.Add(randomPosition);
        }
    }

    // Places NPCs randomly on the map
    void LayoutNPCs() {
        int objectCount = Random.Range(npcCount.minimum, npcCount.maximum + 1);

        // Nothing can be placed on the current tile
        if (grid.count() == 0)
            return;

        for (int i = 0; i < objectCount; i++) {
            Vector3 randomPosition = grid.RandomPosition(1, 1);
            randomPosition.z = 10 - randomPosition.y;
            GameObject instance = Instantiate(npc, randomPosition, Quaternion.identity) as GameObject;
            instance.transform.SetParent(boardHolder);
            instance.GetComponent<NPC>().init();
            instance.GetComponent<NPC>().PlaceAt(tileCol, tileRow, (int)randomPosition.x, (int)randomPosition.y, (int)randomPosition.z);
            instance.SetActive(false);
            
            npcs.Add(instance);
            npcLocations.Add(randomPosition);
        }
    }


    // Tells the tile what to use as sprites
    public void SetupSprites(GameObject road, GameObject[] outerwall, GameObject[] floor, GameObject[] building, GameObject[] wall) {
        roadTile = road;
        outerWallTiles = outerwall;
        floorTiles = floor;
        buildingTiles = building;
        wallTiles = wall;
    }

    // Initializes and lays out the tile
    public void SetupScene(int tRow, int tCol, String type) {
        // Instantiate the tile
        tileType = type;            // what type of tile is this?
        tileRow = tRow;             // where is the tile on the map?
        tileCol = tCol;

        // Tile is a market; has lots of stalls and people
        if (tileType.Equals("Market")) {
            buildingCount = new Count(1, 2);
            wallCount = new Count(0, 0);
            npcCount = new Count(1, 2);
            bSizeX = 2;
            bSizeY = 2;
        }
        // Tile is a town; has houses and people
        else if (tileType.Equals("Town")) {
            buildingCount = new Count(1, 2);
            wallCount = new Count(0, 5);
            npcCount = new Count(1, 2);
            bSizeX = 2;
            bSizeY = 2;
        }
        // Tile is a forest; has trees and bushes
        else if (tileType.Equals("Forest")) {
            buildingCount = new Count(0, 0);
            wallCount = new Count(10, 30);
            npcCount = new Count(0, 0);
            bSizeX = 1;
            bSizeY = 1;
        }
        // Tile is a cave entrance; has rocks and caves
        else if (tileType.Equals("Cave")) {
            buildingCount = new Count(1, 1);
            wallCount = new Count(5, 10);
            npcCount = new Count(0, 0);
            bSizeX = 2;
            bSizeY = 2;
        }
        // Tile is a farm; has crops and animals
        else if (tileType.Equals("Farm")) {
            buildingCount = new Count(0, 0);
            wallCount = new Count(5, 20);
            npcCount = new Count(0, 1);
            bSizeX = 2;
            bSizeY = 2;
        }
        
        // Creates a new grid and sets up the floor and outerwalls
        grid = new Grid(columns, rows);
        BoardSetup();

        // Instantiates and places a random number of objects
        LayoutObjectAtRandom(buildingTiles, buildingCount.minimum, buildingCount.maximum, bSizeX, bSizeY, buildings, buildingLocations);
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum, 1, 1, walls, wallLocations);
        LayoutNPCs();
    }

    // Draw this tile to the screen
    public void Draw() {
        // Draw the floor
        for (int i = 0; i < floors.Count; i++)
            floors[i].SetActive(true);
        // Draw the walls
        for (int i = 0; i < walls.Count; i++)
            walls[i].SetActive(true);
        // Draw the buildings
        for (int i = 0; i < buildings.Count; i++)
            buildings[i].SetActive(true);
        // Draw the people
        for (int i = 0; i < npcs.Count; i++) {
            npcs[i].SetActive(true);
            npcs[i].GetComponent<NPC>().draw();
        }
    }

    // Removes the tile from the screen
    public void Undraw() {
        // Undraw the floor
        for (int i = 0; i < floors.Count; i++)
            floors[i].SetActive(false);
        // Undraw the walls
        for (int i = 0; i < walls.Count; i++)
            walls[i].SetActive(false);
        // Undraw the buildings
        for (int i = 0; i < buildings.Count; i++)
            buildings[i].SetActive(false);
        // Undraw the people
        for (int i = 0; i < npcs.Count; i++) {
            npcs[i].SetActive(false);
            npcs[i].GetComponent<NPC>().undraw();
        }
    }

    // Finds and returns a random empty location on this tile, if there is one
    public Vector3 EmptyLocation() {
        Vector3 emptyLoc;

        if (grid.count() == 0)
            emptyLoc = new Vector3(-1, -1, -1);
        else {
            int index = Random.Range(0, grid.count());
            emptyLoc = grid.gridPositions[index];
            grid.removeAt(index);
        }
        return emptyLoc;
    }

    // Is there an impassable object at this location?
    public Boolean ObjectAt(int x, int y) {
        // There's a building here
        if (grid.findTile(buildingLocations, x, y) != -1)
            return true;
        // There's a wall here
        if (grid.findTile(wallLocations, x, y) != -1)
            return true;

        // There was nothing there
        return false;
    }
}