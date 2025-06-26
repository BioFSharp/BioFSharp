```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method          | Mean          | Error        | StdDev       | Median        |
|---------------- |--------------:|-------------:|-------------:|--------------:|
| MetadataSmall   |      45.13 μs |     0.511 μs |     1.205 μs |      44.95 μs |
| MetadataNormal  |     510.20 μs |     5.576 μs |     6.848 μs |     509.19 μs |
| MetadataLarge   |   7,841.45 μs |   210.565 μs |   614.229 μs |   7,853.14 μs |
| MetadataExtreme | 156,266.38 μs | 2,980.505 μs | 5,813.247 μs | 153,500.25 μs |
