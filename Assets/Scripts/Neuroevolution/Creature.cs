﻿using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Mathf = UnityEngine.Mathf;

namespace Assets.Scripts.Neuroevolution
{
    public class Creature
    {
        //External
        private readonly int rotationNode;
        private readonly List<Matrix> synapses;
        public readonly CreatureStruct Save;
        public readonly int Generation;
        public readonly int Genome;
        public readonly int Species;
        public readonly int Parent;

        //Internal
        private readonly World world;
        private readonly Body ground;
        private readonly List<RevoluteJoint> revoluteJoints;

        private readonly float initialRotation;
        private readonly bool useRotation;

        private Matrix neuralNetwork;
        private bool isDead;
        private int count;

        //Stats
        private float energy;
        private float time;

        //Globals
        private float currentFriction;
        private float maxTorque;
        private float currentRestitution;


        private static int genomeCount;
        public static int GenomeCount
        {
            get
            {
                genomeCount++;
                return genomeCount - 1;
            }
        }

        private static int speciesCount;
        public static int SpeciesCount
        {
            get
            {
                speciesCount++;
                return speciesCount - 1;
            }
        }



        public Creature(CreatureStruct creature, int generation, int genome, int species, int parent)
        {
            Genome = genome;
            Species = species;
            Parent = parent;
            Generation = generation;
            Save = creature;
            rotationNode = creature.RotationNode;
            synapses = creature.Synapses;
            revoluteJoints = new List<RevoluteJoint>();
            world = new World(new Vector2(0, Globals.WorldYGravity));

            //Add nodes
            foreach (var p in creature.Positions)
            {
                var body = BodyFactory.CreateCircle(world, 0.5f, Globals.BodyDensity, p, null);
                body.CollidesWith = Category.Cat1;
                body.CollisionCategories = Category.Cat2;
                body.IsStatic = false;
                body.Friction = Globals.BodyFriction;
                body.Restitution = Globals.Restitution;
                world.AddBody(body);
            }
            currentFriction = Globals.BodyFriction;
            maxTorque = Globals.MaxMotorTorque;
            currentFriction = Globals.Restitution;
            //Add ground
            ground = BodyFactory.CreateRectangle(world, 1000000, 1, 1, Vector2.Zero, null);
            ground.IsStatic = true;
            ground.CollisionCategories = Category.Cat1;
            ground.CollidesWith = Category.Cat2;
            ground.Rotation = Globals.GroundRotation;
            world.AddBody(ground);
            //Compute bodies
            world.ProcessChanges();

            //Add joints
            foreach (var r in creature.RevoluteJoints)
            {
                var anchorA = world.BodyList[r.anchor].Position - world.BodyList[r.a].Position;
                var anchorB = world.BodyList[r.anchor].Position - world.BodyList[r.b].Position;
                var j = JointFactory.CreateRevoluteJoint(world, world.BodyList[r.a], world.BodyList[r.b], anchorA, anchorB, false);
                j.LimitEnabled = true;
                j.SetLimits(-r.lowerLimit, r.upperLimit);
                j.Enabled = true;
                j.MotorEnabled = true;
                j.MaxMotorTorque = Globals.MaxMotorTorque;
                world.AddJoint(j);
                revoluteJoints.Add(j);
            }
            foreach (var d in creature.DistanceJoints)
            {
                world.AddJoint(JointFactory.CreateDistanceJoint(world, world.BodyList[d.a], world.BodyList[d.b], Vector2.Zero, Vector2.Zero));
            }
            world.ProcessChanges();

            //Rotation node
            if (rotationNode != -1)
            {
                useRotation = true;
                initialRotation = world.BodyList[rotationNode].Rotation;
            }
        }



        public Creature GetChild(float variation)
        {
            var synapses = new List<Matrix>();
            for (var k = 0; k < this.synapses.Count; k++)
            {
                synapses.Add(this.synapses[k] + Matrix.Random(this.synapses[k].M, this.synapses[k].N) * variation);
            }
            var c = Save;
            c.Synapses = synapses;
            return new Creature(c, Generation + 1, GenomeCount, Species, Genome);
        }

        public Creature GetCopy()
        {
            return new Creature(Save, Generation, Genome, Species, Parent);
        }

        public Creature GetRandomClone()
        {
            return CreatureFactory.CreateCreature(Save, synapses[0].N, synapses.Count - 1);
        }



