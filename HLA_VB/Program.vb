Imports System
Imports System.Data
Imports System.Diagnostics.Tracing
Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports System.Xml.Schema

Namespace HLA_VB
    ' TODO: think about enumerations for symbols and/or operators

    Public Module Program
        Const HLA_EXTENSION = ".hla"

        Public Const HIGHEST_MEMORY_ADDRESS = 1024
        Public Const HIGHEST_REGISTER_NUMBER = 15
        Public Const REGISTER_SIZE = 32


        Sub DisplayMenu()
            Console.WriteLine("L - (L)oad a HLA file")
            Console.WriteLine("D - (D)isplay the HLA program")
            Console.WriteLine("C - (C)ompile the current HLA program")
            Console.WriteLine("E - (E)xecute the compiled program")
            Console.WriteLine("N - create a (N)ew HLA program")
            Console.WriteLine("H - display the (H)elp file")
            Console.WriteLine("Q - (Q)uit the HLA program")
        End Sub

        Function GetMenuOption() As String
            Console.Write("Enter option: ")
            Return Console.ReadLine().ToUpper()
        End Function

        Function LoadHLAFile(existingProgram As List(Of String)) As List(Of String)
            Console.Write("HLA filename: ")
            Dim Filename = Console.ReadLine().ToUpper()
            If Not Filename.EndsWith(HLA_EXTENSION) Then
                Filename += HLA_EXTENSION
            End If
            Dim program = New List(Of String)
            If File.Exists(Filename) Then
                Using HLA = New StreamReader(Filename)
                    program.AddRange(HLA.ReadToEnd().Split(Environment.NewLine))
                End Using
            Else
                Console.WriteLine($"Couldn't find the file {Filename}")
                program = existingProgram
            End If
            Return program
        End Function

        Sub DisplayHLA(program As List(Of String))
            Dim number = 1
            For Each line In program
                Console.WriteLine($"{number,-4}  {line}")
                number += 1
            Next
        End Sub

        Function NewHLAProgram() As List(Of String)

            Console.WriteLine("Enter your program line by line. Just press Enter to finish")
            Dim lineNumber = 1
            Dim Program = New List(Of String)
            Do
                Console.Write($"Line {lineNumber}: ")
                Dim line = Console.ReadLine()
                If line <> "" Then
                    Debug.Assert(line IsNot Nothing)
                    Program = Program.Append(line).ToList()
                    lineNumber += 1
                Else
                    Exit Do
                End If
            Loop

            Return Program
        End Function

        Sub DisplayHelp()
            Const HELP_FILENAME = "HLA.hlp"
            If File.Exists(HELP_FILENAME) Then
                Try
                    Process.Start("NOTEPAD.EXE", HELP_FILENAME)
                Catch
                    Console.WriteLine($"Could not start NOTEPAD or find the help file ({HELP_FILENAME})")
                End Try
            End If

        End Sub

        ' These functions take a list of tokens representing the current line and return a list of
        ' memory locations/instructions. List because IF ... GOTO generates two instructions
        Function LDRDirect(t As IEnumerable(Of Token)) As List(Of Instruction)
            ' Rd = MEM[100]
            ' 0  1  2 3 4 5
            Try
                Return New List(Of Instruction)() From {New LoadInstructionDirect(t(0).r, t(4).i)}
            Catch ex As Exception
                Throw New Exception()
            End Try
        End Function

        Function LDRDirectLabel(t As IEnumerable(Of Token)) As List(Of Instruction)
            ' Rd = MEM[Start]
            ' 0  1  2 3  4  5
            Return New List(Of Instruction)() From {New LoadInstructionDirect(t(0).r, t(4).id)}
        End Function

        Function STRDirect(t As IEnumerable(Of Token)) As List(Of Instruction)
            ' MEM[100] = Rd
            '  0 1 2 3 4 5
            Return New List(Of Instruction)() From {New StoreInstructionDirect(t(5).r, t(2).i)}
        End Function

        Function STRDirectLabel(t As IEnumerable(Of Token)) As List(Of Instruction)
            ' MEM[Start] = Rd
            '  0 1  2  3 4 5
            Return New List(Of Instruction)() From {New StoreInstructionDirect(t(5).r, t(2).id)}
        End Function

        Function AddRegister(t As IEnumerable(Of Token)) As List(Of Instruction)
            ' Rd = Rn + Rm
            ' 0  1 2  3 4
            Return New List(Of Instruction)() From {New ADDRegisterInstruction(t(0).r, t(2).r, t(4).r)}
        End Function


        Function AddImmediate(t As IEnumerable(Of Token)) As List(Of Instruction)
            ' Rd = Rn + 45
            ' 0  1 2  3 4
            Return New List(Of Instruction)() From {New ADDImmediateInstruction(t(0).r, t(2).r, t(4).i)}
        End Function

        Function CompileHLA(program As List(Of String)) As (assembly As Memory, registers As Registers, errorList As List(Of String))
            Dim m As New Memory()
            Dim r As New Registers()
            Dim errors As New List(Of String)

            Dim patterns As New List(Of (pattern As IEnumerable(Of Token), generator As Func(Of List(Of Token), List(Of Instruction)))) From
            {
            ("R0 = MEM[100]".ToTokens(), AddressOf LDRDirect),
            ("R0 = MEM[First]".ToTokens(), AddressOf LDRDirectLabel),
            ("MEM[100] = R0".ToTokens(), AddressOf STRDirect),
            ("MEM[First] = R0".ToTokens(), AddressOf STRDirectLabel),
            ("R0 = R1 + R2".ToTokens(), AddressOf AddRegister),
            ("R0 = R1 + 25".ToTokens(), AddressOf AddImmediate)}

            r.PC = 0
            For Each line In program
                Dim s As IEnumerable(Of Token) = line.ToTokens()
                Dim labels As New List(Of String)
                Do While s.Count >= 2
                    If s.First().type = TokenType.Identifier AndAlso s(1).type = TokenType.Symbol AndAlso s(1).sym = ":" Then
                        labels.Add(s.First().id)
                        s = s.Skip(2)
                    Else
                        Exit Do
                    End If
                Loop

                Dim matches = patterns.Where(Function(p) s.Matches(p.pattern))
                If matches.Any() Then
                    Dim i = matches.First().generator(s)
                    For Each instruction In matches.First().generator(s)
                        m(r.PC) = instruction
                        r.PC += 1
                    Next
                Else
                    errors.Add($"Error: {line}")
                End If
            Next
            'r.PC = 0
            Return (m, r, errors)
        End Function

        Sub DisplayErrors(program As List(Of String), errorList As List(Of String))
            For Each errorMessage In errorList
                Console.WriteLine(program.Count)
            Next
        End Sub

        Sub DisplayAssembly(m As Memory)
            Debug.Assert(m IsNot Nothing)
        End Sub

        Sub ExecuteProgram(m As Memory, r As Registers)
            Do While Not (r.Halted)
                Dim MAR As Integer
                Dim MBR As MemoryLocation
                Dim CIR As MemoryLocation

                MAR = r.PC
                MBR = m(MAR)
                r.PC += 1

                CIR = MBR
                If CIR.IsExecutable Then
                    CIR.Execute(r, m)
                Else
                    Throw New DataException($"Attempting to execute a non-executable instruction")
                End If

                'TODO: single step, stack for registers and memory to allow undo?
            Loop
        End Sub

        Sub Main(args As String())
            Dim program As New List(Of String)
            Dim m As Memory
            Dim r As Registers

            Do
                DisplayMenu()
                Select Case GetMenuOption()
                    Case "L"
                        program = LoadHLAFile(program)
                    Case "D"
                        DisplayHLA(program)
                    Case "C", "T"
                        If program.Count > 0 Then
                            With CompileHLA(program)
                                m = .assembly
                                r = .registers
                                If .errorList.Count > 0 Then
                                    DisplayErrors(program, .errorList)
                                    m.Clear()
                                Else
                                    DisplayAssembly(m)
                                End If
                            End With
                        Else
                            Console.WriteLine("No HLA program to compile")
                        End If
                    Case "E"
                        ExecuteProgram(m, r)
                    Case "N"
                        program = NewHLAProgram()
                    Case "H", "?"
                        DisplayHelp()
                    Case "Q"
                        Exit Do
                    Case Else
                End Select
            Loop
        End Sub
    End Module

End Namespace