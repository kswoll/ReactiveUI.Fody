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
            var reactiveUI = ModuleDefinition.AssemblyReferences.SingleOrDefault(x => x.Name == "ReactiveUI");
            if (reactiveUI == null)
                throw new Exception("Could not find assembly: ReactiveUI (" + string.Join(", ", ModuleDefinition.AssemblyReferences.Select(x => x.Name)) + ")");
            var helpers = ModuleDefinition.AssemblyReferences.SingleOrDefault(x => x.Name == "ReactiveUI.Fody.Helpers");
            if (helpers == null)
                throw new Exception("Could not find assembly: ReactiveUI.Fody.Helpers (" + string.Join(", ", ModuleDefinition.AssemblyReferences.Select(x => x.Name)) + ")");
            var reactiveObject = new TypeReference("ReactiveUI", "ReactiveObject", ModuleDefinition, reactiveUI);

            var targetTypes = ModuleDefinition.Types.Where(x => x.BaseType != null && reactiveObject.IsAssignableFrom(x.BaseType));

            var observableAsPropertyHelper = new TypeReference("ReactiveUI", "ObservableAsPropertyHelper`1", ModuleDefinition, reactiveUI);
            observableAsPropertyHelper.GenericParameters.Add(new GenericParameter("T", observableAsPropertyHelper));
            var observableAsPropertyAttribute = new TypeReference("ReactiveUI.Fody.Helpers", "ObservableAsPropertyAttribute", ModuleDefinition, helpers);
//            var observableAsPropertyAttribute = ModuleDefinition.Import(typeof(ObservableAsPropertyAttribute));
            var observableAsPropertyHelperGetValue = ModuleDefinition.Import(observableAsPropertyHelper.Resolve().Properties.Single(x => x.Name == "Value").GetMethod);

            foreach (var targetType in targetTypes)
            {
                foreach (var property in targetType.Properties.Where(x => x.IsDefined(observableAsPropertyAttribute)).ToArray())
                {
                    var genericObservableAsPropertyHelper = observableAsPropertyHelper.MakeGenericInstanceType(property.PropertyType);
                    var genericObservableAsPropertyHelperGetValue = observableAsPropertyHelperGetValue.Bind(genericObservableAsPropertyHelper);
                    ModuleDefinition.Import(genericObservableAsPropertyHelperGetValue);
                    LogInfo(genericObservableAsPropertyHelperGetValue.Resolve().ToString());

                    // Declare a field to store the property value
                    var field = new FieldDefinition("$" + property.Name, FieldAttributes.Private, genericObservableAsPropertyHelper);
                    targetType.Fields.Add(field);

                    targetType.Methods.Remove(property.GetMethod);

                    property.GetMethod.Body = new MethodBody(property.GetMethod);
                    property.GetMethod.Body.Emit(il =>
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Callvirt, genericObservableAsPropertyHelperGetValue);
                        il.Emit(OpCodes.Ret);                        
                    });
                    property.GetMethod.SemanticsAttributes = MethodSemanticsAttributes.Getter;
                    targetType.Methods.Add(property.GetMethod);
                }
            }
        }         
    }
}