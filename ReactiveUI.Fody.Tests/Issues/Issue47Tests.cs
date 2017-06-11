using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.Tests.Issues
{
	public class Issue47Tests
	{
		/// <summary>
		/// The "test" here is simply for these to compile
		/// Tests ObservableAsPropertyWeaver.EmitDefaultValue
		/// </summary>
		class TestModel : ReactiveObject
		{
			public extern int IntProperty { [ObservableAsProperty] get; }
			public extern double DoubleProperty { [ObservableAsProperty] get; }
			public extern float FloatProperty { [ObservableAsProperty] get; }
			public extern long LongProperty { [ObservableAsProperty] get; }
		}
	}
}
