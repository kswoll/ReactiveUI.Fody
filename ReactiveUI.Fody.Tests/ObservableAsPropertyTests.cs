using System.Reactive.Linq;
using NUnit.Framework;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.Tests
{
    public class ObservableAsPropertyTests
    {
        [Test]
        public void TestPropertyReturnsFoo()
        {
            var model = new TestModel();
            Assert.AreEqual("foo", model.TestProperty);
        }

        class TestModel : ReactiveObject
        {
            [ObservableAsProperty]
            public string TestProperty { get; private set; }

            public TestModel()
            {
                Observable.Return("foo").ToPropertyEx(this, x => x.TestProperty);
            }
        } 
    }
}