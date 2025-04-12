
![Logo](docs/img/Logo_large.png)

[![Made with F#](https://img.shields.io/badge/Made%20with-FSharp-rgb(184,69,252).svg)](https://fsharp.org/)
[![Nuget](https://img.shields.io/nuget/v/BioFSharp?label=nuget(stable))](https://www.nuget.org/packages/BioFSharp/)
[![Nuget](https://img.shields.io/nuget/vpre/BioFSharp?label=nuget(prerelease))](https://www.nuget.org/packages/BioFSharp/)
![GitHub contributors](https://img.shields.io/github/contributors/BioFSharp/BioFSharp)

BioFSharp is an open source bioinformatics and computational biology toolbox written in F#. https://biofsharp.com/BioFSharp

| Build status (ubuntu and windows) | Test Coverage |
|---|---|
| ![](https://github.com/CSBiology/BioFSharp/actions/workflows/build-test.yml/badge.svg) | [![codecov](https://codecov.io/gh/BioFSharp/BioFSharp/branch/developer/graph/badge.svg)](https://codecov.io/gh/BioFSharp/BioFSharp) |

## Overview

BioFSharp provides a type models, readers, writers and algorithms for various domains across bioinformatics and computational biology.

This repo contains the core (types, IO, and some algorithms)

BioFSharp has an ecosystem of extension packages, which include more sophisticated statistical analysis and ML.

Check out all repos [here](https://github.com/BioFSharp)

## Documentation

Functions, types and Classes contained in BioFSharp come with short explanatory description, which can be found in the [API Reference](https://biofsharp.com/BioFSharp/reference/index.html).

More indepth explanations, tutorials and general information about the project can be found [here](http://biofsharp.com/BioFSharp).

The documentation and tutorials for this library are generated from scripts and notebooks in the [docs folder](./docs).

## Contributing

Every contribution is welcome, such as:

- Bug reports
- Feature requests
- Documentation improvement requests
- Typo fixes
- Performance discussions and improvements
- New alogithm implementations
- etc.

**Please start by opening an issue**.

**Check the [Development section](#development)** for general guidance on the codebase

**Pull Requests should target the `main` branch from a forked version of the repo.**

This is an **open source project** created as the result of scientific teaching and research efforts.
Please follow the [Code of Conduct](CODE_OF_CONDUCT.md) and refrain from unrealistic expectations from maintainers.

## Development

### General

BioFSharp repositories usually folllow this structure:

```
root
│   📄<project name>.sln
│   📄build.cmd
│   📄build.sh
├───📁build
├───📁docs
├───📁src
|   └───📁<project name>
└───tests
    └───📁<testproject name>
```

- <project name>.sln is the root solution file.
- `build` contains a [FAKE](https://fake.build/) build project with targets for building, testing and packaging the project.
- `build/sh` and `build.cmd` in the root are shorthand scripts to execute the buildproject.
- `docs` contains the documentation in form of literate scripts and notebooks. 
- `src` contains folders with the source code of the project(s).
- `tests` contains folders with test projects.

### Build

just call `build.sh` or `build.cmd` depending on your OS.

### Test

```bash
build.sh runtests
```

```bash
build.cmd runtests
```

### Create Nuget package

```bash
build.sh pack
```
```bash
build.cmd pack
```

### Docs

You can watch locally with hot reload via

```bash
build.sh watchdocs
```
```bash
build.cmd watchdocs
```