# ReactiveUI.Fody
C# Fody extension to generate RaisePropertyChange notifications for properties and ObservableAsPropertyHelper   properties.

Eases the need for boilerplate in your view models when using [reactiveui](https://github.com/reactiveui/ReactiveUI).  Typically, 
in your view models you must declare properties like this:

    string _SearchId;
    
    [DataMember]
    public string SearchId {
        get { return _SearchId; }
        set { this.RaiseAndSetIfChanged(ref _SearchId, value); }
    }

This is tedious since all you'd like to do is declare properties as normal:

    [DataMember]public string Search { get; set; }
    
This fody plugin will weave the former into your output based on the simple auto-property declaration you provide.  Similarly, 
in order to handle observable property helper properties, you must declare them like this:

    ObservableAsPropertyHelper<string> _PersonInfo;
    
    public string PersonInfo {
        get { return _PersonInfo.Value; }
    }

Then elsewhere you'd set it up via:

    ...
    .ToProperty(this, x => x.PersonInfo, out _PersonInfo);

This plugin will instead allow you to declare the property like:

    [ObservableAsProperty]public extern string PersonInfo { get; }
    
It will generate the field and implement the property for you.  Because there is no field for you to pass to `.ToProperty`, you 
should use the provided `.ToPropertyEx` extension method provided by this library:

    ...
    .ToProperty(this, x => x.PersonInfo);
    
This extension will assign the auto-generated field for you rather than relying on the `out` parameter.
