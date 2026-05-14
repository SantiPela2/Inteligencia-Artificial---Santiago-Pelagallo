using System;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : MonoBehaviour
{
    public static List<Node> Run(Node initialNode, Func<Node, bool> isSatisfied, Func<Node, List<Node>> getConnections, Func<Node, Node, float> getCosts, int watchDog = 1000)
    {
        PriorityQueue<Node> pending = new PriorityQueue<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        Dictionary<Node, Node> parents = new Dictionary<Node, Node>();
        Dictionary<Node, float> costs = new Dictionary<Node, float>();
        costs[initialNode] = 0;

        pending.Enqueue(initialNode, 0);
        visited.Add(initialNode);

        //int counter = 0;

        while (!pending.IsEmpty)
        {
            //counter++;
            //if (counter > watchDog) break;

            Node node = pending.Dequeue();

            if (isSatisfied(node))
            {
                List<Node> path = new List<Node>();
                path.Add(node);
                Node current = node;

                while (parents.ContainsKey(current))
                {
                    path.Add(parents[current]);
                    current = parents[current];
                }

                path.Reverse();
                return path;
            }
            else
            {
                List<Node> children = getConnections(node);

                for (int i = 0; i < children.Count; ++i)
                {
                    if (visited.Contains(children[i]))
                    {
                        continue;
                    }
                    float currentCosts = costs[node] + getCosts(node, children[i]);
                    if (costs.ContainsKey(children[i]) && currentCosts > costs[children[i]])
                    {
                        continue;
                    }
                    costs[children[i]] = currentCosts;
                    pending.Enqueue(children[i], currentCosts);
                    visited.Add(children[i]);
                    parents[children[i]] = node;
                }
            }

        }
        return new List<Node>();

    }

}