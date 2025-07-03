```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method             | Mean         | Error       | StdDev    | Gen0       | Allocated    |
|------------------- |-------------:|------------:|----------:|-----------:|-------------:|
| ReadPDBFileNormal  |     284.6 μs |     3.21 μs |   2.84 μs |   239.7461 |    490.57 KB |
| ReadPDBFileExtreme | 109,713.9 μs | 1,029.55 μs | 859.72 μs | 86400.0000 | 176701.69 KB |
