# Cogito.Autofac

Attribute-driven registration for [Autofac](https://autofac.org/), plus a convention for composing an
application out of assemblies rather than a central wiring file.

## Why

Autofac's fluent API puts every registration in one place, far from the type it registers. As an
application grows that file becomes the thing everyone edits and nobody owns. This package lets a type
declare how it is registered, and lets each assembly contribute its own module, so composition follows
the code.

## Install

```shell
dotnet add package Cogito.Autofac
```

## Registering by attribute

Mark a type with `[RegisterAs]` and whatever lifetime and modifier attributes apply, then scan for them:

```csharp
[RegisterAs(typeof(IGreeter))]
[RegisterSingleInstance]
public class Greeter : IGreeter
{
    public string Greet(string name) => $"Hello, {name}.";
}

var builder = new ContainerBuilder();
builder.RegisterFromAttributes(typeof(Greeter).Assembly);
var container = builder.Build();
```

The attributes cover the registration options you would otherwise write fluently — among them
`RegisterAs`, `RegisterType`, `RegisterSingleInstance`, `RegisterInstancePerLifetimeScope`,
`RegisterNamed`, `RegisterKeyed`, `RegisterExternallyOwned`, `RegisterOrder`, `RegisterPriority`,
`RegisterAggregateService` and `RegisterWithAttributeFiltering`.

For registrations that cannot be expressed declaratively, implement `IRegistrationBuilderAttribute` or
`IRegistrationRootAttribute` and the scanner will call your code.

## Composing across assemblies

Derive from `ModuleBase` in each assembly that has something to contribute:

```csharp
public class AssemblyModule : ModuleBase
{
    protected override void Register(ContainerBuilder builder)
    {
        builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
    }
}
```

and load every one of them that the dependency context can see:

```csharp
builder.RegisterAllAssemblyModules();
```

Each module is registered once no matter how many times it is reached, so modules can safely depend on
each other.

## Also here

- `OrderedRegistrationExtensions` / `OrderedResolutionExtensions` — resolve a set of services in a
  declared order rather than registration order.
- `LifetimeScopeContext` — flow the current lifetime scope through a logical call context.
- `ComponentContextExtensions`, `ContainerBuilderExtensions` — smaller helpers over the Autofac API.

## License

MIT.
