```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method         | Mean         | Error      | StdDev       | Median       |
|--------------- |-------------:|-----------:|-------------:|-------------:|
| SASAatom_small |     98.32 ms |   1.966 ms |     4.104 ms |     97.52 ms |
| SASAatom_big   | 21,489.02 ms | 887.143 ms | 2,615.760 ms | 19,906.27 ms |
