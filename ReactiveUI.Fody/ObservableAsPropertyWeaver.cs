using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ReactiveUI.Fody
{
    public class ObservableAsPropertyWeaver
    {
        public ModuleDefinition ModuleDefinition { get; set; }

        // Will log an MessageImportance.High message to MSBuild. OPTIONAL
        public Action<string> LogInfo  { get; set; }

        public void Execute()
        {
            var reactiveUI = ModuleDefinition.FindAssembly("ReactiveUI");
            if (reactiveUI == null)
                throw new Exception("Could not find assembly: ReactiveUI (" + string.Join(", ", ModuleDefinition.AssemblyReferences.Select(x => x.Name)) + ")");
            var helpers = ModuleDefinition.AssemblyReferences.SingleOrDefault(x => x.Name == "ReactiveUI.Fody.Helpers");
            if (helpers == null)
                throw new Exception("Could not find assembly: ReactiveUI.Fody.Helpers (" + string.Join(", ", ModuleDefinition.AssemblyReferences.Select(x => x.Name)) + ")");

            var reactiveObject = ModuleDefinition.FindType("ReactiveUI", "ReactiveObject", reactiveUI);

            // The types we will scan are subclasses of ReactiveObject
            var targetTypes = ModuleDefinition.Types.Where(x => x.BaseType != null && reactiveObject.IsAssignableFrom(x.BaseType));

            var observableAsPropertyHelper = ModuleDefinition.FindType("ReactiveUI", "ObservableAsPropertyHelper`1", reactiveUI, "T");
            var observableAsPropertyAttribute = ModuleDefinition.FindType("ReactiveUI.Fody.Helpers", "ObservableAsPropertyAttribute", helpers);
            var observableAsPropertyHelperGetValue = ModuleDefinition.Import(observableAsPropertyHelper.Resolve().Properties.Single(x => x.Name == "Value").GetMethod);

            foreach (var targetType in targetTypes)
            {
                foreach (var property in targetType.Properties.Where(x => x.IsDefined(observableAsPropertyAttribute)).ToArray())
                {
                    var genericObservableAsPropertyHelper = observableAsPropertyHelper.MakeGenericInstanceType(property.PropertyType);
                    var genericObservableAsPropertyHelperGetValue = observableAsPropertyHelperGetValue.Bind(genericObservableAsPropertyHelper);
                    ModuleDefinition.Import(genericObservableAsPropertyHelperGetValue);

                    // Declare a field to store the property value
                    var field = new FieldDefinition("$" + property.Name, FieldAttributes.Private, genericObservableAsPropertyHelper);
                    targetType.Fields.Add(field);

                    // We're re-implementing the getter since the original definition was an extern method.  So remove that
                    // stub.
                    targetType.Methods.Remove(property.GetMethod);

                    property.GetMethod.Body = new MethodBody(property.GetMethod);
                    property.GetMethod.Body.Emit(il =>
                    {
                        il.Emit(OpCodes.Ldarg_0);                                               // this
                        il.Emit(OpCodes.Ldfld, field);                                          // pop -> this.$PropertyName
                        il.Emit(OpCodes.Callvirt, genericObservableAsPropertyHelperGetValue);   // pop -> this.$PropertyName.Value
                        il.Emit(OpCodes.Ret);                                                   // Return the value that is on the stack
                    });
                    targetType.Methods.Add(property.GetMethod);
                }
            }
        }         
    }
}