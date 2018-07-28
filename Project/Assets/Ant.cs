using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    /// <summary>
    /// The base ant class
    /// </summary>
    public abstract class Ant : MonoBehaviour
    {
        /// <summary>
        /// The X coordinate of the ant
        /// </summary>
        public int X;

        /// <summary>
        /// The Z coordinate of the ant
        /// </summary>
        public int Z;

        /// <summary>
        /// If the ant has been instantiated or not
        /// </summary>
        public bool Instantiated = false;

        /// <summary>
        /// If the ant is currently carrying a voxel
        /// </summary>
        public bool voxelCarried = false;

        /// <summary>
        /// The current direction the ant is facing
        /// </summary>
        public Coordinates facing = Coordinates.North;

        /// <summary>
        /// The current health of the ant
        /// </summary>
        public int health;
    }

    /// <summary>
    /// The worker ant type
    /// </summary>
    public class WorkerAnt : Ant
    {
        /// <summary>
        /// Controls the ants behaviour each tick
        /// </summary>
        public void FixedUpdate()
        {
            if (Instantiated)
            {
                Environment.Instance.Move(this);

                /*
                 * 
                 * This is where your code goes
                 * 
                 */

            }
        }
    }

    //The scavenger ant type
    public class ScavengerAnt : Ant
    {
        /// <summary>
        /// Controls the ants behaviour each tick
        /// </summary>
        public void FixedUpdate()
        {
            if (Instantiated)
            {
                Environment.Instance.Move(this);

                /*
                 * 
                 * This is where your code goes
                 * 
                 */

            }
        }
    }



}
