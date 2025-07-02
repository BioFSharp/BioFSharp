```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4482/24H2/2024Update/HudsonValley)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2 DEBUG
  DefaultJob : .NET 8.0.17 (8.0.1725.26602), X64 RyuJIT AVX2


```
| Method               | Mean         | Error      | StdDev       | Median       |
|--------------------- |-------------:|-----------:|-------------:|-------------:|
| extractAtoms_rubisco |     33.10 ms |   0.470 ms |     0.393 ms |     32.94 ms |
| extractAtoms_htq     | 15,290.48 ms | 877.718 ms | 2,587.971 ms | 16,606.93 ms |
