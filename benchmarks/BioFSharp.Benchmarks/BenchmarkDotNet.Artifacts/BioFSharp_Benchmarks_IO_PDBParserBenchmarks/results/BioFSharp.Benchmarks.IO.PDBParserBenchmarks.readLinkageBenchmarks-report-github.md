```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method             | Mean        | Error      | StdDev       | Median      |
|------------------- |------------:|-----------:|-------------:|------------:|
| readLinkageNormal  |    12.09 ms |   0.227 ms |     0.189 ms |    12.05 ms |
| readLinkageExtreme | 5,268.24 ms | 811.987 ms | 2,394.163 ms | 3,830.67 ms |
