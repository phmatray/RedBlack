using RedBlack;

var tree = new RedBlackTree<int>();

int[] data = { 10, 10, 5, 12, 3, 7, 15, 10 }; 
// Notice we inserted 10 multiple times (duplicates allowed)

foreach (int val in data)
{
    tree.Insert(val);
}

Console.WriteLine($"Tree count: {tree.Count}"); // Should reflect all duplicates
Console.WriteLine("InOrder:");
tree.PrintInOrder();  // Will show sorted order with duplicates

Console.WriteLine("All values via IEnumerable:");
foreach (var item in tree)
{
    Console.Write(item + " ");
}
Console.WriteLine();

Console.WriteLine($"Min = {tree.Min()}, Max = {tree.Max()}");

Console.WriteLine("Deleting 10...");
tree.Delete(10);
Console.WriteLine($"Tree count: {tree.Count}");
tree.PrintInOrder();

// Check membership
Console.WriteLine($"Tree contains 10? {tree.Contains(10)}");
Console.WriteLine($"Tree contains 999? {tree.Contains(999)}");