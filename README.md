# Cogito.Autofac

[![Build](https://github.com/alethic/Cogito.Autofac/actions/workflows/Cogito.Autofac.yml/badge.svg)](https://github.com/alethic/Cogito.Autofac/actions/workflows/Cogito.Autofac.yml)

Attribute-driven registration for Autofac, and a convention for composing an application out of assemblies rather than one central wiring file.

## Packages

**[Cogito.Autofac](https://www.nuget.org/packages/Cogito.Autofac)** — Attribute-driven registration for [Autofac](https://autofac.org/), plus a convention for composing an application out of assemblies rather than a central wiring file.

**[Cogito.Autofac.DependencyInjection](https://www.nuget.org/packages/Cogito.Autofac.DependencyInjection)** — Bridges [Autofac](https://autofac.org/) and `Microsoft.Extensions.DependencyInjection` in both directions, so a container built with Autofac can host services registered the Microsoft way, and vice versa.

Each package carries its own README with the detail; the links above go to nuget.org.

## Building

```shell
dotnet restore Cogito.Autofac.sln
dotnet msbuild -p:Configuration=Release Cogito.Autofac.dist.msbuildproj
```

Packages are staged into `dist/nuget` and test suites into `dist/tests`; run a suite with
`dotnet test -f <tfm> <path to its assembly>`.

## License

MIT — see [LICENSE](LICENSE).
