using Assets;
using Assets.Voxel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinate system for choosing direction ants face
/// </summary>
public enum Coordinates
{
    North, East, South, West
}

/// <summary>
/// The environment. Call these class methods to perform actions on the world
/// </summary>
public class Environment : MonoBehaviour
{

    #region Fields

    /// <summary>
    /// The instance of the environment
    /// </summary>
    public static Environment Instance;

    /// <summary>
    /// The width of the environemnt
    /// </summary>
    public int xWidth;

    /// <summary>
    /// The depth of the environment
    /// </summary>
    public int zWidth;

    /// <summary>
    /// The probability of any piece being a food piece
    /// </summary>
    public float FoodProbability;

    /// <summary>
    /// The number of worker ants
    /// </summary>
    public int NumberOfWorkerAnts;

    /// <summary>
    /// The number of scavenger ants
    /// </summary>
    public int NumberOfScavengerAnts;

    /// <summary>
    /// The number of nests generated
    /// </summary>
    public int numNestVoxels;

    /// <summary>
    /// The amount of food each food voxel starts with
    /// </summary>
    public int FoodAmount;

    /// <summary>
    /// The maximum amount of food the nest can have
    /// </summary>
    public int NestAmount;

    /// <summary>
    /// The starting health of the ants
    /// </summary>
    public int AntStartingHealth;

    /// <summary>
    /// The amount of food to be generated each second by each food block
    /// </summary>
    public int FoodIncreasePerTick = 5;

    /// <summary>
    /// The seed for this simulation
    /// </summary>
    public int seed;

    /// <summary>
    /// The parent object for all ants
    /// </summary>
    [HideInInspector]
    public GameObject antParent;

    /// <summary>
    /// The random number generator used
    /// </summary>
    [HideInInspector]
    public System.Random rng;

    /// <summary>
    /// The voxels of this simulation
    /// </summary>
    private Voxel[,] Field;

    #endregion

    #region Start Method

