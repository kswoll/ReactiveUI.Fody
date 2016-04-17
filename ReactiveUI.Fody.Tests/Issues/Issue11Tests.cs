using System.Reactive.Linq;
using NUnit.Framework;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.Tests.Issues
{
    [TestFixture]
    public class Issue11Tests
    {
        [Test]
        public void AllowObservableAsPropertyAttributeOnAccessor()
        {
            var model = new TestModel("foo");
            Assert.AreEqual("foo", model.MyProperty);
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