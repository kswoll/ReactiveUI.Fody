using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ReactiveUI.Fody
{
    public static class CecilExtensions
    {
        public static void Emit(this MethodBody body, Action<ILProcessor> il)
        {
            il(body.GetILProcessor());
        }

        public static GenericInstanceMethod MakeGenericMethod(this MethodReference method, params TypeReference[] genericArguments)
        {
            var result = new GenericInstanceMethod(method);
            foreach (var argument in genericArguments)
                result.GenericArguments.Add(argument);
            return result;
        }

        public static bool IsAssignableFrom(this TypeReference baseType, TypeReference type)
        {
            return baseType.Resolve().IsAssignableFrom(type.Resolve());
        }

        public static bool IsAssignableFrom(this TypeDefinition baseType, TypeDefinition type)
        {
            Queue<TypeDefinition> queue = new Queue<TypeDefinition>();
            queue.Enqueue(type);

            while (queue.Any())
            {
                var current = queue.Dequeue();
                if (Equals(baseType, current))
                    return true;

                if (current.BaseType != null)
                    queue.Enqueue(current.BaseType.Resolve());

                foreach (var @interface in current.Interfaces)
                {
                    queue.Enqueue(@interface.Resolve());
                }
            }

            return false;
        }

        public static bool IsDefined(this IMemberDefinition member, TypeReference attributeType)
        {
            return member.HasCustomAttributes && member.CustomAttributes.Any(x => x.AttributeType.FullName == attributeType.FullName);
        }

        public static MethodReference Bind(this MethodReference method, GenericInstanceType genericType)
        {
            var reference = new MethodReference(method.Name, method.ReturnType, genericType);
            reference.HasThis = method.HasThis;
            reference.ExplicitThis = method.ExplicitThis;
            reference.CallingConvention = method.CallingConvention;

            foreach (var parameter in method.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            return reference;
        }
    }
}