    /// <summary>
    /// On initialization
    /// </summary>
    void Awake()
    {
        //Create the ant parent
        antParent = new GameObject("Ants");
        //Set the Instance
        if (Instance == null)
            Instance = this;
        else
        {
            if (!ReferenceEquals(Instance, this))
            {
                DestroyImmediate(this);
            }
        }
        //Instantiate the field
        Field = new Voxel[xWidth, zWidth];
        //Instantiate rng
        rng = new System.Random(seed);

        //Create the voxels
        #region Voxel Creation

        //Instantiate all voxels
        for (int i = 0; i < xWidth; i++)
        {
            for (int j = 0; j < zWidth; j++)
            {
                GameObject g = new GameObject();
                Voxel v = null;
                if (rng.NextDouble() <= FoodProbability)
                    v = g.AddComponent<FoodVoxel>();
                else
                    v = g.AddComponent<DirtVoxel>();
                //Set the voxel coords
                v.X = Convert.ToInt32(i);
                v.Y = rng.Next(-2, 2);
                v.Z = Convert.ToInt32(j);
                //Set the game object position
                g.transform.position = new Vector3(i, v.Y, j);
                //Set the parent
                g.transform.parent = transform;
                Field[i, j] = v;
            }
        }

        #endregion

        //Set our initial neighbours
        #region Neighbour Setting

        //Set all neighbours
        for (int EW = 0; EW < xWidth; EW++)
        {
            for (int NS = 0; NS < zWidth; NS++)
            {
                Voxel v = Field[EW, NS];
                if (NS + 1 < zWidth)
                {
                    v.N = Field[EW, NS + 1];
                    Field[EW, NS + 1].S = v;
                }
                if (EW + 1 < xWidth)
                {
                    v.E = Field[EW + 1, NS];
                    Field[EW + 1, NS].W = v;
                }
                v.Init();
            }
        }

        #endregion

        //Set nest voxels
        #region Nest creation

        //For the desired number of nests
        for (int i = 0; i < numNestVoxels; i++)
        {
            //Get a random spot
            int xIndex = rng.Next(0, xWidth);
            int zIndex = rng.Next(0, zWidth);
            //Delete the dirt voxel from it
            GameObject obj = Field[xIndex, zIndex].gameObject;
            Voxel old = obj.GetComponent<Voxel>();
            //If it is already a nest, skip
            if (old.tag == "Nest")
            {
                i--;
                continue;
            }
            //Grab its height so we can set the  new voxel to be properly placed
            int yIndex = old.Y;
            GameObject.Destroy(old);
            NestVoxel nest = obj.AddComponent<NestVoxel>();
            //Put new voxel in our field
            Field[xIndex, zIndex] = nest;
            //Set nest coordinates
            nest.X = xIndex;
            nest.Y = yIndex;
            nest.Z = zIndex;
            #region Neighbour setting
            if (zIndex + 1 < zWidth)
            {
                nest.N = Field[xIndex, zIndex + 1];
                Field[xIndex, zIndex + 1].S = nest;
            }
            if (xIndex + 1 < xWidth)
            {
                nest.E = Field[xIndex + 1, zIndex];
                Field[xIndex + 1, zIndex].W = nest;
            }
            if (zIndex - 1 >= 0)
            {
                nest.S = Field[xIndex, zIndex - 1];
                Field[xIndex, zIndex - 1].N = nest;
            }
            if (xIndex - 1 >= 0)
            {
                nest.W = Field[xIndex - 1, zIndex];
                Field[xIndex - 1, zIndex].E = nest;
            }
            #endregion
            //Init the mesh
            nest.Init();
        }

        #endregion

        //Instantiate all the ants
        #region Instantiate Ants

        //Instantiate AScavenger ants
        #region Scavenger
        //For the desired number of scavenger ants
        for (int i = 0; i < NumberOfScavengerAnts; i++)
        {
            //Grab random coord
            int xIndex = rng.Next(0, xWidth);
            int zIndex = rng.Next(0, zWidth);
            //Get gameobject location
            Voxel v = Field[xIndex, zIndex];
            GameObject ant = Instantiate(Resources.Load<GameObject>("Models/Ant Prefab"));
            //Place ant transform
            ant.transform.position = new Vector3(v.X, v.Y - 0.5f, v.Z);
            ant.transform.parent = antParent.transform;
            //Add script
            Ant antScript = ant.AddComponent<ScavengerAnt>();
            v.antsOnThisVoxel.Add(antScript);
            //Set ant vars
            antScript.X = v.X;
            antScript.Z = v.Z;
            antScript.health = AntStartingHealth;
            //Set to be instantiated
            antScript.Instantiated = true;
        }

        #endregion

        //Instantiate worker Ants
        #region Worker

        //For desired number of worker ants
        for (int i = 0; i < NumberOfWorkerAnts; i++)
        {
            //Grab random coord
            int xIndex = rng.Next(0, xWidth);
            int zIndex = rng.Next(0, zWidth);
            //Get gameobject location
            Voxel v = Field[xIndex, zIndex];
            GameObject ant = Instantiate(Resources.Load<GameObject>("Models/Ant Prefab"));
            //Place ant transform
            ant.transform.position = new Vector3(v.X, v.Y - 0.5f, v.Z);
            ant.transform.parent = antParent.transform;
            //Add script
            Ant antScript = ant.AddComponent<WorkerAnt>();
            v.antsOnThisVoxel.Add(antScript);
            //Set ant vars
            antScript.X = v.X;
            antScript.Z = v.Z;
            antScript.health = AntStartingHealth;
            //Set to be instantiated
            antScript.Instantiated = true;
        }

        #endregion

        #endregion

    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Causes an ant to dig the block in front of it, removing it from the field. Can only dig blocks within a height difference of 1 from the ant
    /// </summary>
    /// <param name="ant"></param>
    /// <returns>Returns true if the dig was sucessful, false otherwise</returns>
    public bool Dig(Ant ant)
    {
        //First reduce our health
        if (!ReduceHealth(ant))
            return false;
        //If we are already carrying a voxel
        if (ant.voxelCarried)
        {
            Debug.Log("Ants cannot carry more than one block at a time");
            return false;
        }
        //Get the coordinates of where we are, and where we are trying to dig
        Voxel from;
        Voxel to;
        GetVoxelFacing(ant, out from, out to);
        //If we are unable to dig
        if (to == null)
        {
            Debug.Log("Cannot dig outside of the board");
            return false;
        }
        //If the height difference is 1
        if (!IsValidMove(from, to))
        {
            Debug.Log("Cannot dig more than one block height");
            return false;
        }
        //get old coord
        int yIndex = to.Y - 1;
        int xIndex = to.X;
        int zIndex = to.Z;
        //Create new dirt voxel
        DirtVoxel dirt = to.gameObject.AddComponent<DirtVoxel>();
        //Delete old voxel
        GameObject.Destroy(to);
        //place it in our field
        Field[xIndex, yIndex] = dirt;
        //set new voxel coords
        dirt.X = xIndex;
        dirt.Y = yIndex;
        dirt.Z = zIndex;
        //Set new neighbours
        if (dirt.Z + 1 < zWidth)
        {
            dirt.N = Field[dirt.X, dirt.Z + 1];
            Field[dirt.X, dirt.Z + 1].S = dirt;
        }
        if (dirt.X + 1 < xWidth)
        {
            dirt.E = Field[dirt.X + 1, dirt.Z];
            Field[dirt.X + 1, dirt.Z].W = dirt;
        }
        if (dirt.Z - 1 >= 0)
        {
            dirt.S = Field[dirt.X, dirt.Z - 1];
            Field[dirt.X, dirt.Z - 1].N = dirt;
        }
        if (dirt.X - 1 >= 0)
        {
            dirt.W = Field[dirt.X - 1, dirt.Z];
            Field[dirt.X - 1, dirt.Z].E = dirt;
        }
        //init mesh creation
        dirt.Init();
        return true;
    }

    /// <summary>
    /// Causes an ant to attempt to eat from the block it is currently on. This will restore its health to full
    /// unless the block contians less food than the amount trying to be eaten. If eat causes the block food to hit zero,
    /// the block will turn to dirt
    /// </summary>
    /// <param name="ant"></param>
    public void Eat(Ant ant)
    {
        //First reduce our health
        if (!ReduceHealth(ant))
            return;

        //Get the type of the voxel
        Type atType = Field[ant.X, ant.Z].GetType();
        //If not a food voxel
        if (atType != new FoodVoxel().GetType())
        {
            Debug.Log("Ants cannot eat dirt or nest");
            return;
        }
        //else, decrement resource
        FoodVoxel vox = Field[ant.X, ant.Z] as FoodVoxel;
        if (vox.Resource > (AntStartingHealth - ant.health))
        {
            vox.Resource -= (AntStartingHealth - ant.health);
            ant.health += (AntStartingHealth - ant.health);
        }
        //If we have diminished ALL the food
        else
        {
            ant.health += vox.Resource;
            vox.Resource = 0;

            //Turn food source to dirt
            if (vox.Resource <= 0)
            {
                //get old coord
                int yIndex = vox.Y - 1;
                int xIndex = vox.X;
                int zIndex = vox.Z;
                //Create new dirt voxel
                DirtVoxel dirt = vox.gameObject.AddComponent<DirtVoxel>();
                //Delete old voxel
                GameObject.Destroy(vox);
                //place it in our field
                Field[xIndex, yIndex] = dirt;
                //set new voxel coords
                dirt.X = xIndex;
                dirt.Y = yIndex;
                dirt.Z = zIndex;
                //Set new neighbours
                if (dirt.Z + 1 < zWidth)
                {
                    dirt.N = Field[dirt.X, dirt.Z + 1];
                    Field[dirt.X, dirt.Z + 1].S = dirt;
                }
                if (dirt.X + 1 < xWidth)
                {
                    dirt.E = Field[dirt.X + 1, dirt.Z];
                    Field[dirt.X + 1, dirt.Z].W = dirt;
                }
                if (dirt.Z - 1 >= 0)
                {
                    dirt.S = Field[dirt.X, dirt.Z - 1];
                    Field[dirt.X, dirt.Z - 1].N = dirt;
                }
                if (dirt.X - 1 >= 0)
                {
                    dirt.W = Field[dirt.X - 1, dirt.Z];
                    Field[dirt.X - 1, dirt.Z].E = dirt;
                }
                //init mesh creation
                dirt.Init();
            }
        }

    }

    /// <summary>
    /// Exchanges health from an ant to the nest. Will only work if the ant is a scavenger, and only if it is standing upon a nest block
    /// </summary>
    /// <param name="ant"></param>
    /// <param name="amount">the amount of food to give (this reduces the health of the ant by the same amount)</param>
    public void Feed(Ant ant, int amount)
    {
        Type antType = ant.GetType();
        //If we are not a scavenger
        if (antType != new ScavengerAnt().GetType())
        {
            Debug.Log("Only scavenger ants can give food to the nest");
            return;
        }
        //Check type of current block
        Type atType = Field[ant.X, ant.Z].GetType();
        //If not a nest block
        if (atType != new NestVoxel().GetType())
        {
            Debug.Log("Cannot give food to a non-nest block");
            return;
        }
        //Clamp the amount to be at most the difference between the max and the current plus our amount
        amount = Mathf.Clamp(amount, 0, NestVoxel.MaxFood - (NestVoxel.Food + amount));
        //Else exchange resource
        NestVoxel.Food += amount;
        ant.health -= amount;
        //Reduce our health
        if (!ReduceHealth(ant))
            return;
    }

    /// <summary>
    /// Causes an ant to place a block ontop of the block in front of it. can only place blocks within a height difference of 1 from the ant
    /// </summary>
    /// <param name="ant"></param>
    /// <returns>true if the placement was sucessful, false otherwise</returns>
    public bool Place(Ant ant)
    {
        //First reduce our health
        if (!ReduceHealth(ant))
            return false;
        if (ant == null)
        {
            Debug.Log("An ant has died");
            return false;
        }
        //Check that the ant is carrying a voxel
        if (!ant.voxelCarried)
        {
            Debug.Log("Cannot place a block if no block is carried");
            return false;
        }
        Voxel from;
        Voxel to;
        //If we're on the board
        GetVoxelFacing(ant, out from, out to);
        if (to == null)
        {
            Debug.Log("Cannot place block off of the board");
            return false;
        }
        //And the height difference is only one
        if (!IsValidMove(from, to))
        {
            Debug.Log("Cannot place block more than one block height");
            return false;
        }
        //Place the voxel
        PlaceVoxel(to);
        //
        ant.voxelCarried = false;
        return true;
    }

    /// <summary>
    /// Will move an ant forward one block. Ants are only able to move onto a block with a max height difference of 1 from their own
    /// </summary>
    /// <param name="ant"></param>
    public void Move(Ant ant)
    {
        if (ant == null)
        {
            Debug.Log("An ant has died");
            return;
        }
        //Get voxel from and to
        Voxel from;
        Voxel to;
        GetVoxelFacing(ant, out from, out to);
        //If moving off board
        if (to == null)
        {
            Debug.Log("Cannot move off of the board");
        }
        //If height difference too great
        if (!IsValidMove(from, to))
        {
            Debug.Log("Cannot climb more than one block height");
            return;
        }
        //Alter ant transform to be on new coordinate
        ant.transform.position = to.transform.position - new Vector3(0, 0.5f, 0);
        //Alter internal coords
        ant.X = to.X;
        ant.Z = to.Z;
        from.antsOnThisVoxel.Remove(ant);
        to.antsOnThisVoxel.Add(ant);

    }

    /// <summary>
    /// Returns the current block the ant is standing on. This is the only method that does not reduce the health of an ant
    /// </summary>
    /// <param name="ant"></param>
    /// <returns></returns>
    public Voxel GetCurrentBlock(Ant ant)
    {
        return Field[ant.X, ant.Z];
    }

    /// <summary>
    /// returns a list of all other ants on this current voxel
    /// </summary>
    /// <param name="ant"></param>
    /// <returns></returns>
    public List<Ant> Others(Ant ant)
    {
        return Field[ant.X, ant.Z].antsOnThisVoxel;
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Checks if the move an ant is trying to make is valid
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private bool IsValidMove(Voxel from, Voxel to)
    {
        if (to == null)
        {
            return false;
        }
        if (Math.Abs(from.Y - to.Y) < 2)
            return true;
        return false;
    }

    /// <summary>
    /// Returns the current voxel an ant is on, as well as the voxel of the direction the ant is facing
    /// </summary>
    /// <param name="ant"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    private void GetVoxelFacing(Ant ant, out Voxel from, out Voxel to)
    {
        from = Field[ant.X, ant.Z];
        to = null;
        switch (ant.facing)
        {
            case Coordinates.North:
                to = from.N;
                break;
            case Coordinates.East:
                to = from.E;
                break;
            case Coordinates.South:
                to = from.S;
                break;
            case Coordinates.West:
                to = from.W;
                break;
            default:
                break;
        }
        return;
    }

    /// <summary>
    /// Reduces the current ants health by one, and kills the ant if it goes lower than 1
    /// </summary>
    /// <param name="ant"></param>
    private bool ReduceHealth(Ant ant)
    {
        ant.health--;
        if (ant.health <= 0)
        {
            Debug.Log("An ant has died");
            GameObject.Destroy(ant.gameObject);
            Type antType = ant.GetType();
            if (antType == new WorkerAnt().GetType())
                NumberOfWorkerAnts--;
            else
                NumberOfScavengerAnts--;

            Field[ant.X, ant.Z].antsOnThisVoxel.Remove(ant);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Places voxel on the ground. Checks to see if the voxel should be a nest or not, and places the correct corresponding voxel type
    /// </summary>
    /// <param name="to"></param>
    private void PlaceVoxel(Voxel to)
    {
        //The block to place should be a nest or not
        bool isNest = false;

        //Check surrounding voxels for a nest voxel
        #region check if we place a nest
        if (to.N != null)
        {
            Type north = to.N.GetType();
            if (north == new NestVoxel().GetType())
                isNest = true;
        }
        if (to.E != null)
        {
            Type east = to.E.GetType();
            if (east == new NestVoxel().GetType())
                isNest = true;
        }
        if (to.S != null)
        {
            Type south = to.S.GetType();
            if (south == new NestVoxel().GetType())
                isNest = true;
        }
        if (to.W != null)
        {
            Type west = to.W.GetType();
            if (west == new NestVoxel().GetType())
                isNest = true;
        }
        #endregion

        //Get old coordinates
        GameObject obj = to.gameObject;
        int yIndex = to.Y + 1;
        int xIndex = to.X;
        int zIndex = to.Z;
        GameObject.Destroy(to);
        Voxel toAdd;
        if (isNest)
        {
            //Create new nest voxel
            toAdd = obj.AddComponent<NestVoxel>();
            //increment num of nest
            numNestVoxels++;
        }
        else
        {
            //Create new dirt voxelS
            toAdd = obj.AddComponent<DirtVoxel>();
        }
        //set voxel on field
        Field[xIndex, zIndex] = toAdd;
        //set voxel coordinates
        toAdd.X = xIndex;
        toAdd.Y = yIndex;
        toAdd.Z = zIndex;
        //Set voxel neighbours
        if (toAdd.Z + 1 < zWidth)
        {
            toAdd.N = Field[toAdd.X, toAdd.Z + 1];
            Field[toAdd.X, toAdd.Z + 1].S = toAdd;
        }
        if (toAdd.X + 1 < xWidth)
        {
            toAdd.E = Field[toAdd.X + 1, toAdd.Z];
            Field[toAdd.X + 1, toAdd.Z].W = toAdd;
        }
        if (toAdd.Z - 1 >= 0)
        {
            toAdd.S = Field[toAdd.X, toAdd.Z - 1];
            Field[toAdd.X, toAdd.Z - 1].N = toAdd;
        }
        if (toAdd.X - 1 >= 0)
        {
            toAdd.W = Field[toAdd.X - 1, toAdd.Z];
            Field[toAdd.X - 1, toAdd.Z].E = toAdd;
        }
        //init mesh of voxel
        toAdd.Init();
    }

    #endregion

}
