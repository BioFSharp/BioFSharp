```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method                 | Mean     | Error    | StdDev    | Median   |
|----------------------- |---------:|---------:|----------:|---------:|
| scaleTespoints_rubisco | 25.20 μs | 1.542 μs |  4.400 μs | 24.90 μs |
| scaleTestpoints_htq    | 47.38 μs | 4.228 μs | 12.332 μs | 41.90 μs |
