# NuFlexiArch + JSVaporizer

Welcome to a minimal-yet-powerful .NET WebAssembly toolkit that unifies **NuFlexiArch** (transformable DTOs, dynamic components) with **JSVaporizer** (structured .NET↔JS interop). This combination aims to give you fine-grained control over DOM manipulation, JSON-based data transformations, and reflection-driven UI components—all while keeping the codebase lean and explicit.

---

## Table of Contents

1. [Overview](#overview)
2. [Distinctive Features](#distinctive-features)
   1. [Ephemeral `JSObject` Management](#ephemeral-jsobject-management)
   2. [Dictionary-Based Callback Pools](#dictionary-based-callback-pools)
   3. [NuFlexiArch Transformer Registry](#nuflexiarch-transformer-registry)
   4. [Reflection-Based Dynamic Component Creation](#reflection-based-dynamic-component-creation)
   5. [Source-Generated JSON + AOT Friendliness](#source-generated-json--aot-friendliness)
   6. [Minimal, Modular DOM Wrapping](#minimal-modular-dom-wrapping)
   7. [Two-Fold Architecture for UI + Data](#two-fold-architecture-for-ui--data)
3. [High-Level Workflow](#high-level-workflow)
4. [Usage Examples](#usage-examples)
5. [Project Structure](#project-structure)
6. [Future Directions](#future-directions)
7. [License](#license)

---

## Overview

- **NuFlexiArch**:
  - Provides an extensible system for converting JSON to DTOs, generating “view” strings from those DTOs, and reflecting them into UI components.  
  - Features a dynamic component model (`AComponent`, `IComponentRenderer`, `CompStateDto`) with custom metadata and state serialization (powered by source-generated `System.Text.Json`).

- **JSVaporizer**:
  - Simplifies calls between .NET and JavaScript in WebAssembly environments by combining `[JSImport]` and `[JSExport]` into easy-to-use partial classes.  
  - Manages ephemeral `JSObject` references so you don’t have to worry about disposing them manually.  
  - Provides dictionary-based pooling for function callbacks, allowing dynamic event binding.

When fused, these libraries let you define or instantiate UI components on the fly (from JSON), manipulate DOM elements directly from C#, handle events with minimal boilerplate, and store or transform data in a structured, AOT-friendly way.

---

## Distinctive Features

### 1. Ephemeral `JSObject` Management

**JSVaporizer** automatically disposes the underlying `JSObject` each time you interact with a DOM element if it’s already connected to the DOM. This prevents memory bloat (from leftover JS objects) and lowers the risk of accidentally referencing stale DOM pointers. You get:

- **On-demand** fetch-and-dispose strategy: each call to `Document.GetElementById(...)` returns a fresh `JSObject`, which JSVaporizer disposes shortly after use.  
- **Reduced risk** of memory leaks and easier debugging of interop code.

### 2. Dictionary-Based Callback Pools

Rather than requiring a dedicated `[JSExport]` method for each JavaScript callback, JSVaporizer stores delegates in dictionaries keyed by simple strings:

- **Dynamic event handler registration**: attach or detach event callbacks at runtime without generating new exported methods.  
- **Reduced clutter**: keep your code DRY by avoiding repetitious “one export per callback” patterns.  
- **Easy debugging**: track function keys in a single dictionary for clarity on which handlers are active.

### 3. NuFlexiArch Transformer Registry

**NuFlexiArch** introduces a “transformer” concept, represented by `ITransformer` and `TransformerDto`. The registry (`ITransformerRegistry`) lets you:

- **Register** multiple transformers under different keys.  
- **Invoke** them at runtime (`Invoke(...)`) to convert JSON → DTO → (optional) view string.  
- Swap or upgrade transformation logic without altering the main code, just by pointing to a different transformer key.

### 4. Reflection-Based Dynamic Component Creation

**IJSVComponent** provides a mechanism to **reflectively construct** components based on metadata (e.g., type name) and a unique prefix:

- **`InitializeFromJson(...)`** method:  
  1. Deserializes JSON metadata to determine the component’s type and prefix.  
  2. Instantiates the component with reflection.  
  3. Deserializes and applies the component’s state.  
- Ideal for building dynamic UIs that can be specified or updated purely via JSON.

### 5. Source-Generated JSON + AOT Friendliness

All major classes (`CompStateDto`, `ComponentMetadata`, etc.) are annotated with `[JsonSerializable(...)]` partial contexts:

- **Compile-time** generation of serialization code reduces reflection overhead.  
- Works well with **.NET WASM AOT** scenarios, ensuring smaller binaries and faster startup times.

### 6. Minimal, Modular DOM Wrapping

JSVaporizer doesn’t attempt to replicate the entire DOM or create a full component framework. Instead, it offers:

- **`Document`** and **`Element`** classes for essential DOM operations (`AppendChild`, `SetProperty`, `GetAttribute`, etc.).  
- **Event Hooks** via `AddEventListener`, which ties in with the dictionary-based delegate pool.

This is perfect if you want a light approach to raw DOM control rather than adopting a comprehensive framework like Blazor or Angular.

### 7. Two-Fold Architecture for UI + Data

- **NuFlexiArch**: Helps define a “data-driven UI” approach where each component has state (DTO) and metadata.  
- **JSVaporizer**: Provides the actual interop channels for updating DOM elements, hooking up event handlers, or calling JS functions from .NET.  

Together, they streamline end-to-end flows: *json* → *component instantiation* → *render DOM* → *manipulate DOM / handle events in .NET* → *synchronize state*.

---

## High-Level Workflow

1. **Register Transformers**: In your .NET code, populate the `TransformerRegistry` with one or more **`JSVTransformer`** subclasses.  
2. **Create/Lookup Components**: Either create an `AComponent` subclass (e.g., `JSVTextInput`) directly or reflect it using `InitializeFromJson(...)`.  
3. **Render (Optional)**: If server-side HTML generation is used, call your `IComponentRenderer` to produce markup.  
4. **DOM Interop**: On the client side, rely on JSVaporizer’s `Document` and `Element` wrappers to set properties, attach event listeners, etc.  
5. **Call Transformers**: If you need to transform data (like a DTO) to a “view” string or back, invoke the registry’s `DtoToView()` or `JsonToDto()` methods.  
6. **Handle State**: Store, serialize, or reload component state as needed, always referencing the source-generated JSON contexts for performance.

---

## Usage Examples

### Registering a Transformer

```csharp
var registry = new TransformerRegistry(new Dictionary<string, JSVTransformer>
{
    ["myKey"] = new MyCustomTransformer(),
    ["anotherKey"] = new AnotherTransformer()
});

// Now registry.Get("myKey") gives you MyCustomTransformer
```

### Invoking a Transformer

```csharp
string dtoJson = "...";
string userInfoJson = "...";
string htmlView = TransformerRegistry.Invoke(registry, "myKey", dtoJson, userInfoJson);
// Produces a view string via DtoToView
```

### Creating a Component Programmatically

```csharp
var textInput = new JSVTextInput("TextInputPrefix", new TextInputRenderer());
var stateDto = new TextInputStateDto { LabelValue = "Hello", InputValue = "World" };
textInput.SetState(stateDto);

// If you need server-side rendering:
await textInput.GetRenderer().RenderAsync(textInput, htmlHelper, new HtmlContentBuilder());
```

### Initializing a Component from JSON (Reflection)

```csharp
// Suppose you have metadataJson and stateDtoJson from somewhere
bool success = IJSVComponent.InitializeFromJson(metadataJson, stateDtoJson);
```

Or from JavaScript (if `[JSExport]` is used):

```csharp
Module.JSVComponentInitializer.InitializeFromJson(metadataJson, stateDtoJson);
```
### Project Structure

```csharp
NuFlexiArch/
├── ITransformer.cs, TransformerDto.cs, ITransformerRegistry.cs
├── IComponent.cs, AComponent.cs, CompStateDto.cs, ...
├── <JsonSerializerContext classes>
└── ...

JSVNuFlexiArch/
├── JSVTransformer.cs
├── TransformerRegistry.cs
├── IJSVComponent.cs, JSVComponentRenderer.cs
└── ...

MyViewLib/
├── JSVComponentInitializer.cs
├── ATextInput.cs, JSVTextInput.cs, TextInputRenderer.cs
└── ...

JSVaporizer/
├── JSVapor.cs (public static partial classes for Imports/Exports)
├── Element.cs
├── Document.cs
├── <Other partial classes>
└── ...
```
