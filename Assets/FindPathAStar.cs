﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathMarker) obj).location);
        }

    }

    public override int GetHashCode()
    {
        return 0;
    }
}



public class FindPathAStar : MonoBehaviour
{
    public Maze maze;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public PathMarker goalNode;
    public PathMarker startNode;

    PathMarker lastPos;
    bool done = false;

    void BeginSearch()
    {
        done = false;

        List<MapLocation> locations = new List<MapLocation>();
        for (int z = 1; z < maze.depth - 1; z++)
            for (int x = 1; x < maze.width - 1; x++)
            {
                if (maze.map[x, z] != 1)
                    locations.Add(new MapLocation(x, z));
            }

        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0, null);


        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0, null);

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
    }

    void BeginSearch(PathMarker start, PathMarker end)
    {
        done = false;

        maze.locations.Shuffle();

        startNode = start;
        goalNode = end;

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode)
    {
        if (thisNode.Equals(goalNode)) { done = true; return; } //goal has been found

        foreach (MapLocation dir in maze.directions)
        {
            MapLocation neighbour = dir + thisNode.location;
            if (maze.map[neighbour.x, neighbour.z] == 1) continue;
            if (neighbour.x < 1 || neighbour.x >= maze.width || neighbour.z < 1 || neighbour.z >= maze.depth) continue;
            if (IsClosed(neighbour)) continue;

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float F = G + H;


            if (!UpdateMarker(neighbour, G, H, F, thisNode))
                open.Add(new PathMarker(neighbour, G, H, F, thisNode));
        }

        open = open.OrderBy(p => p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker) open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);

        lastPos = pm;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        foreach (PathMarker p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }
        return false;
    }

    // Start is called before the first frame update
    public void Build()
    {
        BeginSearch();
        while (!done)
            Search(lastPos);
        maze.InitialiseMap();
        MarkPath();
    }

    public PathMarker Build(Maze m, MapLocation start, MapLocation end)
    {
        maze = m;
        BeginSearch(new PathMarker(start, 0, 0, 0, null), new PathMarker(end, 0, 0, 0, null));
        while (!done)
            Search(lastPos);

        return lastPos;
    }

    void MarkPath()
    {
        PathMarker begin = lastPos;

        while (!startNode.Equals(begin) && begin != null)
        {
            //Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale),
            //Quaternion.identity);
            maze.map[begin.location.x, begin.location.z] = 0;
            begin = begin.parent;
        }

        //Instantiate(pathP, new Vector3(startNode.location.x * maze.scale, 0, startNode.location.z * maze.scale),
               //Quaternion.identity);
    }

    void GetPath()
    {
        PathMarker begin = lastPos;

        while (!startNode.Equals(begin) && begin != null)
        {
            begin = begin.parent;
        }

    }

}
