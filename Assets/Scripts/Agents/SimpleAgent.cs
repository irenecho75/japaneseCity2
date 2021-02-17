﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAgent : MonoBehaviour
{
    private TakeScreenshot screenshot;
    private NavMeshAgent navMeshAgent;
    //private List<Vector3> destList = new List<Vector3>();
    private List<List<Vector3>> regions = new List<List<Vector3>>();
    private Vector3 myDest = new Vector3(0, 0, 0);
    public int totalPoints = 40;
    public bool mouseMode = false; //true = the destination is set by clicking and/or holding left click down. false = roam randomly between a list random points, where.

    // Start is called before the first frame update
    /*void Start()
    {
        screenshot = this.GetComponent<TakeScreenshot>();
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = false;
    }*/

    // Update is called once per frame
    void Update()
    {
        myDest = navMeshAgent.destination;
        if (mouseMode)
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    navMeshAgent.SetDestination(hit.point);
                    screenshot.CaptureScreenshot(Camera.main, Screen.width, Screen.height);
                }
            }
        }
        else {
            if ( navMeshAgent.enabled && navMeshAgent.remainingDistance < 0.2f) {
                //navMeshAgent.SetDestination(destList[Random.Range(0, destList.Count)]);
                if (regions.Count > 0)
                {
                    Vector3 v = getRandomPoint();
                    if(navMeshAgent.enabled)
                        navMeshAgent.SetDestination(v);
                    screenshot.CaptureScreenshot(Camera.main, Screen.width, Screen.height);
                }
            }
        }
    }

    public void StartAgent(List<(Vector3, Vector3)> bboxlist) {
        gameObject.SetActive(true);
        screenshot = GetComponent<TakeScreenshot>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        //make regions
        NavMeshPath path = new NavMeshPath();
        List<Vector3> d = createRandomPoints(bboxlist);
        while (d.Count > 0)
		{
            Vector3 src = d[0];
            List<Vector3> region = new List<Vector3>();
            for (int i=0; i<d.Count; i++)
			{
                NavMesh.CalculatePath(src, d[i], NavMesh.AllAreas, path);
				if (path.status == NavMeshPathStatus.PathComplete)
                {
                    //Then d[0] and d[i] share the same region.
                    region.Add(d[i]);
                    d.RemoveAt(i);
                    i--;
				}
            }
            regions.Add(region);
		}
        transform.position = regions[0][0];
        navMeshAgent.enabled = true;
    }

    public void ResetAgent(List<List<Vector3>> regions)
    {
        if (regions.Count > 0)
        {
            transform.position = regions[0][0];
            navMeshAgent.enabled = true;
        }
        else
        {
            navMeshAgent.enabled = false;
        }
    }

    private Vector3 getRandomPoint()
	{
        /*
        int x = Random.Range(0, destLen);
        for (int i = 0; i < regions.Count; i++)
        {
            int l = regions[i].Count;
            if (x >= l)
            {
                x -= l;
            }
            else
            {
                return regions[i][x];
            }
        }
        return regions[0][0];
        */
        if (regions[0].Count > 0)
        {
            int x = Random.Range(0, regions[0].Count);
            Vector3 point = regions[0][x];
            regions[0].RemoveAt(x);
            return point;
        }
        else
        {
            navMeshAgent.enabled = false;
            regions.RemoveAt(0);
            ResetAgent(regions);
            return transform.position;
        }
    }

    private List<Vector3> createRandomPoints(List<(Vector3, Vector3)> bboxlist)
    {
        List<Vector3> randomPoints = new List<Vector3>();
        int pointsPerLevel = totalPoints / bboxlist.Count;
        for (int l = 0; l < bboxlist.Count; l++)
        {
            //generate totalPoints random points on NavMesh!
            if (l == bboxlist.Count - 1)
            {
                pointsPerLevel += totalPoints - (pointsPerLevel * bboxlist.Count);
            }
            for (int i = 0; i < pointsPerLevel; i += 0)
            {
                float rx = UnityEngine.Random.Range(bboxlist[l].Item1.x, bboxlist[l].Item2.x);
                float ry = UnityEngine.Random.Range(bboxlist[l].Item1.y, bboxlist[l].Item2.y);
                float rz = UnityEngine.Random.Range(bboxlist[l].Item1.z, bboxlist[l].Item2.z);
                Vector3 randomPoint = new Vector3(rx, ry, rz);
                NavMeshHit hit;
                Vector3 result = new Vector3(0, 0, 0); //All this code is doing is finding a random point within the bounding box of the level we are looking at,
                                                       //and then finding the closest point on the NavMesh.
                if (NavMesh.SamplePosition(randomPoint, out hit, 70.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    randomPoints.Add(result);
                    i++;
                }
                /*else
                {
                    Debug.Log("Point not found.");
                    Debug.Log(randomPoint);
                }*/

            }
        }
        return randomPoints;
    }

    void OnDrawGizmos()
    {
        float radius = 0.1f;
        Gizmos.color = Color.red;
        Color[] colors = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue, Color.green, Color.magenta, Color.black };
        for (int i = 0; i < regions.Count; i++)
        {
            Gizmos.color = colors[i%8];
            foreach (Vector3 v in regions[i])
                Gizmos.DrawSphere(v, radius);
        }
        Gizmos.color = Color.white;
        if (Application.isPlaying)
            Gizmos.DrawSphere(myDest, radius+0.1f);

    }
}
