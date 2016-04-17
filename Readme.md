# ReactiveUI.Fody

[![Windows Build Status]](https://ci.appveyor.com/project/KirkWoll/reactiveui-fody)

C# Fody extension to generate RaisePropertyChange notifications for properties and ObservableAsPropertyHelper properties.

## Install ##
Nuget package ReactiveUI.Fody:

> Install-Package ReactiveUI.Fody

Currently, you need to manually add `<ReactiveUI />` to your Fody weavers configuration. If this is your first Fody plugin then `FodyWeavers.xml` should look like this after the change:

    <?xml version="1.0" encoding="utf-8" ?>
    <Weavers>
        <ReactiveUI />
    </Weavers>

##Reactive Properties##

Eases the need for boilerplate in your view models when using [reactiveui](https://github.com/reactiveui/ReactiveUI).  Typically, in your view models you must declare properties like this:

    string _SearchId;
    
    public string SearchId 
    {
        get { return _SearchId; }
        set { this.RaiseAndSetIfChanged(ref _SearchId, value); }
    }

This is tedious since all you'd like to do is declare properties as normal:

    [Reactive]public string SearchId { get; set; }
    
If a property is annotated with the `[Reactive]` attribute, the plugin will weave the boilerplate into your 
output based on the simple auto-property declaration you provide.  

##ObservableAsPropertyHelper Properties

Similarly, in order to handle observable property helper properties, you must declare them like this:

    ObservableAsPropertyHelper<string> _PersonInfo;
    
    public string PersonInfo 
    {
        get { return _PersonInfo.Value; }
    }

Then elsewhere you'd set it up via:

    ...
    .ToProperty(this, x => x.PersonInfo, out _PersonInfo);

This plugin will instead allow you to declare the property like:

    public extern string PersonInfo { [ObservableAsProperty]get; }
    
It will generate the field and implement the property for you.  Because there is no field for you to pass to
`.ToProperty`, you should use the `.ToPropertyEx` extension method provided by this library:

    ...
    .ToPropertyEx(this, x => x.PersonInfo);
    
This extension will assign the auto-generated field for you rather than relying on the `out` parameter.

[Windows Build Status]: https://ci.appveyor.com/api/projects/status/github/kswoll/ReactiveUI.Fody?svg=true
