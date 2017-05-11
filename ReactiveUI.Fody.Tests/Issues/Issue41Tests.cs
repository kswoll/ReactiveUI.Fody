using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.Tests.Issues
{
    [TestFixture]
    public class Issue41Tests
    {
        [Test]
        public void PropertyChangedRaisedForDerivedPropertyOnIntPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = nameof(TestModel.DerivedProperty);
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged) model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.IntProperty = 5;

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName));
        }

        [Test]
        public void PropertyChangedRaisedOnStringPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = nameof(TestModel.DerivedProperty);
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged) model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.StringProperty = "Foo";

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName));
        }

        [Test]
        public void PropertyChangedRaisedForDerivedPropertyAndAnotherExpressionBodiedPropertyAndCombinedExpressionBodyPropertyWithAutoPropOnIntPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName1 = nameof(TestModel.AnotherExpressionBodiedProperty);
            var expectedInpcPropertyName2 = nameof(TestModel.DerivedProperty);
            var expectedInpcPropertyName3 = nameof(TestModel.CombinedExpressionBodyPropertyWithAutoProp);
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged) model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.IntProperty = 5;

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName1));
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName2));
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName3));
        }

        [Test]
        public void PropertyChangedRaisedForCombinedExpressionBodyPropertyWithAutoPropOnStringPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = nameof(TestModel.CombinedExpressionBodyPropertyWithAutoProp);
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged)model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.StringProperty = "Foo";

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName));
        }

        [Test]
        public void PropertyChangedRaisedForCombinedExpressionBodyPropertyWithAutoPropNonReactivePropertyOnIntPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = nameof(TestModel.CombinedExpressionBodyPropertyWithAutoPropNonReactiveProperty);
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged)model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.IntProperty = 5;

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName));
        }

        [Test]
        // Ensure that this only works with dependent properties that have the [Reactive] attribute
        public void PropertyChangedNotRaisedForCombinedExpressionBodyPropertyWithAutoPropNonReactivePropertyOnNonReactivePropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = nameof(TestModel.CombinedExpressionBodyPropertyWithAutoPropNonReactiveProperty);
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged)model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.NonReactiveProperty = "Foo";

            // Assert
            Assert.IsEmpty(receivedInpcPropertyNames);
        }

        class TestModel : ReactiveObject
        {
            [Reactive]
            public int IntProperty { get; set; }

            [Reactive]
            public string StringProperty { get; set; }

            public string NonReactiveProperty { get; set; }

            [Reactive]
            // Raise property change when either StringProperty or IntProperty are set
            public string DerivedProperty => StringProperty + IntProperty;

            [Reactive]
            // Raise property change when IntProperty is set
            public int AnotherExpressionBodiedProperty => IntProperty;

            [Reactive]
            // Raise property changed when StringProperty or IntProperty is set (as AnotherExpressionBodiedProperty is dependent upon IntProperty)
            public string CombinedExpressionBodyPropertyWithAutoProp => AnotherExpressionBodiedProperty + StringProperty;

            [Reactive]
            public string CombinedExpressionBodyPropertyWithAutoPropNonReactiveProperty => CombinedExpressionBodyPropertyWithAutoProp + NonReactiveProperty;
        } 
    }
}
