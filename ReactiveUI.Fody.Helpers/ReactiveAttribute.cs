using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveUI.Fody.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class ReactiveAttribute : Attribute
    {
    }
}
