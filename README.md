# small-list

![Build and Test](https://github.com/varelen/small-list/actions/workflows/dotnet.yml/badge.svg?branch=main)
![Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/varelen/b7fbb39b2c38deefe76a2f1e13bb9901/raw/d4589424e4605336b84738bca8c2b1ecb3fe145f/small-list-cobertura-coverage.json)
![NuGet Version](https://img.shields.io/nuget/v/Varelen.SmallList)
![GitHub License](https://img.shields.io/github/license/varelen/small-list)

An optimized list implementation which remains fast and also allocation free for a small number of items (up to 4) for .NET 8+.

This was mainly created as a learning and weekend project and inspired by optimizations done in other projects (e.g., the LLVM [`SmallVector`](https://llvm.org/doxygen/classllvm_1_1SmallVector.html) class).

## Content

* [Features](#features)
* [Installation](#installation)
* [Usage](#usage)
* [Benchmarks](#benchmarks)
  * [Zero allocation instantiation](#zero-allocation-instantiation)
  * [For Each](#for-each)
  * [Indexer](#indexer)
  * [Contains](#contains)
* [Contributing](#contributing)
* [License](#license)

## Features

- [X] up to 4 inlined items
  - [X] zero heap allocations
  - [X] optimized foreach and indexer
- [X] still implements `IList<T>` and `IEnumerable<T>`
  - [X] can be used like a normal list
  - [X] can be passed to existing methods that use these interfaces
  - [X] can be used with LINQ
- [X] unit tested

## Installation

You can use [NuGet](https://www.nuget.org/packages/Varelen.SmallList/) to install this package.

Example via .NET CLI:

```bash
dotnet add package Varelen.SmallList --version 0.0.1-alpha2
```

## Usage

The list can store up to 4 items inline without any allocations.

```c#
// Conveniant constructors for 0 to 4 items
var smallList = new SmallList<int>(1, 2, 3, 4);
// or via an array / IEnumerable
// var smallList = new SmallList<int>([1, 2, 3, 4, 5, 6, 7]);

var first = smallList[0];

smallList[0] = 100;

var firstChanged = smallList[0];

Console.WriteLine(first);
Console.WriteLine(firstChanged);
// 1
// 100
```

Using `foreach` on the explicit type is also optimized.

```c#
var smallList = new SmallList<int>(100, 200);

foreach (int i in smallList)
{
    Console.WriteLine(i);
}
// 100
// 200
```

Since `SmallList<T>` also implements `IList<T>` and so also `IEnumerable<T>`, you can use the type in existing methods too. If you really need to, it also works with LINQ extension methods on `IEnumerable<T>`.

Keep in mind that this implicit boxing of the struct `SmallList<T>` causes allocations. Although I have tried to reduce it to the minimum, there is a small overhead.

```c#
var smallList = new SmallList<int>(1, 2, 100);

int sum = MySum(smallList);
int linqSum = smallList.Sum();

Console.WriteLine(sum);
// 103

Console.WriteLine(linqSum);
// 103

static int MySum(IEnumerable<int> list)
{
    int sum = 0;

    foreach (int item in list)
    {
        sum += item;
    }

    return sum;
}
```

## Benchmarks

Benchmarks are located unter the `benchmarks` folder.

The system under which the shown benchmarks ran:

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
AMD Ryzen 9 5900X 3.70GHz, 1 CPU, 24 logical and 12 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
```

### Zero allocation instantiation

Creation with constructor is just passing an array to the `List<T>` or `SmallList<T>` constructor.

Creation loop add is creating an empty instance of `List<T>` or `SmallList<T>` and manually looping over an array and calling `Add`.

Creation with value type (int):

| Method                         | Categories          | N  | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------------- |--- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Creation_Constructor_List**      | **CreationConstructor** | **4**  |  **6.727 ns** | **0.0705 ns** | **0.0659 ns** |  **1.00** |    **0.01** | **0.0043** |      **72 B** |        **1.00** |
| Creation_Constructor_SmallList | CreationConstructor | 4  |  1.835 ns | 0.0080 ns | 0.0067 ns |  0.27 |    0.00 |      - |         - |        0.00 |
|                                |                     |    |           |           |           |       |         |        |           |             |
| **Creation_Constructor_List**      | **CreationConstructor** | **24** |  **8.407 ns** | **0.0928 ns** | **0.0823 ns** |  **1.00** |    **0.01** | **0.0091** |     **152 B** |        **1.00** |
| Creation_Constructor_SmallList | CreationConstructor | 24 |  7.661 ns | 0.0682 ns | 0.0638 ns |  0.91 |    0.01 | 0.0072 |     120 B |        0.79 |
|                                |                     |    |           |           |           |       |         |        |           |             |
| **Creation_Loop_List**             | **CreationLoopAdd**     | **4**  | **10.230 ns** | **0.0730 ns** | **0.0647 ns** |  **1.00** |    **0.01** | **0.0043** |      **72 B** |        **1.00** |
| Creation_Loop_SmallList        | CreationLoopAdd     | 4  |  5.729 ns | 0.0576 ns | 0.0539 ns |  0.56 |    0.01 |      - |         - |        0.00 |
|                                |                     |    |           |           |           |       |         |        |           |             |
| **Creation_Loop_List**             | **CreationLoopAdd**     | **24** | **68.262 ns** | **0.2664 ns** | **0.2361 ns** |  **1.00** |    **0.00** | **0.0219** |     **368 B** |        **1.00** |
| Creation_Loop_SmallList        | CreationLoopAdd     | 24 | 55.680 ns | 1.1061 ns | 1.3584 ns |  0.82 |    0.02 | 0.0176 |     296 B |        0.80 |

Creation with reference type (simple class with int):

| Method                         | Categories          | N  | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------------- |--- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **Creation_Constructor_List**      | **CreationConstructor** | **4**  | **20.610 ns** | **0.1651 ns** | **0.1544 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| Creation_Constructor_SmallList | CreationConstructor | 4  |  2.150 ns | 0.0059 ns | 0.0053 ns |  0.10 |      - |         - |        0.00 |
|                                |                     |    |           |           |           |       |        |           |             |
| **Creation_Constructor_List**      | **CreationConstructor** | **24** | **25.143 ns** | **0.1258 ns** | **0.1176 ns** |  **1.00** | **0.0148** |     **248 B** |        **1.00** |
| Creation_Constructor_SmallList | CreationConstructor | 24 | 19.202 ns | 0.1097 ns | 0.0972 ns |  0.76 | 0.0129 |     216 B |        0.87 |
|                                |                     |    |           |           |           |       |        |           |             |
| **Creation_Loop_List**             | **CreationLoopAdd**     | **4**  | **10.115 ns** | **0.0632 ns** | **0.0592 ns** |  **1.00** | **0.0043** |      **72 B** |        **1.00** |
| Creation_Loop_SmallList        | CreationLoopAdd     | 4  |  5.782 ns | 0.0992 ns | 0.0927 ns |  0.57 |      - |         - |        0.00 |
|                                |                     |    |           |           |           |       |        |           |             |
| **Creation_Loop_List**             | **CreationLoopAdd**     | **24** | **68.411 ns** | **0.6428 ns** | **0.5699 ns** |  **1.00** | **0.0219** |     **368 B** |        **1.00** |
| Creation_Loop_SmallList        | CreationLoopAdd     | 24 | 38.342 ns | 0.2428 ns | 0.2153 ns |  0.56 | 0.0176 |     296 B |        0.80 |

### For Each

| Method            | Categories | N  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------ |----------- |--- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **ForEach_List**      | **ForEach**    | **4**  |  **2.052 ns** | **0.0361 ns** | **0.0338 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| ForEach_SmallList | ForEach    | 4  |  1.251 ns | 0.0095 ns | 0.0089 ns |  0.61 |    0.01 |         - |          NA |
|                   |            |    |           |           |           |       |         |           |             |
| **ForEach_List**      | **ForEach**    | **24** | **10.540 ns** | **0.1537 ns** | **0.1437 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| ForEach_SmallList | ForEach    | 24 |  5.967 ns | 0.0461 ns | 0.0431 ns |  0.57 |    0.01 |         - |          NA |

### Indexer

| Method            | Categories | N  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------ |----------- |--- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **Indexer_List**      | **Indexer**    | **4**  |  **2.229 ns** | **0.0233 ns** | **0.0218 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| Indexer_SmallList | Indexer    | 4  |  1.455 ns | 0.0134 ns | 0.0126 ns |  0.65 |    0.01 |         - |          NA |
|                   |            |    |           |           |           |       |         |           |             |
| **Indexer_List**      | **Indexer**    | **24** | **12.989 ns** | **0.1914 ns** | **0.1494 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Indexer_SmallList | Indexer    | 24 | 12.649 ns | 0.0953 ns | 0.0891 ns |  0.97 |    0.01 |         - |          NA |


### Contains

| Method             | N  | Mean      | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------------------- |--- |----------:|----------:|----------:|------:|----------:|------------:|
| **Contains_List**      | **4**  |  **3.490 ns** | **0.0166 ns** | **0.0155 ns** |  **1.00** |         **-** |          **NA** |
| Contains_SmallList | 4  |  3.454 ns | 0.0195 ns | 0.0182 ns |  0.99 |         - |          NA |
|                    |    |           |           |           |       |           |             |
| **Contains_List**      | **42** | **11.621 ns** | **0.0595 ns** | **0.0557 ns** |  **1.00** |         **-** |          **NA** |
| Contains_SmallList | 42 | 13.562 ns | 0.0651 ns | 0.0609 ns |  1.17 |         - |          NA |

## Contributing

Any contibutions are greatly appreciated.
Just fork the project, create a new feature branch, commit and push your changes and open a pull request.

## License

Distributed under the MIT License. See LICENSE for more information.
