using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using ReactiveUI.Fody.CodeFix;
using ReactiveUI.Fody;
using ReactiveUI.Fody.Helpers;

namespace ReactiveUI.Fody.CodeFix.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ReactiveUI.Fody.Helpers;

namespace ConsoleApplication1
{
    class TypeName
    {   
         int _Foo = 10;
         int Foo { 
              get { return _Foo; } 
              set { 
                    this.RaiseAndSetIfChanged(ref _Foo, value); 
              }
         }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "ReactiveUIFodyCodeFix",
                Message = "The property Foo can be simplified using ReactiveUI.Fody",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 15, 14) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ReactiveUI.Fody.Helpers;

namespace ConsoleApplication1
{
    class TypeName
    {
        [Reactive]
        int Foo { get; set; }
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }


        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PropertyToReactiveCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReactiveUIFodyCodeFixAnalyzer();
        }
    }

    [TestClass]
    public class NegativeUnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ReactiveUI.Fody.Helpers;

namespace ConsoleApplication1
{
    class TypeName
    {   
         int _Foo = 10;
         int Foo {get { return _Foo; } set{_Foo =  value;}}
    }
}";
            var expected = new DiagnosticResult[] {};

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ReactiveUI.Fody.Helpers;

namespace ConsoleApplication1
{
    class TypeName
    {   
         int _Foo = 10;
         int Foo {get { return _Foo; } set{_Foo =  value;}}
    }
}";

            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PropertyToReactiveCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ReactiveUIFodyCodeFixAnalyzer();
        }
    }
}