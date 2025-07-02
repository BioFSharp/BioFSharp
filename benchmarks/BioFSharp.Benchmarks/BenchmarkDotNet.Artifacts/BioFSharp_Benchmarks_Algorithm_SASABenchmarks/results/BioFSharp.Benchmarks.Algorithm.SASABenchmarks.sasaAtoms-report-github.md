```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method           | Mean         | Error        | StdDev       | Median       |
|----------------- |-------------:|-------------:|-------------:|-------------:|
| SASAatom_rubisco |     96.25 ms |     1.122 ms |     0.937 ms |     95.98 ms |
| SASAatom_htq     | 21,380.82 ms | 1,057.647 ms | 3,101.895 ms | 19,950.50 ms |
