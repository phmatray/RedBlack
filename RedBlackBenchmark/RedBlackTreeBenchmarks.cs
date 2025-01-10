using BenchmarkDotNet.Attributes;
using RedBlack;

namespace RedBlackBenchmark;

[MemoryDiagnoser]  // measures memory, too
public class RedBlackTreeBenchmarks
{
    private RedBlackTree<int>? _tree;
    private int[]? _testData;

    private const int N = 100_000;  // Size of our test input

    [GlobalSetup]
    public void Setup()
    {
        // 1) Generate some random data
        var random = new Random(0);
        _testData = Enumerable.Range(1, N)
            .Select(_ => random.Next(1, 10_000)) // random in [1..10000]
            .ToArray();

        // 2) Create a fresh Red-Black tree
        _tree = new RedBlackTree<int>();

        // 3) Pre-insert half the data (optional scenario)
        //    So we have an already partially-filled tree
        for (int i = 0; i < N / 2; i++)
        {
            _tree.Insert(_testData[i]);
        }
    }

    [Benchmark]
    public void InsertTest()
    {
        // Insert the *other* half of the data that wasn't pre-inserted
        // We'll just do it fresh each time
        var tree = new RedBlackTree<int>();
        for (int i = 0; i < N / 2; i++)
        {
            tree.Insert(_testData![i]);
        }
    }

    [Benchmark]
    public bool SearchTest()
    {
        // We'll search for an element from the second half
        // Just pick an index in the second half
        int idx = (3 * N) / 4;  // 75% index
        return _tree!.Contains(_testData![idx]);
    }

    [Benchmark]
    public void DeleteTest()
    {
        // We'll delete the last item from the *first half* 
        // that was inserted in [GlobalSetup]
        int idx = (N / 2) - 1; 
        _tree!.Delete(_testData![idx]);
            
        // Then re-insert it to maintain the same state
        _tree.Insert(_testData[idx]);
    }
}