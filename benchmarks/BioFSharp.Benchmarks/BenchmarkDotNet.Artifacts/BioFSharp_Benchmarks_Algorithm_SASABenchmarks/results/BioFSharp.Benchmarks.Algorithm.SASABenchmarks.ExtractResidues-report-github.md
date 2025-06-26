```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method                | Mean         | Error      | StdDev     | Gen0         | Gen1        | Gen2      | Allocated  |
|---------------------- |-------------:|-----------:|-----------:|-------------:|------------:|----------:|-----------:|
| extractTestdata_small |     36.09 ms |   0.674 ms |   0.563 ms |    2733.3333 |   1266.6667 |  666.6667 |   13.49 MB |
| extractTestdata_big   | 16,869.82 ms | 293.599 ms | 274.633 ms | 1152000.0000 | 186000.0000 | 6000.0000 | 5120.88 MB |
