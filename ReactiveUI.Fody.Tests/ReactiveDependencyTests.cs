using System;
using System.ComponentModel;
using NUnit.Framework;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.Tests
{
    public class ReactiveDependencyTests
    {
        [Test]
        public void IntPropertyOnWeavedFacadeReturnsBaseModelIntPropertyDefaultValueTest()
        {
            var model = new BaseModel();
            var expectedResult = model.IntProperty;

            var facade = new FacadeModel(model);

            Assert.AreEqual(expectedResult, facade.IntProperty);
        }

        [Test]
        public void AnotherStringPropertyOnFacadeReturnsBaseModelStringPropertyDefaultValueTest()
        {
            var model = new BaseModel();
            var expectedResult = model.StringProperty;

            var facade = new FacadeModel(model);

            Assert.AreEqual(expectedResult, facade.AnotherStringProperty);
        }

        [Test]
        public void SettingAnotherStringPropertyUpdatesTheDependencyStringProperty()
        {
            var expectedResult = "New String Value";
            var facade = new FacadeModel(new BaseModel());

            facade.AnotherStringProperty = expectedResult;

            Assert.AreEqual(expectedResult, facade.Dependency.StringProperty);
        }

        [Test]
        public void SettingFacadeIntPropertyUpdatesDependencyIntProperty()
        {
            var expectedResult = 999;
            var facade = new FacadeModel(new BaseModel());

            facade.IntProperty = expectedResult;

            Assert.AreEqual(expectedResult, facade.Dependency.IntProperty);
        }

        [Test]
        public void FacadeIntPropertyChangedEventFiresOnAssignementTest()
        {
            var expectedPropertyChanged = "IntProperty";
            var resultPropertyChanged = string.Empty;

            var facade = new FacadeModel(new BaseModel());

            var obj = (INotifyPropertyChanged) facade;
            obj.PropertyChanged += (sender, args) => resultPropertyChanged = args.PropertyName;

            facade.IntProperty = 999;

            Assert.AreEqual(expectedPropertyChanged, resultPropertyChanged);
        }

        [Test]
        public void FacadeAnotherStringPropertyChangedEventFiresOnAssignementTest()
        {
            var expectedPropertyChanged = "AnotherStringProperty";
            var resultPropertyChanged = string.Empty;

            var facade = new FacadeModel(new BaseModel());

            var obj = (INotifyPropertyChanged) facade;
            obj.PropertyChanged += (sender, args) => resultPropertyChanged = args.PropertyName;

            facade.AnotherStringProperty = "Some New Value";

            Assert.AreEqual(expectedPropertyChanged, resultPropertyChanged);
        }

        [Test]
        public void StringPropertyOnWeavedDecoratorReturnsBaseModelDefaultStringValue()
        {
            var model = new BaseModel();
            var expectedResult = model.StringProperty;

            var decorator = new DecoratorModel(model);

            Assert.AreEqual(expectedResult, decorator.StringProperty);
        }

        [Test]
        public void DecoratorStringPropertyRaisesPropertyChanged()
        {
            var expectedPropertyChanged = "StringProperty";
            var resultPropertyChanged = string.Empty;

            var decorator = new DecoratorModel(new BaseModel());

            var obj = (INotifyPropertyChanged) decorator;
            obj.PropertyChanged += (sender, args) => resultPropertyChanged = args.PropertyName;

            decorator.StringProperty = "Some New Value";

            Assert.AreEqual(expectedPropertyChanged, resultPropertyChanged);
        }
    }

    public class BaseModel : ReactiveObject
    {
        public virtual int IntProperty { get; set; } = 5;
        public virtual string StringProperty { get; set; } = "Initial Value";
    }

    public class FacadeModel : ReactiveObject
    {
        private BaseModel _dependency;

        public FacadeModel()
        {
            _dependency = new BaseModel();
        }

        public FacadeModel(BaseModel dependency)
        {
            _dependency = dependency;
        }

        public BaseModel Dependency
        {
            get { return _dependency; }
            private set { _dependency = value; }
        }

        // Property with the same name, will look for a like for like name on the named dependency
        [ReactiveDependency(nameof(Dependency))]
        public int IntProperty { get; set; }

        // Property named differently to that on the dependency but still pass through value
        [ReactiveDependency(nameof(Dependency), TargetProperty = "StringProperty")]
        public string AnotherStringProperty { get; set; }
    }

    public class DecoratorModel : BaseModel
    {
        private readonly BaseModel _model;

        // Testing ctor
        public DecoratorModel()
        {
            _model = new BaseModel();
        }

        public DecoratorModel(BaseModel baseModel)
        {
            _model = baseModel;
        }

        [Reactive]
        public string SomeCoolNewProperty { get; set; }

        // Works with private fields
        [ReactiveDependency(nameof(_model))]
        public override string StringProperty { get; set; }

        // Can't be attributed as has additional functionality in the decorated get
        public override int IntProperty
        {
            get { return _model.IntProperty * 2; }
            set
            {
                _model.IntProperty = value;
                this.RaisePropertyChanged();
            }
        }
    }
}