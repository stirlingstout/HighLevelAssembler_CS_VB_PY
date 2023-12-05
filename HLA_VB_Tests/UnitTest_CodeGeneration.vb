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

        <TestMethod>
        Sub TestCodeGeneration1()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = R2 + R3", "R1 = R2 + 45"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(TypeName(New ADDRegisterInstruction(1, 2, 3)), TypeName(m(0)))
            Assert.AreEqual(TypeName(New ADDImmediateInstruction(1, 2, 3)), TypeName(m(1)))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration2()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Const errorRegister = 45
            Dim invalidLine = $"R1 = R2 + R{errorRegister}"
            Dim errorLine = $"Invalid second operand register {errorRegister} in {invalidLine}"
            Dim program = New List(Of String)() From {"R1 = R2 + R3", invalidLine}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(1, errors.Count)
            Assert.AreEqual(TypeName(New ADDRegisterInstruction(1, 2, 3)), TypeName(m(0)))
            Assert.AreEqual(errorLine, errors(0))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration3()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim errorLine = $"Undefined label START"
            Dim program = New List(Of String)() From {"GOTO START"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(1, errors.Count)
            Assert.AreEqual(errorLine, errors(0))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration4()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim errorLine = $"Undefined label START"
            Dim program = New List(Of String)() From {"Start: GOTO START"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(0, CType(m(0), BInstruction).destination)
        End Sub

        <TestMethod>
        Sub TestCodeGeneration5()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 25", "START: GOTO START"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.IsTrue(TypeOf m(0) Is MOVImmediateInstruction)
            Assert.AreEqual(1, CType(m(1), BInstruction).destination)
        End Sub

        <TestMethod>
        Sub TestCodeGeneration6()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 13", "START: R1 = R1 - 6", "IF R1 > 0 GOTO Start", "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(-5, r(1))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration7()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0", "FOR R2 = 1 to 10", "R1 = R1 + R2", "END FOR", "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(55, r(1))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration8()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0", "FOR R2 = 100 downto 1", "R1 = R1 + R2", "END FOR", "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(5050, r(1))
        End Sub
    End Class
End Namespace
