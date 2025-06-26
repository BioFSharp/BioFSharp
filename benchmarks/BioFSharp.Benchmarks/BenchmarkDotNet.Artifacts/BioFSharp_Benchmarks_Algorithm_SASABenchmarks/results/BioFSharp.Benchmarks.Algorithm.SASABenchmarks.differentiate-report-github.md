```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method                              | Mean        | Error     | StdDev    |
|------------------------------------ |------------:|----------:|----------:|
| differentiateExposedandBuried_small |    113.6 ms |   1.35 ms |   1.33 ms |
| differentiateExposedandBuried_big   | 19,318.5 ms | 232.26 ms | 181.33 ms |
