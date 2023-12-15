Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports System.Data
Imports System.Reflection.Metadata
Imports NuGet.Frameworks
Imports System.IO

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTestIndirectLoadAndStoreOffset

        <TestMethod>
        Sub TestLoadAndStoreIndirect4()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("Structures.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(6, r.PC)
            ExecuteProgram(m, r)

            Assert.AreEqual(15, r(2))
            Assert.AreEqual(21, r(3))
            Assert.AreEqual(15, m(4).GetValue())
            Assert.AreEqual(21, m(5).GetValue())
        End Sub
    End Class

End Namespace
