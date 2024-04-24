# Oqtane.State

This is a sample project which demonstrates the techniques utilized in the Oqtane Framework (https://www.oqtane.org) for sharing state in components across static and interactive render modes. 

Traditionally when developing in Interactive Blazor, the two most common ways to manage state within an application were by using Cascading Parameters or Scoped Services. These techniques were simple to implement and provided a consistent developer experience across your entire Blazor application. 

In .NET 8, Microsoft introduced Static Blazor which completely changed the approach for developing Blazor applications. In Static Blazor, components are now statically rendered by default and you also have the ability to render some components interactively based on your specific requirements. This new approach resulted in some challenges for state management as static and interactive components utilize completely different process models. 

Specifically, the state held in Cascading Parameters or Scoped Services does not flow automatically across render mode boundaries. If your application loaded some state on the server it could be accessed within your static components - but if you tried to access it within your interactive components, it would be null.

IF we take a step back and consider the typical usage patterns for state within Blazor applications, by far the most common pattern was to load some state early in the application life cycle so that it could be utilized within your downstream components. Cascading Parameters and Scoped Services acted as convenient containers to hold the state, and were essentially being used as a read-only immutable cache for the current user session. And if we focus on this common scenario, there is a simple way to allow Blazor to support Cascading Parameters and Scoped Services in .NET 8.

Basically we need to manually transfer the state across the render mode boundary. Blazor has always provided a native capability for passing information from one component to another - Parameters. The most important thing to note when passing state across process boundaries is that values must be serializable. 

---------------

This sample project was originally created using the standard Blazor Web template (with WebAssembly chosen for Interactivity). We have a SiteState model which is defined as a Scoped Service in Program.cs (client and server projects) and is initialized in App.razor. We also have a PageState model which is defined as a Cascading Parameter and initialized in Routes.razor. 

Since the Counter page is an interactive Blazor component we are going to focus on how to enable state management within this page. This is going to require us to split the original Counter.razor page component into 2 parts - a page component and a standard component. 

![image](https://github.com/sbwalker/Oqtane.State/assets/4840590/1f470121-6630-48fd-9f4f-17aabee68a4d)

The Counter page component is located in the Pages folder and executes on the static side of the render mode boundary. It contains the @page route directive so that the Blazor router can find it. It also injects the SiteState service and includes the PageState cascading parameter. You will notice that the content of the page component has been replaced with a new component named RenderModeBoundary which includes some parameters for ComponentType, SiteState, and PageState. Essentially these parameters are passing values as serializable parameters to the RenderModeBoundary component. It also includes an @rendermode property which indicates that the RenderModeBoundary component should be rendered interactively (using WebAssembly by default but can be changed to Server or Auto).

```
@using Oqtane.State.Client.Models
@using Oqtane.State.Client.Components
@page "/counter"
@inject SiteState SiteState

<PageTitle>Counter</PageTitle>

<RenderModeBoundary 
    ComponentType="Oqtane.State.Client.Components.Counter" 
    SiteState="@SiteState" 
    PageState="@PageState" 
    @rendermode="RenderMode.InteractiveWebAssembly" />

@code {
    // this component is on the static side of the render mode boundary

    [CascadingParameter]
    public PageState PageState { get; set; }
}
```

The RenderModeBoundary component is located in the Components folder and it executes on the interactive side of the render mode boundary. It is a "shim" component which accepts the parameter values which are passed to it by the parent component. The PageState value is assigned to a new cascading parameter which will be available to all downstream interactive components. The SiteState value is used to hydrate the SiteState service (named ComponentSiteState for clarity) which will be available to all downstream interactive components. The ComponentType string is used to dynamically create the Counter component (note that a string is used because a Type is not serializable).

```
@using Oqtane.State.Client.Models
@inject SiteState ComponentSiteState

<CascadingValue Value="@PageState">
    <DynamicComponent Type="@Type.GetType(ComponentType)"></DynamicComponent>
</CascadingValue>

@code {
    // this component is on the interactive side of the render mode boundary
    // it receives state as serializable parameters and makes it available to downstream components

    [Parameter]
    public string ComponentType { get; set; }

    [Parameter]
    public SiteState SiteState { get; set; }

    [Parameter]
    public PageState PageState { get; set; }

    protected override void OnParametersSet()
    {
        // hydrate the SiteState service based on the values passed in the SiteState parameter
        // hydration is required as you need to copy the values rather than the reference
        // this is how Scoped Service state is marshalled across the render mode boundary
        ComponentSiteState.Hydrate(SiteState);
    }
}
```

The Counter standard component is located in the Components folder and it executes on the interactive side of the render mode boundary (inheriting the render mode from the RenderModeBoundary component). It includes references to the (interactive) SiteState service and the (interactive) PageState cascading parameter.

```
@using Oqtane.State.Client.Models
@inject SiteState SiteState

<h1>Counter</h1>

<div>Cascading Parameter State: @PageState.MyProperty</div>
<div>Scoped Service State: @SiteState.RemoteIPAddress</div>

<br />
<p role="status">Current count: @currentCount</p>

<button class="btn btn-primary" @onclick="IncrementCount">Increase Counter</button>

@code {
    // this component is on the interactive side of the render mode boundary
    // it consumes Scoped Services and Cascading Parameters using the standard approach

    [CascadingParameter]
    public PageState PageState { get; set; }

    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```

When you run the application you can see how the SiteState and PageState values are seamlessly transferred from the static page component to the interactive standard component. Obviously this approach requires some ceremony to implement, but it is only required for scenarios which need to cross the render mode boundary. The logic could easily be abstracted into a base class or service to simplify the implementation in a larger application. And downstream components can rely on the exact same cascading parameters or scoped services regardless of the render mode.

Obviously, this solution will only work for the common scenario outlined above where state is essentially being used as a read-only immutable cache. If you have more advanced requirements such as the need to mutate state or notify other components of state changes, you will need a more elaborate solution. In those cases, I would suggest you read Rocky Lhotka's blog: https://blog.lhotka.net/2023/11/28/Per-User-Blazor-8-State

![image](https://github.com/sbwalker/Oqtane.State/assets/4840590/f7229fc5-3925-4aa0-bba8-9465e69d10fa)




