using NUnit.Framework;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveUI.Fody.Tests.Issues
{
    [TestFixture]
    public class Issue13Tests
    {
        [Test]
        public void AccessingAChainedObservableAsPropertyOfDoubleDoesntThrow()
        {
            var vm = new VM();
            Assert.AreEqual(0.0, vm.P2);
        }

        class VM : ReactiveObject
        {
            [ObservableAsProperty] public double P1 { get; }
            [ObservableAsProperty] public double P2 { get; }

            public VM()
            {
                Observable.Return(0.0).ToPropertyEx(this, vm => vm.P1);
                this.WhenAnyValue(vm => vm.P1).ToPropertyEx(this, vm => vm.P2);
            }
        }

    }

}
