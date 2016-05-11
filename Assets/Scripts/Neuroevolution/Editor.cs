﻿using System.Collections.Generic;
using FVector2 = Microsoft.Xna.Framework.Vector2;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Neuroevolution
{
    enum EditMode { Nodes, DistanceMuscles, RotationMuscles }
    public class Editor
    {
        List<FVector2> positions;
        List<DistanceJointStruct> distanceJoints;
        List<RevoluteJointStruct> revoluteJoints;
        List<Object> objects;
        EditMode editMode;
        //Distance
        int currentMuscleNodeIndex = -1; //Index of the node which is the start of the muscle currently created
        LineRenderer currentLine;
        //Rotation
        int firstNodeIndex = -1;
        int anchorNodeIndex = -1;
        int secondNodeIndex = -1;
        GameObject firstNodeGameObject;
        GameObject secondNodeGameObject;
        GameObject anchorNodeGameObject;
        float upperLimit = 1;
        float lowerLimit = 1;
        AngleUI lowerLimitUI;
        AngleUI upperLimitUI;


        public Editor()
        {
            positions = new List<FVector2>();
            objects = new List<Object>();
            distanceJoints = new List<DistanceJointStruct>();
            revoluteJoints = new List<RevoluteJointStruct>();
            editMode = EditMode.Nodes;
            AddLine();
            lowerLimitUI = new AngleUI(Vector2.zero, 100, 3, Color.blue, true);
            upperLimitUI = new AngleUI(Vector2.zero, 100, 3f, Color.red, false);
            lowerLimitUI.SetActive(false);
            upperLimitUI.SetActive(false);
        }

        public void AddPrefabs()
        {
            AddLine();
            editMode = EditMode.RotationMuscles;
            var a = new FVector2(-10, 10);
            var b = new FVector2(0, 10);
            var c = new FVector2(0, 20);



            distanceJoints.Add(new DistanceJointStruct(0, 1));
            distanceJoints.Add(new DistanceJointStruct(2, 1));

            currentLine.SetPosition(0, ToVector2(a));
            currentLine.SetPosition(1, ToVector2(b));
            AddLine();
            currentLine.SetPosition(0, ToVector2(c));
            currentLine.SetPosition(1, ToVector2(b));
            AddLine();

            firstNodeGameObject = Object.Instantiate(Resources.Load("Circle"), ToVector2(a), Quaternion.identity) as GameObject;
            firstNodeGameObject.GetComponent<SpriteRenderer>().color = Color.green;
            firstNodeGameObject.name = positions.Count.ToString();
            firstNodeIndex = positions.Count;
            positions.Add(a);
            objects.Add(firstNodeGameObject);

            anchorNodeGameObject = Object.Instantiate(Resources.Load("Circle"), ToVector2(b), Quaternion.identity) as GameObject;
            anchorNodeGameObject.GetComponent<SpriteRenderer>().color = Color.blue;
            anchorNodeGameObject.name = positions.Count.ToString();
            anchorNodeIndex = positions.Count;
            positions.Add(b);
            objects.Add(anchorNodeGameObject);

            secondNodeGameObject = Object.Instantiate(Resources.Load("Circle"), ToVector2(c), Quaternion.identity) as GameObject;
            secondNodeGameObject.GetComponent<SpriteRenderer>().color = Color.green;
            secondNodeGameObject.name = positions.Count.ToString();
            secondNodeIndex = positions.Count;
            positions.Add(c);
            objects.Add(secondNodeGameObject);

        }
        public Vector2 ToVector2(FVector2 fvector2)
        {
            return new Vector2(fvector2.X, fvector2.Y);
        }

        void AddLine()
        {
            currentLine = (new GameObject()).AddComponent<LineRenderer>();
            currentLine.material = new Material(Shader.Find("Diffuse"));
            currentLine.material.color = Color.black;
            objects.Add(currentLine);
        }

        void EditNodes()
        {
            if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                if (hit.collider == null)
                {
                    var p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    p.z = 0;
                    var go = Object.Instantiate(Resources.Load("Circle"), p, Quaternion.identity) as GameObject;
                    go.name = positions.Count.ToString();
                    go.GetComponent<SpriteRenderer>().color = Color.white;
                    objects.Add(go);
                    positions.Add(new FVector2(p.x, p.y));
                }
            }
        }

        void EditDistanceMuscles()
        {
            //Render muscle
            if (currentMuscleNodeIndex != -1)
            {
                currentLine.enabled = true;
                var p = ToVector2(positions[currentMuscleNodeIndex]);
                var q = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                q.z = 0;
                currentLine.SetPosition(0, p);
                currentLine.SetPosition(1, q);
            }
            //Cancel edit
            if (Input.GetMouseButtonDown(1))
            {
                currentMuscleNodeIndex = -1;
            }
            //Create muscle
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                int nodeIndex;
                if (hit.collider != null && int.TryParse(hit.collider.name, out nodeIndex))
                {
                    if (currentMuscleNodeIndex == -1)
                    {
                        currentMuscleNodeIndex = nodeIndex;
                    }
                    else if (currentMuscleNodeIndex != nodeIndex)
                    {
                        distanceJoints.Add(new DistanceJointStruct(currentMuscleNodeIndex, nodeIndex));
                        currentLine.SetPosition(1, new Vector2(hit.transform.position.x, hit.transform.position.y));
                        AddLine();
                        currentMuscleNodeIndex = -1;
                    }
                }
            }
        }

        void EditRotationMuscles()
        {
            //Render
            if (secondNodeIndex != -1)
            {
                var p = new Vector2(positions[anchorNodeIndex].X, positions[anchorNodeIndex].Y);
                lowerLimitUI.SetPosition(p);
                lowerLimitUI.SetActive(true);
                upperLimitUI.SetPosition(p);
                upperLimitUI.SetActive(true);
                lowerLimitUI.SetAngle(lowerLimit);
                upperLimitUI.SetAngle(upperLimit);
            }
            //Cancel edit
            if (Input.GetMouseButtonDown(1))
            {
                firstNodeGameObject.GetComponent<SpriteRenderer>().color = Color.white;
                anchorNodeGameObject.GetComponent<SpriteRenderer>().color = Color.white;
                secondNodeGameObject.GetComponent<SpriteRenderer>().color = Color.white;
                firstNodeIndex = -1;
                secondNodeIndex = -1;
                anchorNodeIndex = -1;
            }
            //Create muscle
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                int nodeIndex;
                if (hit.collider != null && int.TryParse(hit.collider.name, out nodeIndex))
                {
                    if (firstNodeIndex == -1)
                    {
                        firstNodeIndex = nodeIndex;
                        firstNodeGameObject = hit.transform.gameObject;
                        firstNodeGameObject.GetComponent<SpriteRenderer>().color = Color.green;
                    }
                    else if (anchorNodeIndex == -1 && nodeIndex != firstNodeIndex)
                    {
                        anchorNodeIndex = nodeIndex;
                        anchorNodeGameObject = hit.transform.gameObject;
                        anchorNodeGameObject.GetComponent<SpriteRenderer>().color = Color.blue;
                    }
                    else if(nodeIndex != firstNodeIndex && nodeIndex != anchorNodeIndex)
                    {
                        secondNodeIndex = nodeIndex;
                        secondNodeGameObject = hit.transform.gameObject;
                        secondNodeGameObject.GetComponent<SpriteRenderer>().color = Color.green;
                    }
                }
            }
            if (secondNodeIndex != -1)
            {
                upperLimit += 0.01f * Input.GetAxis("Vertical");
                lowerLimit += 0.01f * Input.GetAxis("Horizontal");

                lowerLimit = Mathf.Clamp(lowerLimit, 0, Mathf.PI);
                upperLimit = Mathf.Clamp(upperLimit, 0, Mathf.PI);
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //TODO: replace speed
                var a = ToVector2(positions[firstNodeIndex]);
                var b = ToVector2(positions[secondNodeIndex]);
                var anchor = ToVector2(positions[anchorNodeIndex]);
                var angle = Vector2.Angle(a - anchor, b - anchor) * Mathf.Deg2Rad;
                revoluteJoints.Add(new RevoluteJointStruct(firstNodeIndex, secondNodeIndex, anchorNodeIndex,
                    lowerLimit, upperLimit, 2000));

                firstNodeGameObject.GetComponent<SpriteRenderer>().color = Color.white;
                anchorNodeGameObject.GetComponent<SpriteRenderer>().color = Color.white;
                secondNodeGameObject.GetComponent<SpriteRenderer>().color = Color.white;
                firstNodeIndex = -1;
                secondNodeIndex = -1;
                anchorNodeIndex = -1;
            }
        }

        public void Update()
        {
            lowerLimitUI.SetActive(false);
            upperLimitUI.SetActive(false);
            currentLine.enabled = false;
            switch (editMode)
            {
                case EditMode.Nodes:
                    EditNodes();
                    break;
                case EditMode.DistanceMuscles:
                    EditDistanceMuscles();
                    break;
                case EditMode.RotationMuscles:
                    EditRotationMuscles();
                    break;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                editMode = EditMode.Nodes;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                editMode = EditMode.DistanceMuscles;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                editMode = EditMode.RotationMuscles;
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                AddPrefabs();
            }
        }

        public List<FVector2> GetPositions()
        {
            return positions;
        }

        public List<DistanceJointStruct> GetDistanceJoints()
        {
            return distanceJoints;
        }

        public List<RevoluteJointStruct> GetRevoluteJoints()
        {
            return revoluteJoints;
        }

        public void Destroy()
        {
            foreach (var o in objects)
            {
                Object.Destroy(o);
            }
            lowerLimitUI.Destroy();
            upperLimitUI.Destroy();
        }
    }
}