        public void Update(float dt)
        {
            if (!isDead)
            {
                world.Step(dt);
                time += dt;
                count++;
                if (Globals.Debug)
                {
                    foreach (var r in revoluteJoints)
                    {
                        var x = Globals.DeltaTime * Globals.MotorTorque * ((time % Globals.CycleDuration > Globals.CycleDuration / 2) ? 1 : -1);
                        energy += Mathf.Abs(r.MotorSpeed - x);
                        r.MotorSpeed = x;
                    }
                }
                else
                {
                    if (count >= Globals.TrainCycle)
                    {
                        count = 0;
                        Train();
                    }
                    //Change speeds
                    for (var i = 0; i < revoluteJoints.Count; i++)
                    {
                        var x = Globals.DeltaTime * neuralNetwork[0][i] * Globals.MotorTorque;
                        energy += Mathf.Abs(revoluteJoints[i].MotorSpeed - x);
                        revoluteJoints[i].MotorSpeed = x;
                    }
                }
            }

            if ((world.BodyList[0].Position.Y > Globals.MaxYPosition) || (useRotation && Globals.KillFallen && GetAngle() > Globals.MaxAngle))
            {
                isDead = true;
            }
            //Globals update
            if (currentFriction != Globals.BodyFriction)
            {
                foreach (var b in world.BodyList)
                {
                    b.Friction = Globals.BodyFriction;
                }
                currentFriction = Globals.BodyFriction;
            }
            if (currentRestitution != Globals.Restitution)
            {
                foreach (var b in world.BodyList)
                {
                    b.Restitution = Globals.Restitution;
                }
                currentRestitution = Globals.Restitution;
            }
            if (ground.Rotation != Globals.GroundRotation)
            {
                ground.Rotation = Globals.GroundRotation;
            }
            if (maxTorque != Globals.MaxMotorTorque)
            {
                foreach (var r in revoluteJoints)
                {
                    r.MaxMotorTorque = Globals.MaxMotorTorque;
                }
                maxTorque = Globals.MaxMotorTorque;
            }

            world.Gravity.Y = Globals.WorldYGravity;
        }


        private void Train()
        {
            neuralNetwork = new Matrix(1, 2 * revoluteJoints.Count + 1);

            //Revolute joints entries
            for (var i = 0; i < revoluteJoints.Count; i++)
            {
                var r = revoluteJoints[i];
                //Angle between -1 and 1 based on limits
                neuralNetwork[0][2 * i] = (r.JointAngle - r.LowerLimit) / (r.UpperLimit - r.LowerLimit) * 2 - 1;
                //Rotation direction
                neuralNetwork[0][2 * i + 1] = (revoluteJoints[i].MotorSpeed > 0) ? 1 : -1;
            }

            //Time entry
            neuralNetwork[0][2 * revoluteJoints.Count] = 2 * (time % Globals.CycleDuration) / Globals.CycleDuration - 1;

            //Process
            for (var k = 0; k < synapses.Count; k++)
            {
                neuralNetwork = Sigma(Matrix.Dot(neuralNetwork, synapses[k]));
            }
        }
        private static Matrix Sigma(Matrix m)
        {
            for (var x = 0; x < m.M; x++)
            {
                for (var y = 0; y < m.N; y++)
                {
                    m[x][y] = 1 / (1 + Mathf.Exp(-m[x][y])) * 2 - 1;
                }
            }
            return m;
        }



        public List<Body> GetBodies()
        {
            return world.BodyList.GetRange(0, world.BodyList.Count - 1);
        }

        public List<Joint> GetJoints()
        {
            return world.JointList;
        }

        public Vector2 GetAveragePosition()
        {
            var a = Vector2.Zero;
            foreach (var b in world.BodyList)
            {
                if (!b.IsStatic)
                {
                    a += b.Position;
                }
            }
            return a / (world.BodyList.Count - 1); //-1 : ground
        }

        public float GetFitness()
        {
            if (isDead)
            {
                return float.NegativeInfinity;
            }
            else
            {
                var x = 0f;
                if (useRotation)
                {
                    x = (GetAngle() > Globals.MaxAngle) ? 1 : 0;
                }
                return GetAveragePosition().X + x * Globals.BadAngleImpact + GetPower() * Globals.EnergyImpact;
            }
        }

        public float GetPower()
        {
            return energy / time;
        }

        public float GetEnergy()
        {
            return energy;
        }

        public float GetAngle()
        {
            if (rotationNode == -1)
            {
                return 0;
            }
            else
            {
                return Mathf.Abs(world.BodyList[rotationNode].Rotation - initialRotation);
            }
        }
    }
}
