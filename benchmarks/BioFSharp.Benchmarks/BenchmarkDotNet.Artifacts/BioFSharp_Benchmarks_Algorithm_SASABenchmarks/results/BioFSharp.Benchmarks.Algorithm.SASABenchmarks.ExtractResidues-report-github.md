```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method                  | Mean         | Error      | StdDev       | Median       | Gen0         | Gen1        | Gen2      | Allocated  |
|------------------------ |-------------:|-----------:|-------------:|-------------:|-------------:|------------:|----------:|-----------:|
| extractTestdata_rubisco |     36.12 ms |   1.695 ms |     4.863 ms |     33.91 ms |    2000.0000 |   1000.0000 |         - |    13.5 MB |
| extractTestdata_htq     | 16,194.17 ms | 697.103 ms | 2,044.483 ms | 16,316.95 ms | 1143000.0000 | 185000.0000 | 6000.0000 | 5120.97 MB |
