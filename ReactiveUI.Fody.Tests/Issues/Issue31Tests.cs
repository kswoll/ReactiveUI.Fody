using System;
using System.Reactive.Linq;
using NUnit.Framework;
using ReactiveUI.Fody.Helpers;
using GlobalSettings = ReactiveUI.Fody.Helpers.Settings.GlobalSettings;

namespace ReactiveUI.Fody.Tests.Issues
{
    [TestFixture]
    public class Issue31Tests
    {
        [Test]
        public void ExceptionPropertyInfo()
        {
            try
            {
                GlobalSettings.IsLogPropertyOnErrorEnabled = true;

                try
                {
                    new TestModel();
                    Assert.Fail();
                }
                catch (UnhandledErrorException ex)
                {
                    var propertyException = (LogPropertyOnErrorException)ex.InnerException;
                    Assert.AreEqual(nameof(TestModel.MyProperty), propertyException.Property);
                }
            }
            finally
            {
                GlobalSettings.IsLogPropertyOnErrorEnabled = false;
            }
        }

        public class TestModel : ReactiveObject
        {
            public extern string MyProperty { [ObservableAsProperty]get; }

            public TestModel()
            {
                Observable.Throw<string>(new Exception("Observable error")).ToPropertyEx(this, x => x.MyProperty);
            }
        }
    }
}