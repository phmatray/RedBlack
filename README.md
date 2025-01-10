# Red-Black Tree with Order Statistics & Duplicates

A **C#** implementation of a **Red-Black Tree** supporting:

- **Multiset Counting**: Multiple occurrences of the same key stored in a single node.
- **Order Statistics**:
    - **Select(k)**: Get the k-th smallest element.
    - **Rank(key)**: Find how many elements are strictly less than `key`.
- **Sentinel Node** design to simplify `null` checks, keeping the tree balanced at all times.
- Classic **BST** operations: Insert, Search/Contains, and Delete in **\(O(\log n)\)**.
- Built-in **IEnumerable** (in-order traversal) yields duplicates consecutively in sorted order.

Tested with [**BenchmarkDotNet**](https://github.com/dotnet/BenchmarkDotNet) for performance measurements.

## Table of Contents

<!-- TOC -->
* [Red-Black Tree with Order Statistics & Duplicates](#red-black-tree-with-order-statistics--duplicates)
  * [Table of Contents](#table-of-contents)
  * [Features](#features)
  * [Installation](#installation)
  * [Usage Example](#usage-example)
  * [API Overview](#api-overview)
  * [Benchmarking](#benchmarking)
    * [Quick Start](#quick-start)
  * [Contributing](#contributing)
  * [License](#license)
<!-- TOC -->

---

## Features

1. **Red-Black Balancing**
    - Guaranteed worst-case height of \(O(\log n)\) for insertion, deletion, and search.

2. **Multiset**
    - Storing duplicates directly in a node’s `DuplicateCount` (instead of having separate nodes for identical keys).
    - Reduces tree growth for repeated values.

3. **Order Statistics**
    - **Select(k)**: Get the k-th smallest element (1-based).
    - **Rank(key)**: How many elements are strictly less than `key`. (Adjustable to “less or equal” if needed.)

4. **Sentinel Node**
    - One `Nil` node for all `null` references. Simplifies logic, fewer `null` checks.

5. **IEnumerable**
    - Traverse in ascending order (duplicates included multiple times).

6. **BenchmarkDotNet Setup**
    - Sample benchmark class to measure performance of typical operations.

---

## Installation

1. **Clone** or **download** this repository.
2. **Open** it in Visual Studio, Rider, VSCode, or any other C# IDE.
3. Make sure your project references the **.cs** file(s) containing the Red-Black Tree code.

---

## Usage Example

Below is a simple program showing how to use the **RedBlackTree** data structure:

```csharp
using System;
using RedBlackTreeOrderStats;

public class Program
{
    public static void Main(string[] args)
    {
        var tree = new RedBlackTree<int>();

        // Insert elements
        tree.Insert(10);
        tree.Insert(5);
        tree.Insert(5);  // Duplicate
        tree.Insert(20);
        tree.Insert(15);
        tree.Insert(15); // Duplicate
        tree.Insert(15); // Another duplicate

        // Print total count (including duplicates)
        Console.WriteLine($"Tree Count = {tree.Count}"); 
        // In-order
        foreach (var x in tree)
            Console.Write(x + " ");
        Console.WriteLine();

        // Check membership
        Console.WriteLine($"Contains(10)? {tree.Contains(10)}"); // true

        // Order-statistics
        Console.WriteLine("Rank(15) = " + tree.Rank(15));  // # of elements < 15
        Console.WriteLine("Select(3) = " + tree.Select(3)); // 3rd smallest

        // Deletion: If a node has duplicates, we decrement first
        // Deleting 15 once
        tree.Delete(15);
        Console.WriteLine($"After deleting 15 once, count = {tree.Count}");
        
        // Print in-order again
        tree.PrintInOrder();
    }
}
```

**Expected Output** might look like:

```
Tree Count = 7
5 5 10 15 15 15 20 
Contains(10)? True
Rank(15) = 2
Select(3) = 10
After deleting 15 once, count = 6
5(Red):2[2] 10(Black):1[1] 15(Red):2[2] 20(Black):1[1] 
```

(The exact color output depends on the internal balancing.)

---

## API Overview

Below are key members of the `RedBlackTree<T>` class:

- **Insert(T key)**  
  Inserts a new key in \(O(\log n)\). If `key` already exists, it increments `DuplicateCount` in that node.

- **Delete(T key)**  
  Removes one occurrence of `key`. If that node has `DuplicateCount > 1`, it decrements; otherwise removes the node from the tree. Returns `true` if a key was removed, `false` if not found.

- **Contains(T key)**  
  Checks membership in \(O(\log n)\).

- **int Count**  
  Total number of elements, including duplicates.

- **T Select(int k)**  
  Returns the k-th smallest element (1-based). Throws if `k` is out of range.

- **int Rank(T key)**  
  Returns how many elements are strictly less than `key`. If you want “less or equal,” adapt logic in code or do `Rank(key+1)` in some scenarios (for numeric types).

- **IEnumerable<T>**  
  The tree is enumerable in ascending order. For duplicates, each duplicate is yielded.

- **PrintInOrder()**  
  Debug method printing each node in ascending order, along with color, `DuplicateCount`, and subtree size.

---

## Benchmarking

We provide a **BenchmarkDotNet** sample that measures:
- **Insert** performance (e.g., inserting \(N\) elements).
- **Search** performance (e.g., random lookups).
- **Delete** performance.

Based on these **BenchmarkDotNet** results:

| Method     | Mean (ns)       | Error (ns)    | StdDev (ns)   | Median (ns)     | Allocated (B) |
|----------- |----------------:|--------------:|--------------:|----------------:|--------------:|
| **InsertTest** | **3,689,277.60** | **18,956.626** | **16,804.555** | **3,682,642.58** | **556,619**     |
| **SearchTest** |             15.13 |          0.025 |          0.022 |           15.13 | 0            |
| **DeleteTest** |             36.98 |          0.753 |          1.237 |           36.35 | 0            |

you might think the **InsertTest** is “too large” or that there is a “problem,” especially compared to the tiny times for **SearchTest** and **DeleteTest**. However, there are a few key points to understand:

**InsertTest Likely Performs *N* Operations, While Search/Delete Are 1 Operation**

- **InsertTest** typically inserts **tens of thousands** (or even **hundreds of thousands**) of elements in a loop. The benchmark time (e.g., ~3.68 ms) is the total **for all those insertions** combined.
- **SearchTest** and **DeleteTest** as shown are often just **one** operation each (searching or deleting a single element). That’s why you see extremely small times (15 ns or 37 ns).

**Memory Allocation (~556 KB)**

- Allocating **~556,619 bytes** for tens or hundreds of thousands of inserted elements may be **reasonable**.
- Each node has overhead (fields for parent, left, right, color, size counters, etc.).
- If you inserted 50k or 100k elements, half a megabyte is not surprising for a tree of that size.

In short, there’s **no inherent problem** in seeing a “large” (few milliseconds) total time for **InsertTest** in contrast to sub-100-nanosecond times for single **SearchTest** or **DeleteTest**. They’re measuring fundamentally different things: a **batch** of tens/hundreds of thousands of inserts vs. **one** search or one delete.

### Quick Start

1. **Create a Console Project** (or use the existing sample).
2. **Install** BenchmarkDotNet:

   ```bash
   dotnet add package BenchmarkDotNet
   ```

3. **Add** a benchmark class referencing the Red-Black Tree. For instance:

   ```csharp
   using BenchmarkDotNet.Attributes;
   using BenchmarkDotNet.Running;

   [MemoryDiagnoser]
   public class MyRedBlackBench
   {
       private RedBlackTree<int>? _tree;
       private int[]? _data;
       private const int N = 100_000;

       [GlobalSetup]
       public void Setup()
       {
           _tree = new RedBlackTree<int>();
           _data = new int[N];
           var rand = new Random(0);
           for (int i = 0; i < N; i++)
           {
               _data[i] = rand.Next(1, 10_000);
           }
       }

       [Benchmark]
       public void InsertTest()
       {
           var tree = new RedBlackTree<int>();
           for (int i = 0; i < N; i++)
           {
               tree.Insert(_data![i]);
           }
       }

       [Benchmark]
       public bool SearchTest()
       {
           return _tree!.Contains(_data![N / 2]);
       }
   }

   class Program
   {
       static void Main(string[] args)
       {
           var summary = BenchmarkRunner.Run<MyRedBlackBench>();
       }
   }
   ```

4. **Run** in Release mode:

   ```bash
   dotnet run -c Release
   ```

You’ll see an output table with timings and memory usage.

---

## Contributing

Contributions are welcome! Feel free to:

1. Open issues for bugs, edge-case failures, or enhancements.
2. Submit pull requests with improved balancing logic, concurrency support, or additional features (like **range queries**, custom comparers, etc.).

Please include relevant **tests** for any changes to maintain code quality.

---

## License

This project is available under a permissive license \(e.g. [MIT License](https://opensource.org/licenses/MIT)\). Feel free to customize or adopt a different license based on your organization’s needs.

---

**Enjoy using the Red-Black Tree with order statistics and multiset support!** If you find any issues, please open an issue or create a pull request. Happy coding!
