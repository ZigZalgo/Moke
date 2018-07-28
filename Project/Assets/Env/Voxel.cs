using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Voxel
{
    /// <summary>
    /// Base voxel class which contains methods for how each voxel works
    /// </summary>
    public abstract class Voxel : MonoBehaviour, IEquatable<Voxel>
    {
        public List<Ant> antsOnThisVoxel;
        /// <summary>
        /// The North, east, south, and west neighbours of this voxel
        /// </summary>
        public Voxel N, E, S, W;

        /// <summary>
        /// The x, y, and z coordinates of this voxel
        /// </summary>
        public int X, Y, Z;

        /// <summary>
        /// How to compare a voxel to another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Voxel other)
        {
            if (other == null)
                return false;
            if (X == other.X && Y == other.Y && Z == other.Z)
                return true;
            return false;
        }

        /// <summary>
        /// Creates the mesh, and material for this voxel
        /// </summary>
        public virtual void Init()
        {
            antsOnThisVoxel = new List<Ant>();
            Mesh m = CreateMesh();
            gameObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = m;
        }

        /// <summary>
        /// Creates a mesh for this game object based on our neighbours
        /// </summary>
        /// <returns></returns>
        public Mesh CreateMesh()
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            #region Vector Instantiation
            Vector3 v0 = new Vector3(-0.5f, 0, -0.5f);
            Vector3 v1 = new Vector3(-0.5f, 0, 0.5f);
            Vector3 v4 = new Vector3(0.5f, 0, -0.5f);
            Vector3 v5 = new Vector3(0.5f, 0, 0.5f);
            #endregion

            //Add the floor
            verts.AddRange(new List<Vector3>() { v0, v1, v4, v5 });
            tris.AddRange(new List<int>() { 0, 1, 2, 2, 1, 3 });

            //If a neghbour is higher than us, we need to add a wall reaching up to them
            #region Neighbour Checking

            //Check north Neighbour
            if (N != null && Y > N.Y)
            {
                Vector3 v3 = new Vector3(-0.5f, N.Y - Y, 0.5f);
                Vector3 v7 = new Vector3(0.5f, N.Y - Y, 0.5f);
                verts.Add(v3); verts.Add(v7);
                tris.Add(verts.IndexOf(v3)); tris.Add(verts.IndexOf(v7)); tris.Add(verts.IndexOf(v1));
                tris.Add(verts.IndexOf(v7)); tris.Add(verts.IndexOf(v5)); tris.Add(verts.IndexOf(v1));
            }
            //Check east Neighbour
            if (E != null && Y > E.Y)
            {
                Vector3 v6 = new Vector3(0.5f, E.Y - Y, -0.5f);
                Vector3 v7 = new Vector3(0.5f, E.Y - Y, 0.5f);
                verts.Add(v6); verts.Add(v7);
                tris.Add(verts.IndexOf(v5)); tris.Add(verts.IndexOf(v6)); tris.Add(verts.IndexOf(v4));
                tris.Add(verts.IndexOf(v7)); tris.Add(verts.IndexOf(v6)); tris.Add(verts.IndexOf(v5));
            }
            //Check south Neighbour
            if (S != null && Y > S.Y)
            {
                Vector3 v2 = new Vector3(-0.5f, S.Y - Y, -0.5f);
                Vector3 v6 = new Vector3(0.5f, S.Y - Y, -0.5f);
                verts.Add(v2); verts.Add(v6);
                tris.Add(verts.IndexOf(v4)); tris.Add(verts.IndexOf(v2)); tris.Add(verts.IndexOf(v0));
                tris.Add(verts.IndexOf(v6)); tris.Add(verts.IndexOf(v2)); tris.Add(verts.IndexOf(v4));
            }
            if (W != null && Y > W.Y)
            {
                Vector3 v2 = new Vector3(-0.5f, W.Y - Y, -0.5f);
                Vector3 v3 = new Vector3(-0.5f, W.Y - Y, 0.5f);
                verts.Add(v2); verts.Add(v3);
                tris.Add(verts.IndexOf(v2)); tris.Add(verts.IndexOf(v3)); tris.Add(verts.IndexOf(v1));
                tris.Add(verts.IndexOf(v0)); tris.Add(verts.IndexOf(v2)); tris.Add(verts.IndexOf(v1));
            }

            #endregion

            Mesh retVal = new Mesh
            {
                vertices = verts.ToArray(),
                triangles = tris.ToArray()
            };

            retVal = ClearBlanks(retVal);
            retVal.RecalculateNormals();
            return retVal;

        }

        /// <summary>
        /// Removes all unused vertices from a mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private Mesh ClearBlanks(Mesh mesh)
        {
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            List<Vector3> vertList = vertices.ToList();
            List<int> trianglesList = triangles.ToList();

            int testVertex = 0;

            while (testVertex < vertList.Count)
            {
                if (trianglesList.Contains(testVertex))
                {
                    testVertex++;
                }
                else
                {
                    vertList.RemoveAt(testVertex);

                    for (int i = 0; i < trianglesList.Count; i++)
                    {
                        if (trianglesList[i] > testVertex)
                            trianglesList[i]--;
                    }
                }
            }

            triangles = trianglesList.ToArray();
            vertices = vertList.ToArray();

            mesh.triangles = triangles;
            mesh.vertices = vertices;
            return mesh;
        }
    }

    /// <summary>
    /// An inherited voxel type dirt
    /// </summary>
    public class DirtVoxel : Voxel
    {
        /// <summary>
        /// Inits this voxel to be brown
        /// </summary>
        public override void Init()
        {
            antsOnThisVoxel = new List<Ant>();
            gameObject.tag = "Dirt";
            Mesh m = CreateMesh();
            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            if (filter == null)
                filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer render = gameObject.GetComponent<MeshRenderer>();
            if (render == null)
                render = gameObject.AddComponent<MeshRenderer>();

            render.material = new Material(Shader.Find("Standard"))
            {
                color = new Color(139 / 255f, 69 / 255f, 19 / 255f)
            };
            render.material.SetFloat("_Glossiness", 0f);
            filter.mesh = m;
        }
    }

    /// <summary>
    /// An inherited voxel type nest
    /// </summary>
    public class NestVoxel : Voxel
    {
        /// <summary>
        /// Current food found in the nest
        /// </summary>
        public static int Food = 0;

        /// <summary>
        /// Maximum food the nest can hold
        /// </summary>
        public static int MaxFood = 10000;

        /// <summary>
        /// Inits the nest to be red
        /// </summary>
        public override void Init()
        {
            antsOnThisVoxel = new List<Ant>();
            MaxFood = Environment.Instance.NestAmount;
            gameObject.tag = "Nest";
            Mesh m = CreateMesh();
            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            if (filter == null)
                filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer render = gameObject.GetComponent<MeshRenderer>();
            if (render == null)
                render = gameObject.AddComponent<MeshRenderer>();

            render.material = new Material(Shader.Find("Standard"))
            {
                color = Color.red
            };
            filter.mesh = m;
        }

        /// <summary>
        /// Causes ants to spawn at a nest locaton with some frequency
        /// </summary>
        private void FixedUpdate()
        {
            //the probability is equal to our percentage of food to max food, divided equally among all nest voxels
            float prob = (Food / MaxFood) / Environment.Instance.numNestVoxels;
            //If we have decided to produce an ant
            if(Environment.Instance.rng.NextDouble() <= prob)
            {
                //Create the prefab
                GameObject ant = Instantiate(Resources.Load<GameObject>("Models/Ant Prefab"));
                ant.transform.position = new Vector3(X, Y - 0.5f, Z);
                ant.transform.parent = Environment.Instance.antParent.transform;
                
                //And attach either a worker, or scavenger script with equal probability
                if(Environment.Instance.rng.Next(0, 2) % 2 == 0)
                {
                    Ant antScript = ant.AddComponent<WorkerAnt>();
                    antScript.X = X;
                    antScript.Z = Y;
                    Environment.Instance.NumberOfWorkerAnts++;
                    antScript.Instantiated = true;
                }
                else
                {
                    Ant antScript = ant.AddComponent<ScavengerAnt>();
                    antScript.X = X;
                    antScript.Z = Y;
                    Environment.Instance.NumberOfScavengerAnts++;
                    antScript.Instantiated = true;
                }
                //Spawning an ant will reduce our food by 1/10th
                Food -= (int)(MaxFood * 0.1f);
                //Clamp our food
                Mathf.Clamp(Food, 0, MaxFood);

            }
        }

    }

    /// <summary>
    /// An inherited voxel type food
    /// </summary>
    public class FoodVoxel : Voxel
    {
        /// <summary>
        /// The amount of food available on this food voxel
        /// </summary>
        public int Resource;

        /// <summary>
        /// Inits the voxel to be green
        /// </summary>
        public override void Init()
        {
            antsOnThisVoxel = new List<Ant>();
            Resource = Environment.Instance.FoodAmount;
            gameObject.tag = "Food";
            Mesh m = CreateMesh();
            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            if (filter == null)
                filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer render = gameObject.GetComponent<MeshRenderer>();
            if (render == null)
                render = gameObject.AddComponent<MeshRenderer>();

            render.material = new Material(Shader.Find("Standard"))
            {
                color = Color.green
            };
            filter.mesh = m;
        }

        /// <summary>
        /// Increase the amount of food per tick
        /// </summary>
        private void FixedUpdate()
        {
            Resource += Environment.Instance.FoodIncreasePerTick;        
        }

    }
}
