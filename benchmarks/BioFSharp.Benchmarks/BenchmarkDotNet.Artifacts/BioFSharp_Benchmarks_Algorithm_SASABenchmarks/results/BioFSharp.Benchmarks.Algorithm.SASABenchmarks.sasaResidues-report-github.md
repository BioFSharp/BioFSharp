```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method            | Mean         | Error      | StdDev       | Median       |
|------------------ |-------------:|-----------:|-------------:|-------------:|
| SASAresidue_small |     92.81 ms |   1.825 ms |     2.173 ms |     92.57 ms |
| SASAresidue_big   | 21,605.20 ms | 874.360 ms | 2,578.071 ms | 20,027.79 ms |
