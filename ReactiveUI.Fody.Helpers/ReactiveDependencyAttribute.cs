using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveUI.Fody.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ReactiveDependencyAttribute : Attribute
    {
        private readonly string _targetName;

        public ReactiveDependencyAttribute(string targetName)
        {
            _targetName = targetName;
        }

        /// <summary>
        /// The name of the backing property
        /// </summary>
        public string Target => _targetName;

        /// <summary>
        /// Target property on the backing property
        /// </summary>
        public string TargetProperty { get; set; }
    }
}
