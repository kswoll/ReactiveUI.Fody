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
            var reactiveUI = ModuleDefinition.AssemblyReferences.Where(x => x.Name == "ReactiveUI").OrderByDescending(x => x.Version).FirstOrDefault();
            if (reactiveUI == null)
                throw new Exception("Could not find assembly: ReactiveUI (" + string.Join(", ", ModuleDefinition.AssemblyReferences.Select(x => x.Name)) + ")");
            LogInfo(string.Format("{0} {1}", reactiveUI.Name, reactiveUI.Version));
            var helpers = ModuleDefinition.AssemblyReferences.Where(x => x.Name == "ReactiveUI.Fody.Helpers").OrderByDescending(x => x.Version).FirstOrDefault();
            if (helpers == null)
                throw new Exception("Could not find assembly: ReactiveUI.Fody.Helpers (" + string.Join(", ", ModuleDefinition.AssemblyReferences.Select(x => x.Name)) + ")");
            LogInfo(string.Format("{0} {1}", helpers.Name, helpers.Version));

            var reactiveObject = ModuleDefinition.FindType("ReactiveUI", "ReactiveObject", reactiveUI);

            // The types we will scan are subclasses of ReactiveObject
            var targetTypes = ModuleDefinition.Types.Where(x => x.BaseType != null && reactiveObject.IsAssignableFrom(x.BaseType));

            var observableAsPropertyHelper = ModuleDefinition.FindType("ReactiveUI", "ObservableAsPropertyHelper`1", reactiveUI, "T");
            var observableAsPropertyAttribute = ModuleDefinition.FindType("ReactiveUI.Fody.Helpers", "ObservableAsPropertyAttribute", helpers);
            var observableAsPropertyHelperGetValue = ModuleDefinition.Import(observableAsPropertyHelper.Resolve().Properties.Single(x => x.Name == "Value").GetMethod);
            var exceptionType = ModuleDefinition.FindType("System", "Exception");
            var exceptionConstructor = exceptionType.Resolve().GetConstructors().Single(x => x.Parameters.Count == 1);

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

                    // It's an auto-property, so remove the generated field
                    if (property.SetMethod != null)
                    {
                        // Remove old field (the generated backing field for the auto property)
                        var oldField = (FieldReference)property.GetMethod.Body.Instructions.Where(x => x.Operand is FieldReference).Single().Operand;
                        var oldFieldDefinition = oldField.Resolve();
                        targetType.Fields.Remove(oldFieldDefinition);                        

                        // Re-implement setter to throw an exception
                        property.SetMethod.Body = new MethodBody(property.SetMethod);
                        property.SetMethod.Body.Emit(il =>
                        {
                            il.Emit(OpCodes.Ldstr, "Never call the setter of an ObservabeAsPropertyHelper property.");
                            il.Emit(OpCodes.Newobj, exceptionType);
                            il.Emit(OpCodes.Throw);
                            il.Emit(OpCodes.Ret);
                        });
                    }

                    property.GetMethod.Body = new MethodBody(property.GetMethod);
                    property.GetMethod.Body.Emit(il =>
                    {
                        il.Emit(OpCodes.Ldarg_0);                                               // this
                        il.Emit(OpCodes.Ldfld, field.BindDefinition(targetType));               // pop -> this.$PropertyName
                        il.Emit(OpCodes.Callvirt, genericObservableAsPropertyHelperGetValue);   // pop -> this.$PropertyName.Value
                        il.Emit(OpCodes.Ret);                                                   // Return the value that is on the stack
                    });
                }
            }
        }         
    }
}