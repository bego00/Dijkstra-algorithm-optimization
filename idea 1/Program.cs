using Lucene.Net.Util;
using System.Collections.Generic;
using System.Text.RegularExpressions;
double memoryUsage = 0;

Dictionary<int, Tuple<int, int>> nodes = new Dictionary<int, Tuple<int, int>>();// node , coordinates (x,y)
Dictionary<int, HashSet<int>> adj = new Dictionary<int, HashSet<int>>();//node and its neighours
Dictionary<int, double> distance = new Dictionary<int, double>();//cost,node

Dictionary<Tuple<int, int, double>, List<int>> path = new Dictionary<Tuple<int, int, double>, List<int>>(); //source ,destination ,return list of shortest path
Console.WriteLine($"first after init: {System.Environment.WorkingSet / 1024f / 1024f}");
//read map file
string[] lines = System.IO.File.ReadAllLines(@"Map.txt");
int nodeSize = Int32.Parse(lines[0].Split()[0]);
int edgeSize = Int32.Parse(lines[0].Split()[1]);


//read vertex position and coordinates 
for (int i = 1; i < nodeSize + 1; ++i)
{
    string line = Regex.Replace(lines[i], @"\s+", " ");
    line = line.Trim();
    int node = Int32.Parse(line.Split()[0]);
    int x = Int32.Parse(line.Split()[1]);
    int y = Int32.Parse(line.Split()[2]);
    nodes.Add(node, Tuple.Create(x, y));
    adj.Add(node, new HashSet<int>());
    distance.Add(node, double.MaxValue);
}

System.GC.Collect();
Console.WriteLine($"first after first loop: {System.Environment.WorkingSet / 1024f / 1024f}");
for (int i = nodeSize + 1; i < nodeSize + edgeSize + 1; ++i)
{
    string line = Regex.Replace(lines[i], @"\s+", " ");
    line = line.Trim();
    int node = Int32.Parse(line.Split()[0]);
    int neighbor = Int32.Parse(line.Split()[1]);
    adj[node].Add(neighbor);
    adj[neighbor].Add(node);

}
System.GC.Collect();
Console.WriteLine($"second after second loop: {System.Environment.WorkingSet / 1024f / 1024f}");
// reading routes file
string[] routes = System.IO.File.ReadAllLines(@"ShortRoutes50000.txt");
int routeSize = Int32.Parse(routes[0]);

var watch = System.Diagnostics.Stopwatch.StartNew();//start time to calculate 
for (int i = 1  ; i < routeSize + 1 ; ++i)
{
    string line = Regex.Replace(routes[i], @"\s+", " ");
    line = line.Trim();
    int source = Int32.Parse(line.Split()[0]);
    int destination = Int32.Parse(line.Split()[1]);
    double cost;
    var sol = dijkstra(source, destination, out cost);
    path.TryAdd(Tuple.Create(source, destination, cost), sol);
    //Path.Add(Tuple.Create(source, destination), dijkstra(source, destination));
}
watch.Stop();
Console.WriteLine($"third after third loop: {System.Environment.WorkingSet / 1024f / 1024f}");
Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");// write execution time.
Console.WriteLine($"Highest Memory Usage at Dijkstra: {memoryUsage}");
//return shortest pathes
using (StreamWriter stream = new("TC2-op.txt"))
{
    foreach (var sol in path)
    {
        string output = string.Join(" ", sol.Value);
        stream.WriteLine($"{Math.Round(sol.Key.Item3, 1)} {sol.Key.Item1} {sol.Key.Item2} " + output + " \n");
    }

}


List<int> dijkstra(int source, int destination, out double cost)
{

    Dictionary<int, int> parent = new Dictionary<int, int>();//node,parent
    List<int> path = new List<int>();
    PriorityQueue<int, double> queue = new PriorityQueue<int, double>();
    HashSet<int> visited = new HashSet<int>();
    queue.Enqueue(source, 0);

    distance[source] = 0;
    parent.Add(source, -1);
    visited.Add(source);
    
    while (queue.Count > 0)
    {
        int root = queue.Dequeue();
        if (root == destination)
        {
            goto exit;
        }
        foreach (var neighbor in adj[root])
        {
            visited.Add(neighbor);
            double dis = Math.Sqrt(Math.Pow(nodes[root].Item1 - nodes[neighbor].Item1, 2) + Math.Pow(nodes[root].Item2 - nodes[neighbor].Item2, 2)) + distance[root];

            if (distance[neighbor] > dis)
            {
                distance[neighbor] = dis;
                parent[neighbor] = root;
                queue.Enqueue(neighbor, dis);
            }
        }
    }

exit:
    cost = distance[destination];
    foreach (var node in visited)
        distance[node] = double.MaxValue;
    if (!parent.ContainsKey(destination))
        return path;

    int dest = parent[destination];
    path.Add(destination);
    while (dest != -1)
    {
        path.Add(dest);
        dest = parent[dest];
    }
    //  path.Add(source);
    memoryUsage = Math.Max(System.Environment.WorkingSet / 1024f / 1024f, memoryUsage);
    return path.Reverse<int>().ToList();
}