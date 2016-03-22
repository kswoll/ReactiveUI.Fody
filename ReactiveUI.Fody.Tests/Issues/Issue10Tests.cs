using System;
using NUnit.Framework;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.Tests.Issues
{
    [TestFixture]
    public class Issue10Tests
    {
        [Test]
        public void UninitializedObservableAsPropertyHelperDoesntThrowAndReturnsDefaultValue()
        {
            var model = new TestModel();
            Assert.AreEqual(null, model.MyProperty);
            Assert.AreEqual(0, model.MyIntProperty);
            Assert.AreEqual(default(DateTime), model.MyDateTimeProperty);
        }

        class TestModel : ReactiveObject
        {
            [ObservableAsProperty]
            public string MyProperty { get; private set; }

            [ObservableAsProperty]
            public int MyIntProperty { get; private set; }

            [ObservableAsProperty]
            public DateTime MyDateTimeProperty { get; private set; }

            public string OtherProperty { get; private set; }

            public TestModel()
            {
                OtherProperty = MyProperty;
            }
        }
    }
}