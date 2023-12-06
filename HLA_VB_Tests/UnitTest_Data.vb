Imports HLA_VB.HLA_VB
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace HLA_VB_Tests

    Public Class UnitTest_Data
        <TestClass>
        Public Class UnitTestData
            Inherits TestCommon

            <TestMethod>
            Sub TestData0()
                Dim d = New Data()
                Assert.AreEqual(0, d.GetValue())
            End Sub

            <TestMethod>
            Sub TestData1()
                Dim d = New Data()
                d.SetValue(27)
                Assert.AreEqual(27, d.GetValue())
            End Sub

            <TestMethod>
            Sub TestData2()
                Dim d = New Data()
                d.SetValue(98)
                Assert.AreEqual($"{" ".PadRight(TotalWidth)}98", d.ToString())
            End Sub

            <TestMethod>
            Sub TestData3()
                Dim d = New Data()
                d.SetValue(-15)
                d.AddLabel("Start")
                Assert.AreEqual($"{"Start:".PadRight(TotalWidth)}-15", d.ToString())
            End Sub

            <TestMethod>
            Sub TestData4()
                Dim m As Memory
                Dim r As Registers
                Dim errors As List(Of String)

                Dim program = New List(Of String)() From {"R1 = Memory[Zero]", "R9 = memory[Initial]", "R0 = memory[final]",
                                                          "REPEAT",
                                                                "R1 = R1 + R9",
                                                                "R9 = R9 - 1",
                                                          "UNTIL R9 < R0",
                                                          "HALT",
                                                          "Zero: data 0",
                                                          "initial: data 50",
                                                          "final: data 0"}
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

    End Class
End Namespace