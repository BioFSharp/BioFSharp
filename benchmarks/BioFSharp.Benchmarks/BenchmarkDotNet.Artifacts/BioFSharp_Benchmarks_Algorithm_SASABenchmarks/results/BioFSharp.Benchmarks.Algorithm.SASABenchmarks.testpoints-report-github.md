```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method           | Mean         | Error        | StdDev       | Median       |
|----------------- |-------------:|-------------:|-------------:|-------------:|
| testpoints_small |     393.4 ns |      5.22 ns |      4.36 ns |     391.6 ns |
| testpoints_big   | 965,125.0 ns | 30,732.86 ns | 84,647.22 ns | 933,826.4 ns |
