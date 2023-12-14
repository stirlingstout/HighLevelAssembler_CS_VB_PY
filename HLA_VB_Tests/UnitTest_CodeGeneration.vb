Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports HLA_VB.HLA_VB
Imports Microsoft.VisualStudio.TestPlatform.ObjectModel.Client.Interfaces
Imports System.IO

Namespace HLA_VB_Tests
    <TestClass>
    Public Class UnitTest_CodeGeneration_Arithmetic
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
    End Class

    <TestClass>
    Public Class UnitTest_CodeGeneration_Labels

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

            Dim program = New List(Of String)() From {"R1 = 13",
                                                      "START: R1 = R1 - 6",
                                                      "IF R1 > 0 GOTO Start",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(-5, r(1))
        End Sub
    End Class

    <TestClass>
    Public Class UnitTest_CodeGeneration_ControlStructures_FOR
        <TestMethod>
        Sub TestCodeGeneration7()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "FOR R2 = 1 to 10",
                                                            "R1 = R1 + R2",
                                                      "END FOR",
                                                      "HALT"}
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

            Dim program = New List(Of String)() From {"R1 = 0",
                                                      "FOR R2 = 100 downto 1",
                                                            "R1 = R1 + R2",
                                                      "END FOR",
                                                      "HALT"}
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

    <TestClass>
    Public Class UnitTest_CodeGeneration_ControlStructures_REPEAT
        <TestMethod>
        Sub TestCodeGeneration9()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0", "R9 = 50", "R0 =0",
                                                      "REPEAT",
                                                            "R1 = R1 + R9",
                                                            "R9 = R9 - 1",
                                                      "UNTIL R9 <= R0",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(1275, r(1))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration10()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0", "R9 = 0", "R0 =50",
                                                      "REPEAT",
                                                            "R1 = R1 + R9",
                                                            "R9 = R9 + 1",
                                                      "UNTIL R9 >= R0",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(1225, r(1))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration11()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0", "R9 = 50", "R0 =0",
                                                      "REPEAT",
                                                            "R1 = R1 + R9",
                                                            "R9 = R9 - 1",
                                                      "UNTIL R9 < R0",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(1275, r(1))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration12()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0", "R9 = 0", "R0 =50",
                                                      "REPEAT",
                                                            "R1 = R1 + R9",
                                                            "R9 = R9 + 1",
                                                      "UNTIL R9 > R0",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(1275, r(1))
        End Sub
    End Class

    <TestClass>
    Public Class UnitTest_CodeGeneration_ControlStructures_WHILE
        <TestMethod>
        Sub TestCodeGeneration13()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = 0", "R9 = 0", "R0 =50",
                                                      "WHile R9 <> R0",
                                                            "R1 = R1 + R9",
                                                            "R9 = R9 + 1",
                                                      "end while",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(1225, r(1)) ' 1225 since the 50 doesn't get added!
        End Sub

    End Class

    <TestClass>
    Public Class UnitTest_CodeGeneration_IF
        <TestMethod>
        Sub TestCodeGeneration14()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R2 = 99",
                                                      "IF R2 > 0 THEN",
                                                         "R2 = R2 + R2",
                                                      "END IF",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(198, r(2))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration15()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R2 = 99",
                                                      "IF R2 < 0 THEN",
                                                         "R2 = R2 + R2",
                                                      "ELSE",
                                                         "R2 = R2 >> 1",
                                                      "END IF",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(49, r(2))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration16()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R2 = 0",
                                                      "IF R2 < 0 THEN",
                                                         "R2 = R2 + R2",
                                                      "ELSE IF R2 = 0 THEN",
                                                         "R2 = 45",
                                                      "ELSE",
                                                         "R2 = R2 >> 1",
                                                      "END IF",
                                                      "HALT"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)
            Assert.AreEqual(45, r(2))
        End Sub
    End Class

    <TestClass>
    Public Class UnitTest_CodeGeneration_SamplePrograms
        <TestMethod>
        Sub TestCodeGeneration16()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("program1.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ' Program cannot be executed as it has endless loops
        End Sub

        <TestMethod>
        Sub TestCodeGeneration17()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("program2.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)

            Assert.AreEqual(12, r(1))
            Assert.AreEqual(24, r(2))
            Assert.AreEqual(19, r(3))
            Assert.AreEqual(8, r.PC)
        End Sub

        <TestMethod>
        Sub TestCodeGeneration18()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("program3.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)

            Assert.AreEqual(10, r(1))
            Assert.AreEqual(20, r(2))
            Assert.AreEqual(30, r(3))
            Assert.AreEqual(11, r.PC)

            Assert.IsTrue(TypeOf m(11) Is Data)
            Assert.AreEqual(10, m(11).GetValue())
            Assert.IsTrue(TypeOf m(12) Is Data)
            Assert.AreEqual(20, m(12).GetValue())
            Assert.IsTrue(TypeOf m(13) Is Data)
            Assert.AreEqual(30, m(13).GetValue())

        End Sub

        <TestMethod>
        Sub TestCodeGeneration19()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("program4.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.ThrowsException(Of System.Data.DataException)(Sub() ExecuteProgram(m, r))
        End Sub

        <TestMethod>
        Sub TestCodeGeneration20()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("BinaryToHexASCII.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            ExecuteProgram(m, r)

            Assert.AreEqual(63, r(0))
            Assert.AreEqual(51, r(3))
            Assert.AreEqual(70, r(2))

            Assert.IsTrue(TypeOf m(17) Is Data)
            Assert.AreEqual(63, m(17).GetValue())
            Assert.IsTrue(TypeOf m(18) Is Data)
            Assert.AreEqual(51, m(18).GetValue())
            Assert.IsTrue(TypeOf m(19) Is Data)
            Assert.AreEqual(70, m(19).GetValue())
        End Sub

        <TestMethod>
        Sub TestCodeGeneration21()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("Program5.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(3, r.PC)
            ExecuteProgram(m, r)


            Assert.AreEqual(10, r(1))
            Assert.AreEqual(20, r(2))
            Assert.AreEqual(30, r(3))

            Assert.IsTrue(TypeOf m(0) Is Data)
            Assert.AreEqual(10, m(0).GetValue())
            Assert.IsTrue(TypeOf m(1) Is Data)
            Assert.AreEqual(20, m(1).GetValue())
            Assert.IsTrue(TypeOf m(2) Is Data)
            Assert.AreEqual(30, m(2).GetValue())
        End Sub

        <TestMethod>
        Sub TestCodeGeneration22()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program As New List(Of String) From {"HALT", "LOcation 100", "DATA 1", "DATA 2", "DATA 3"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, errors.Count)
            Assert.AreEqual(0, r.PC)
            ExecuteProgram(m, r)

            Assert.IsTrue(TypeOf m(100) Is Data)
            Assert.AreEqual(1, m(100).GetValue())
            Assert.IsTrue(TypeOf m(101) Is Data)
            Assert.AreEqual(2, m(101).GetValue())
            Assert.IsTrue(TypeOf m(102) Is Data)
            Assert.AreEqual(3, m(102).GetValue())
        End Sub

        <TestMethod>
        Sub TestCodeGeneration23()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = (New StreamReader("Program6.hla")).ReadToEnd().Split(Environment.NewLine).ToList()
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(0, r.PC)
            ExecuteProgram(m, r)
            Assert.AreEqual(0, errors.Count)

            Assert.AreEqual(10, r(1))
            Assert.AreEqual(20, r(2))
            Assert.AreEqual(30, r(3))

            Assert.IsTrue(TypeOf m(100) Is Data)
            Assert.AreEqual(10, m(100).GetValue())
            Assert.IsTrue(TypeOf m(101) Is Data)
            Assert.AreEqual(20, m(101).GetValue())
            Assert.IsTrue(TypeOf m(102) Is Data)
            Assert.AreEqual(30, m(102).GetValue())
        End Sub

        <TestMethod>
        Sub TestCodeGeneration24()
            Dim m As Memory
            Dim r As Registers
            Dim errors As List(Of String)

            Dim program = New List(Of String)() From {"R1 = R1 + 5", "LOcation 0"}
            With CompileHLA(program)
                m = .assembly
                r = .registers
                errors = .errorList
            End With
            Assert.AreEqual(1, errors.Count)
            Assert.IsTrue(errors(0).StartsWith("Attempt made to move the deposit location backward from 1 to 0"))

        End Sub
    End Class
End Namespace
