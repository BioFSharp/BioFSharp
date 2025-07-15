```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method              | Mean         | Error       | StdDev      |
|-------------------- |-------------:|------------:|------------:|
| readMetadataNormal  |     635.4 μs |    11.80 μs |    13.12 μs |
| readMetadataExtreme | 157,876.4 μs | 3,148.89 μs | 5,757.92 μs |
