```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method           | Mean         | Error       | StdDev        | Median       |
|----------------- |-------------:|------------:|--------------:|-------------:|
| readChainNormal  |     9.541 ms |   0.1432 ms |     0.1339 ms |     9.542 ms |
| readChainExtreme | 4,835.700 ms | 931.2838 ms | 2,745.9116 ms | 2,933.190 ms |
