// Run all benchmarks in the assembly

using BenchmarkDotNet.Running;
using RedBlackBenchmark;

var summary = BenchmarkRunner.Run<RedBlackTreeBenchmarks>();

// OR if we have multiple classes:
// BenchmarkRunner.Run(typeof(Program).Assembly);