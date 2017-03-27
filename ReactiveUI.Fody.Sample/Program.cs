using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.Sample
{
    class Program
    {
        public static void Main()
        {
            var model = new TestModel("foo");
            Console.WriteLine(model.MyProperty);

            var list = new[] { 1, 2, 3, 4 };

            Console.ReadLine();
        }

        public class TestModel : ReactiveObject
        {
            public extern string MyProperty { [ObservableAsProperty]get; }

            public TestModel(string myProperty)
            {
                Observable.Return(myProperty).ToPropertyEx(this, x => x.MyProperty);
            }
        }
    }
}
