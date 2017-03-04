using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameController : MonoBehaviour
{
    [Header("Invader Variables")]
    public GameObject cube;
    public GameObject laserGameObject;

    [Header("Team Variables")]
    public List<GameObject> team1;
    public List<GameObject> team2;
    public Vector3 team1Spawn = new Vector3(-100, 0, -100);
    public Vector3 team2Spawn = new Vector3(100, 0, 100);
    public int teamAmount = 25;

    [Header("Asteroid Field Variables")]
    public List<GameObject> asteroidPrefabs;
    public Vector3 area = Vector3.zero;
    public int minScale = 10;
    public int maxScale = 200;
    public int asteroidCount = 0;
    public int amountOfAsteroids = 100;



    /****************************************************************************************************
     * Description: This is called once at the start of the game
     * Syntax: ---
     ****************************************************************************************************/
    void Start ()
    {
		team1 = CreateTeam("spaceInvader1.txt", team1Spawn, 20, teamAmount, "Team1");
		team2 = CreateTeam("spaceInvader2.txt", team2Spawn, 20, teamAmount, "Team2");
        CreateAsteroidField();
        RenderSettings.ambientLight = Color.white;
	}


    /****************************************************************************************************
     * Description: Creates an asteroid field.
     * Syntax: CreateAsteroidField();
     ****************************************************************************************************/
    private void CreateAsteroidField()
    {
        //Choose a random location for the asteroid
        float delta = 0.1f;
        float xVelocity = Random.Range(-delta, delta);
        float yVelocity = Random.Range(-delta, delta);
        float zVelocity = Random.Range(-delta, delta);

        GameObject parent = new GameObject("Asteroid Field");
        for (int i = asteroidCount; i < amountOfAsteroids; i++, asteroidCount++)
        {
            int index = Random.Range(0, asteroidPrefabs.Count - 1);

            //Calculate random position for asteroid
            float xPos = Random.Range(-area.x, area.x);
            float yPos = Random.Range(-area.y, area.y);
            float zPos = Random.Range(-area.z, area.z);
            Vector3 finalPos = new Vector3(xPos, yPos, zPos);

            //Calculate random size for asteroid
            int size = Random.Range(minScale, maxScale);
            Vector3 finalScale = new Vector3(size, size, size);

            //Create the asteroid
            GameObject asteroid = (GameObject)Instantiate(asteroidPrefabs[index], finalPos, Quaternion.identity);
            asteroid.transform.localScale = finalScale;
            asteroid.transform.parent = parent.transform;
        }
    }


    /****************************************************************************************************
     * Description: Creates a new space invader gameObject
     * Syntax: GameObject spaceInvader = CreateSpaceInvader(textFile, Vector3);
     *                  textFile = a file containing space invader information
     *                  Vector3 = the position to spawn the space invader
     * Returns: The parent gameObject of the space invader
     ****************************************************************************************************/
    GameObject CreateSpaceInvader(string file, Vector3 spawnLocation)
    {
        // Open the text file
        FileInfo sourceFile = new FileInfo("Assets/Space Invaders/" + file);
        StreamReader reader = sourceFile.OpenText();
        GameObject right = null;
		GameObject middle = null;

        //Grab the width, height and depth of the space invader
        float width, height, depth;
        string[] token = reader.ReadLine().Split(' ');
        width = (float.Parse(token[0]) / 2) / 10;
        height = (float.Parse(token[1]) / 2) / 10;
        depth = float.Parse(token[2]);

        //Grab the color of the space invader
        string text = reader.ReadLine();
        Color color = HexToDec(text);

        //Create the space invader
        text = reader.ReadLine();

        GameObject par = new GameObject();
        par.transform.position = spawnLocation;
        par.name = "Invader";

        //Read through the file
        Vector3 pos = spawnLocation;
        float scale = cube.transform.localScale.x;
        while (text != null)
        {
            //Loop through each character of the line
            foreach (char c in text)
            {
                //Create the invader
                if (c == 'X')
                {
                    for (int i = 0; i < depth; i++)
                    {
                        GameObject part = (GameObject)Instantiate(cube, new Vector3(pos.x - width, pos.y, pos.z - height), Quaternion.identity);
                        part.transform.parent = par.transform;
                        part.transform.localScale = new Vector3(1, 1, 1) * scale;
                        part.GetComponent<MeshRenderer>().material.color = color;
                        pos += new Vector3(0, scale, 0);
                    }
                }
                //Create empty gameobjects in front for calculating purposes
                if (c == 'G')
                {
                    GameObject part = new GameObject();
                    part.transform.position = new Vector3(pos.x - width, pos.y, pos.z - height + 0.01f);
                    part.transform.parent = par.transform;
                    part.transform.localScale = new Vector3(1, 1, 1) * scale;
                    if (right == null || middle == null)
                    {
						if (right == null) {
							right = part;
							right.name = "Right";
						} else {
							middle = part;
							middle.name = "Middle";
						}
                    }
                    else
                        part.name = "Left";
                }
                pos = new Vector3((pos.x - par.transform.position.x) + par.transform.position.x, par.transform.position.y, (pos.z - par.transform.position.z) + par.transform.position.z);
                pos += new Vector3(scale, 0, 0);
            }
            text = reader.ReadLine();
            pos = new Vector3(par.transform.position.x, par.transform.position.y, ((pos.z + scale) - par.transform.position.z) + par.transform.position.z);
        }

        //Add necessary components to the invader
        Rigidbody rbody = par.AddComponent<Rigidbody>();
        rbody.useGravity = false;

        SphereCollider sphereCol = par.AddComponent<SphereCollider>();
        sphereCol.isTrigger = true;
        sphereCol.radius = 6;

		par.AddComponent<AI_Main>();
		par.AddComponent<Pursue>();
        par.AddComponent<SwarmController>();

        reader.Close();
        return par;
    }


    /****************************************************************************************************
     * Description: Converts a hexadecimal value to a Color and returns the new Color
     * Syntax: Color col = HextToDec(hex);
     *              hex = the hexadecimal value of the color
     * Returns: A Color
     ****************************************************************************************************/
    private Color HexToDec(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }


    /****************************************************************************************************
     * Description: Creates a new team(list) of space invaders
     * 
     * Syntax: List<GameObject> CreateTeam = CreateTeam(textFile, Vector3, float, int, team name);
     *                  textFile = a file containing space invader information
     *                  Vector3 = the position to spawn the space invader
     * 					float = the radius in which to spawn the space invaders around the Vector3
     * 					int = the number of space invaders on the team
     * 					team name = the team name used to tag the space invaders
     * 
     * Returns: The list of spacee invader gameObjects
     ****************************************************************************************************/
    public List<GameObject> CreateTeam(string filename, Vector3 position, float radius, int count, string team)
    {
        List<GameObject> boids = new List<GameObject>();

        GameObject temp;

        for (int i = 0; i < count; i++)
        {
            temp = CreateSpaceInvader(filename, position);
            temp.transform.position = new Vector3(temp.transform.position.x + 4 * (int)(i / 10),
                temp.transform.position.y + 2 * (int)(i / 7),
                temp.transform.position.z + 3 * (i % 4));
            temp.tag = team;
            boids.Add(temp);
        }
        return boids;
    }
}
