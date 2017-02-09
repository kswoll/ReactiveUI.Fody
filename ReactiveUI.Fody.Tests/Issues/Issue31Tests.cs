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
        public void ExceptionPropertyInfoForReactiveProperty()
        {
            try
            {
                GlobalSettings.IsLogPropertyOnErrorEnabled = true;

                try
                {
                    var model = new ReactivePropertyModel();
                    model.MyProperty = "foo";
                    Assert.Fail();
                }
                catch (LogPropertyOnErrorException ex)
                {
                    Assert.AreEqual(nameof(ObservableAsPropertyModel.MyProperty), ex.Property);
                }
            }
            finally
            {
                GlobalSettings.IsLogPropertyOnErrorEnabled = false;
            }
        }

        [Test]
        public void ExceptionPropertyInfoForObservableAsProperty()
        {
            try
            {
                GlobalSettings.IsLogPropertyOnErrorEnabled = true;

                try
                {
                    new ObservableAsPropertyModel();
                    Assert.Fail();
                }
                catch (UnhandledErrorException ex)
                {
                    var propertyException = (LogPropertyOnErrorException)ex.InnerException;
                    Assert.AreEqual(nameof(ObservableAsPropertyModel.MyProperty), propertyException.Property);
                }
            }
            finally
            {
                GlobalSettings.IsLogPropertyOnErrorEnabled = false;
            }
        }

        public class ObservableAsPropertyModel : ReactiveObject
        {
            public extern string MyProperty { [ObservableAsProperty]get; }

            public ObservableAsPropertyModel()
            {
                Observable.Throw<string>(new Exception("Observable error")).ToPropertyEx(this, x => x.MyProperty);
            }
        }

        public class ReactivePropertyModel : ReactiveObject
        {
            [Reactive]public string MyProperty { get; set; }

            public ReactivePropertyModel()
            {
                this.ObservableForProperty(x => x.MyProperty).Subscribe(_ =>
                {
                    throw new Exception("Subscribe error");
                });
            }
        }
    }
}