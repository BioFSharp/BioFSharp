```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method                  | Mean        | Error      | StdDev       | Median      |
|------------------------ |------------:|-----------:|-------------:|------------:|
| acessiblePoints_rubisco |    54.47 ms |   0.998 ms |     1.667 ms |    54.08 ms |
| acessiblePoints_htq     | 2,959.38 ms | 589.760 ms | 1,624.372 ms | 2,215.01 ms |
