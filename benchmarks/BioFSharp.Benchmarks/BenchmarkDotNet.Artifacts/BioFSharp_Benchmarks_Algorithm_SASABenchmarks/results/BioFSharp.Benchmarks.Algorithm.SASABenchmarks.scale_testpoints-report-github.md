```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method               | Mean     | Error    | StdDev    | Median   |
|--------------------- |---------:|---------:|----------:|---------:|
| scaleTespoints_small | 26.28 μs | 0.799 μs |  2.253 μs | 25.95 μs |
| scaleTestpoints_big  | 49.10 μs | 3.925 μs | 11.386 μs | 43.50 μs |
