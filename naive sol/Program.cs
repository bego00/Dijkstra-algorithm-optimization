using System.Text.RegularExpressions;


Dictionary<int, Tuple<int, int>> nodes = new Dictionary<int, Tuple<int, int>>();// node , coordinates (x,y)
Dictionary<int, HashSet<int>> adj = new Dictionary<int, HashSet<int>>();//node and its neighours
Dictionary<Tuple<int, int, double>, HashSet<int>> path = new Dictionary<Tuple<int, int, double>, HashSet<int>>(); //source ,destination ,return shortest path
double memoryUsage = 0;




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
    //distance.Add(node, double.MaxValue);
}

System.GC.Collect();
Console.WriteLine($"after first loop: {System.Environment.WorkingSet / 1024f / 1024f} MB");
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
Console.WriteLine($"after second loop: {System.Environment.WorkingSet / 1024f / 1024f} MB");

// reading routes file
string[] routes = System.IO.File.ReadAllLines(@"ShortRoutes100.txt");
int routeSize = Int32.Parse(routes[0]);

var watch = System.Diagnostics.Stopwatch.StartNew();//start time to calculate .

for (int i = 1; i < routeSize + 1; ++i)
{
    string line = Regex.Replace(routes[i], @"\s+", " ");
    line = line.Trim();
    int source = Int32.Parse(line.Split()[0]);
    int destination = Int32.Parse(line.Split()[1]);
    double cost;
    var sol = dijkstra(source, destination, out cost);//call dijkstra Function
    path.TryAdd(Tuple.Create(source, destination, cost), sol);
}

Console.WriteLine($"after third loop: {System.Environment.WorkingSet / 1024f / 1024f} MB");
watch.Stop();//end time to calculate .
Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");// write execution time.
Console.WriteLine($"Highest Memory Usage at Dijkstra: {memoryUsage}");

// return shortest pathes in path file
using (StreamWriter stream = File.AppendText("TC4.txt"))
{
    foreach (var sol in path)
    {
        string output = string.Join(" ", sol.Value);
        stream.WriteLine($"{Math.Round(sol.Key.Item3, 1)} {sol.Key.Item1} {sol.Key.Item2} " + output + " \n");
    }

}

//dijkstra function.
HashSet<int> dijkstra(int source, int destination, out double cost)
{
    Dictionary<int, double> distance = new Dictionary<int, double>();//cost,node
    Dictionary<int, int> parent = new Dictionary<int, int>();//node,parent

    HashSet<int> path = new HashSet<int>();//store path form source to destination.
    PriorityQueue<int, double> queue = new PriorityQueue<int, double>();//queue store node and cost.
    HashSet<int> visited = new HashSet<int>();//visited vertices
    foreach (var node in nodes.Keys)
    {
        distance.Add(node, double.MaxValue);
        parent.Add(node, -1);
    }

    queue.Enqueue(source, 0);//initiate source in queue
    distance[source] = 0;//initiate source destance
    parent[source] = -1;//initiate source parent
    visited.Add(source);//save source in visited vertices

    //loop on all vertices
    while (queue.Count > 0)
    {
        int root = queue.Dequeue(); //extract minimum
        // relaxe vertex
        foreach (var neighbor in adj[root])
        {
            visited.Add(neighbor);//add to visited vertices
            double dist = Math.Sqrt(Math.Pow(nodes[root].Item1 - nodes[neighbor].Item1, 2) + Math.Pow(nodes[root].Item2 - nodes[neighbor].Item2, 2)) + distance[root];//caluclate Euclidean distance
            //check distance and update vertices short distance
            if (distance[neighbor] > dist)
            {
                distance[neighbor] = dist;
                parent[neighbor] = root;
                queue.Enqueue(neighbor, dist);
            }
        }
    }

    int dest = parent[destination];//store parent of destination
    path.Add(destination);//store destination in path
    if (!parent.ContainsKey(destination))
    {
        cost = 0;
        return path;
    }
    //loop to backtrak path to source
    while (dest != -1)
    {
        path.Add(dest);//add parent of all vertices in path
        dest = parent[dest];//update destination with parent
    }
    //add source to path
    path.Add(source);
    cost = distance[destination];
    memoryUsage = Math.Max(System.Environment.WorkingSet / 1024f / 1024f, memoryUsage);

    return path;
}