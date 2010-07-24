Imports System.Threading
Imports System.IO.Ports

Public Class portserie

    Public port As New System.IO.Ports.SerialPort
    Private port_ouvert As Boolean = False

    Public Sub New(ByVal numero As String, ByVal vitesse As String)
        port.PortName = numero 'nom du port : COM1
        port.BaudRate = vitesse 'vitesse du port 300, 600, 1200, 2400, 9600, 14400, 19200, 38400, 57600, 115200
        port.Parity = IO.Ports.Parity.None 'pas de parité
        port.StopBits = IO.Ports.StopBits.One 'un bit d'arrêt par octet
        port.DataBits = 8 'nombre de bit par octet
        'port.Handshake = Handshake.None
        port.ReadTimeout = 3000
        port.WriteTimeout = 5000
        'port.RtsEnable = False 'ligne Rts désactivé
        'port.DtrEnable = False 'ligne Dtr désactivé
        'AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf port_DataReceived)
    End Sub

    Public Function ouvrir() As String
        Try
            If Not port_ouvert Then
                port.Open()
                port_ouvert = True

                AddHandler port.DataReceived, AddressOf DataReceived

                Return ("Port " & port.PortName & " ouvert")
            Else
                Return ("Port " & port.PortName & " dejà ouvert")
            End If
        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    Public Function fermer() As String
        Try
            If port_ouvert Then
                If (Not (port Is Nothing)) Then ' The COM port exists.
                    If port.IsOpen Then
                        Do While (port.BytesToWrite > 0) ' Wait for the transmit buffer to empty.
                        Loop
                        port.Close()
                        Return ("Port " & port.PortName & " fermé")
                    End If
                    Return ("Port " & port.PortName & "  est déjà fermé")
                End If
                port_ouvert = False
                Return ("Port " & port.PortName & " n'existe pas")
            End If
        Catch ex As UnauthorizedAccessException
            Return ("ERR: Port " & port.PortName & " IGNORE")
            ' The port may have been removed. Ignore.
        End Try
        Return True
    End Function

    Public Function ecrire(ByVal command() As Byte) As String
        'Dim response As String
        Try
            port.Write(command, 0, command.Length)
            'response = port.ReadLine
            Return "donnée ecrite "
        Catch ex As TimeoutException
            Return ("ERR: " & ex.Message)
        Catch ex As InvalidOperationException
            Return ("ERR: " & ex.Message)
        Catch ex As UnauthorizedAccessException
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    'Public Function lire() As Byte()
    '    Dim response As Byte()
    '    Try
    '        'response = port.Read(response(9), 0, response.Length)
    '        Return response
    '    Catch
    '    End Try
    'End Function

    Private Sub DataReceived(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs)
        Dim reponse As String = ""
        Dim bytes As Integer = port.BytesToRead
        'create a byte array to hold the awaiting data
        Dim comBuffer As Byte() = New Byte(bytes - 1) {}
        'read the data and store it
        port.Read(comBuffer, 0, bytes)
        For Each data As Byte In comBuffer
            reponse = reponse & Convert.ToString(data, 16).PadLeft(2, "0"c).PadRight(3, " "c)
        Next
        'display the data to the user
        MsgBox(reponse)
        
    End Sub


End Class
