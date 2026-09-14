# Cogito.Autofac.DependencyInjection

Bridges [Autofac](https://autofac.org/) and `Microsoft.Extensions.DependencyInjection` in both
directions, so a container built with Autofac can host services registered the Microsoft way, and
vice versa.

## Why

The two containers meet awkwardly. `Populate` on a root `ContainerBuilder` is easy to get wrong —
singletons registered through the service collection end up owned by the root scope even when you
wanted them owned by a child. This package gives you a `Populate` that takes a configuration callback
and an explicit lifetime-scope tag for singletons, plus an `AddAutofac` for going the other way.

## Install

```shell
dotnet add package Cogito.Autofac.DependencyInjection
```

## Autofac hosting Microsoft registrations

```csharp
var builder = new ContainerBuilder();

builder.Populate(services =>
{
    services.AddLogging();
    services.AddHttpClient();
});

var container = builder.Build();
```

Pass a `lifetimeScopeTagForSingletons` when the singletons should belong to a tagged child scope
rather than the root:

```csharp
builder.Populate(services => services.AddMyThings(), lifetimeScopeTagForSingletons: "request");
```

## Microsoft hosting Autofac registrations

```csharp
services.AddAutofac(builder => builder.RegisterAllAssemblyModules());
```

## As the host's service provider factory

`AutofacServiceProviderFactory` plugs into the generic host, which is the usual entry point for a
Cogito application:

```csharp
Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(b => b.RegisterAllAssemblyModules())
    .Build()
    .Run();
```

## License

MIT.
