Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_CodeGeneration
        <TestMethod>
        Sub TestCodeGeneration0()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = R2 + R3"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(TypeName(New ADDRegisterInstruction(1, 2, 3)), TypeName(m(0)))
        End Sub

    End Class
End Namespace
