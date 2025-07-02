```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method             | Mean        | Error      | StdDev       | Median      |
|------------------- |------------:|-----------:|-------------:|------------:|
| readResidueNormal  |    10.28 ms |   0.130 ms |     0.121 ms |    10.24 ms |
| readResidueExtreme | 6,268.33 ms | 805.437 ms | 2,374.850 ms | 4,504.45 ms |
