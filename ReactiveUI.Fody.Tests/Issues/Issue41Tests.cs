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
        public void PropertyChangeNotificationRaisedForDerivedPropertyOnIntPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = "DerivedProperty";
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged) model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.IntProperty = 5;

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName));
        }

        [Test]
        public void PropertyChangeNotificationRaisedOnStringPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = "DerivedProperty";
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged) model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.StringProperty = "Foo";

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName));
        }

        [Test]
        public void PropertyChangeNotificationRaisedForDerivedPropertyAndAnotherExpressionBodiedPropertyOnIntPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName1 = "AnotherExpressionBodiedProperty";
            var expectedInpcPropertyName2 = "DerivedProperty";
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged) model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.IntProperty = 5;

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName1));
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName2));
        }

        [Test]
        public void PropertyChangeNotificationRaisedForCombinedExpressionBodyPropertyWithAutoPropOnStringPropertySet()
        {
            // Arrange
            var model = new TestModel();
            var expectedInpcPropertyName = "CombinedExpressionBodyPropertyWithAutoProp";
            var receivedInpcPropertyNames = new List<string>();

            var inpc = (INotifyPropertyChanged)model;
            inpc.PropertyChanged += (sender, args) => receivedInpcPropertyNames.Add(args.PropertyName);

            // Act
            model.StringProperty = "Foo";

            // Assert
            Assert.IsTrue(receivedInpcPropertyNames.Contains(expectedInpcPropertyName));
        }

        class TestModel : ReactiveObject
        {
            // Does not raise property change on itself
            public int IntProperty { get; set; }

            [Reactive]
            // Raise Property Change on self to ensure this doesn't break existing [Reactive] weaving
            public string StringProperty { get; set; }

            [Reactive]
            // Raise property change when either StringProperty or IntProperty are set
            public string DerivedProperty => StringProperty + IntProperty;

            [Reactive]
            // Raise property change when IntProperty is set
            public int AnotherExpressionBodiedProperty => IntProperty;

            [Reactive]
            public string CombinedExpressionBodyPropertyWithAutoProp => AnotherExpressionBodiedProperty + StringProperty;
        } 
    }
}
