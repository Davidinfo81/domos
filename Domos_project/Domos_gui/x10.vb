Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports System.IO.Ports
Imports System.Math
Imports System.Net.Sockets
Imports System.Threading
Imports System.Globalization

Public Class x10
    Public WithEvents port As New System.IO.Ports.SerialPort
    Private port_ouvert As Boolean = False
    Private port_name As String = ""
    Private com_to_hex As New Dictionary(Of String, Byte)
    Private house_to_hex As New Dictionary(Of String, Byte)
    Private device_to_hex As New Dictionary(Of String, Byte)
    Private GetPortInput As Boolean
    Private OutPortDevice As Boolean = False

#Region "Déclaration"
    ' CM11 Handshaking codes
    Public Const SET_TIME As Byte = &H9B
    Public Const INFERFACE_READY As Byte = &H55
    Public Const COMPUTER_READY As Byte = &HC3
    Public Const ACK As Byte = &H0

    ' CM11 Hail codes
    Public Const INTERFACE_CQ As Byte = &H5A
    Public Const CM11_CLOCK_REQ As Byte = &HA5
    Public Const CP10_CLOCK_REQ As Byte = &HA6

    'CM11 Command Header Definitions
    Public Const STANDARD_ADDRESS As Byte = &H4
    Public Const STANDARD_FUNCTION As Byte = &H6
    Public Const ENHANCED_ADDRESS As Byte = &H5
    Public Const ENHANCED_FUNCTION As Byte = &H7
#End Region

    Public Sub New()

        house_to_hex.Add("A", &H60)
        house_to_hex.Add("B", &HE0)
        house_to_hex.Add("C", &H20)
        house_to_hex.Add("D", &HA0)
        house_to_hex.Add("E", &H10)
        house_to_hex.Add("F", &H90)
        house_to_hex.Add("G", &H50)
        house_to_hex.Add("H", &HD0)
        house_to_hex.Add("I", &H70)
        house_to_hex.Add("J", &HF0)
        house_to_hex.Add("K", &H30)
        house_to_hex.Add("L", &HB0)
        house_to_hex.Add("M", &H0)
        house_to_hex.Add("N", &H80)
        house_to_hex.Add("O", &H40)
        house_to_hex.Add("P", &HC0)

        device_to_hex.Add("1", &H6)
        device_to_hex.Add("2", &HE)
        device_to_hex.Add("3", &H2)
        device_to_hex.Add("4", &HA)
        device_to_hex.Add("5", &H1)
        device_to_hex.Add("6", &H9)
        device_to_hex.Add("7", &H5)
        device_to_hex.Add("8", &HD)
        device_to_hex.Add("9", &H7)
        device_to_hex.Add("10", &HF)
        device_to_hex.Add("11", &H3)
        device_to_hex.Add("12", &HB)
        device_to_hex.Add("13", &H0)
        device_to_hex.Add("14", &H8)
        device_to_hex.Add("15", &H4)
        device_to_hex.Add("16", &HC)

        com_to_hex.Add("ALL_UNITS_OFF", &H0)
        com_to_hex.Add("ALL_LIGHTS_ON", &H1)
        com_to_hex.Add("ON", &H2)
        com_to_hex.Add("OFF", &H3)
        com_to_hex.Add("DIM", &H4)
        com_to_hex.Add("BRIGHT", &H5)
        com_to_hex.Add("ALL_LIGHTS_OFF", &H6)
        com_to_hex.Add("EXTENDED_CODE", &H7)
        com_to_hex.Add("HAIL_REQ", &H8)
        com_to_hex.Add("HAIL_ACK", &H9)
        com_to_hex.Add("PRESET_DIM_1", &HA)
        com_to_hex.Add("PRESET_DIM_2", &HB)
        com_to_hex.Add("EXTENDED_DATA_TRANSFER", &HC)
        com_to_hex.Add("STATUS_ON", &HD)
        com_to_hex.Add("STATUS_OFF", &HE)
        com_to_hex.Add("STATUS_REQUEST", &HF)
    End Sub

    Public Function ouvrir(ByVal numero As String) As String
        Try
            If Not port_ouvert Then
                port_name = numero 'pour se rapeller du nom du port
                port.PortName = port_name 'nom du port : COM1
                port.BaudRate = 4800 'vitesse du port 300, 600, 1200, 2400, 9600, 14400, 19200, 38400, 57600, 115200
                port.Parity = IO.Ports.Parity.None 'pas de parité
                port.StopBits = IO.Ports.StopBits.One 'un bit d'arrêt par octet
                port.DataBits = 8 'nombre de bit par octet
                'port.Encoding = System.Text.Encoding.GetEncoding(1252)  'Extended ASCII (8-bits)
                'port.ReadBufferSize = CInt(4096)
                'port.ReceivedBytesThreshold = 1
                port.StopBits = StopBits.One
                port.Handshake = IO.Ports.Handshake.XOnXOff
                port.ReadTimeout = 500
                port.WriteTimeout = 500
                port.Open()
                port_ouvert = True
                'If port.IsOpen Then
                '    port.DtrEnable = True
                '    port.RtsEnable = True
                '    port.DiscardInBuffer()
                'End If
                Return ("Port " & port_name & " ouvert")
            Else
                Return ("ERR: Port " & port_name & " dejà ouvert")
            End If
        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    Public Function fermer() As String
        Try
            If port_ouvert Then
                port_ouvert = False
                'suppression de l'attente de données à lire
                RemoveHandler port.DataReceived, AddressOf DataReceived
                'fermeture des ports
                If (Not (port Is Nothing)) Then ' The COM port exists.
                    If port.IsOpen Then
                        Dim limite As Integer = 0
                        Do While (port.BytesToWrite > 0 And limite < 100) ' Wait for the transmit buffer to empty.
                            limite = limite + 1
                        Loop
                        limite = 0
                        Do While (port.BytesToRead > 0 And limite < 100) ' Wait for the receipt buffer to empty.
                            limite = limite + 1
                        Loop
                        port.Close()
                        port.Dispose()
                        Return ("Port " & port_name & " fermé")
                    End If
                    Return ("Port " & port_name & "  est déjà fermé")
                End If
                Return ("ERR: Port " & port_name & " n'existe pas")
            End If
        Catch ex As UnauthorizedAccessException
            Return ("ERR: Port " & port_name & " IGNORE") ' The port may have been removed. Ignore.
        End Try
        Return True
    End Function

    Public Function lancer() As String
        AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf DataReceived)
        Return "Handler COM OK"
    End Function

    Private Sub DataReceived(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs)
        Try
            While port_ouvert And port.BytesToRead > 0
                ProcessReceivedChar(port.ReadByte())
            End While
        Catch Ex As Exception
            WriteLog("ERR: X10 Datareceived : " & Ex.Message)
        End Try
    End Sub

    Public Sub ProcessReceivedChar(ByVal sComChar As Byte)
        'si Domos est actif
        'If domos_svc.Serv_DOMOS Then
        'rassemble un message complet pour le traiter ensuite avec displaymess
        Dim temp As Byte
        temp = sComChar
        'WriteLog(VB.Right("0" & Hex(temp), 2))
        'End If
    End Sub

    Public Function ecrire(ByVal adresse As String, ByVal commande As String, ByVal data As Integer) As String
        'adresse= adresse du composant : A1
        'commande : ON, OFF...
        'data

        Dim axbData(5) As Byte
        Dim ReadaxbData(1) As Byte
        Dim xbCheckSum As Byte
        Dim nbboucle As Integer
        'Dim isOk As Boolean
        'Dim X10Input As Object
        'Dim XCount As Long

        If port_ouvert Then
            If Not OutPortDevice Then
                OutPortDevice = True

                'suppression de l'attente de données à lire
                'RemoveHandler port.DataReceived, AddressOf DataReceived

                Try
                    'composition des messages à envoyer
                    axbData(0) = STANDARD_ADDRESS
                    axbData(1) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or device_to_hex(Microsoft.VisualBasic.Right(adresse, adresse.Length - 1))
                    axbData(2) = ACK
                    axbData(3) = ((data * 8) And 255) Or STANDARD_FUNCTION
                    axbData(4) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or com_to_hex(commande)
                Catch ex As Exception
                    OutPortDevice = False
                    Return ("ERR: X10: messages non valides : " & adresse & "-" & commande & " --> " & ex.Message)
                End Try

                Try

                    Dim donnee As Byte() = {axbData(0), axbData(1)}
                    nbboucle = 0
                    Do
                        'ecriture de H4 et housecode-devicecode
                        'MsgBox("X10: ecrit : h4 + " & adresse)
                        port.Write(donnee, 0, 2)
                        System.Threading.Thread.Sleep(50)

                        'lecture du checksum renvoyé par le module
                        'MsgBox("X10: lit checksum")
                        ReadaxbData(0) = port.ReadByte()
                        xbCheckSum = (axbData(0) + axbData(1)) And &HFF
                    Loop Until ReadaxbData(0) = xbCheckSum Or nbboucle >= 4
                    'le chesksum n'a jamais été bon
                    If nbboucle >= 4 Then
                        OutPortDevice = False
                        Return ("ERR: X10 : cheksum non valide")
                    End If

                    'on envoie le ack
                    'MsgBox("X10 : ecrit : ACK")
                    Dim donnee2 As Byte() = {axbData(2)}
                    port.Write(donnee2, 0, 1)

                    'on lit la reponse Interface ready
                    'System.Threading.Thread.Sleep(500)
                    'ReadaxbData(0) = port.ReadByte()
                    'If (ReadaxbData(0) <> INFERFACE_READY) Then
                    '    OutPortDevice = False
                    '    Return ("ERR: X10: INTERFACE NOT READY")
                    'End If

                    Dim donnee3 As Byte() = {axbData(3), axbData(4)}
                    nbboucle = 0
                    Do
                        'ecriture de la H6 + valeur du DIM et housecode-commandecode
                        'MsgBox("X10 : ecrit : h6 + Dim:" & data)
                        port.Write(donnee3, 0, 2)
                        System.Threading.Thread.Sleep(50)

                        'lecture du checksum renvoyé par le module
                        'MsgBox("X10 : lit checksum")
                        ReadaxbData(0) = port.ReadByte()
                        xbCheckSum = (axbData(3) + axbData(4)) And &HFF
                    Loop Until ReadaxbData(0) = xbCheckSum Or nbboucle >= 4
                    'le chesksum n'a jamais été bon
                    If nbboucle >= 4 Then
                        OutPortDevice = False
                        Return ("ERR: X10 : cheksum non valide")
                    End If

                    'on envoie le ack
                    'MsgBox("X10 : ecrit : ACK")
                    port.Write(donnee2, 0, 1)

                    'on lit la reponse Interface ready
                    'System.Threading.Thread.Sleep(500)
                    'ReadaxbData(0) = port.ReadByte()
                    'If (ReadaxbData(0) <> INFERFACE_READY) Then
                    '    OutPortDevice = False
                    '    Return ("ERR: X10: INTERFACE NOT READY")
                    'End If

                Catch ex As Exception
                    Return ("ERR: X10: " & ex.Message)
                End Try




                'Try
                '    axbData(0) = STANDARD_ADDRESS
                '    axbData(1) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or device_to_hex(Microsoft.VisualBasic.Right(adresse, adresse.Length - 1))
                '    xbCheckSum = (axbData(0) + axbData(1)) And &HFF
                'Catch ex As Exception
                '    MsgBox("X10 : " & adresse & " non valide")
                '    _err = True
                'End Try
                'If Not _err Then

                '    Try
                '        'ecriture de H4 et housecode-devicecode
                '        Do
                '            Dim donnee() As Byte = {axbData(0), axbData(1)}
                '            port.Write(donnee, 0, donnee.Length)
                '            MsgBox("X10 : ecrit : " & adresse & " --> " & commande)
                '            System.Threading.Thread.Sleep(50)
                '            'X10Input = port.ReadByte()
                '            'ReadaxbData(0) = CByte(X10Input(0))
                '            ReadaxbData(0) = port.ReadByte()
                '            MsgBox("1")
                '            If ReadaxbData(0) = INTERFACE_CQ Or ReadaxbData(0) = CM11_CLOCK_REQ Or ReadaxbData(0) = CP10_CLOCK_REQ Then
                '                wait(100) 'on attend 1s
                '                X10HandleRing()
                '                ReadaxbData(0) = 0 : Exit Do
                '            End If
                '            XCount = XCount + 1
                '            MsgBox("2")
                '        Loop Until (ReadaxbData(0) = xbCheckSum) Or XCount = 10
                '        MsgBox("3")
                '        If ReadaxbData(0) = xbCheckSum Then
                '            isOk = True
                '            ReadaxbData(0) = ACK
                '            Dim donnee2() As Byte = {ReadaxbData(0)}
                '            port.Write(donnee2, 0, donnee2.Length)
                '            MsgBox("4")
                '            System.Threading.Thread.Sleep(500)
                '            'X10Input = port.ReadByte()
                '            'ReadaxbData(0) = CByte(X10Input(0))
                '            'ReadaxbData(0) = port.ReadByte()
                '            'If (ReadaxbData(0) <> INFERFACE_READY) And ReadaxbData(0) <> ACK Then isOk = False
                '        End If
                '        MsgBox("5")
                '        If isOk Then
                '            axbData(0) = ((com_to_hex(commande) * 8) And 255) Or STANDARD_FUNCTION
                '            axbData(1) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or com_to_hex(commande)
                '            xbCheckSum = (axbData(0) + axbData(1)) And &HFF
                '            MsgBox("6")
                '            Do
                '                Dim donnee() As Byte = {axbData(0), axbData(1)}
                '                port.Write(donnee, 0, donnee.Length)
                '                wait(3) 'on attend 0.03s
                '                MsgBox("7")
                '                'X10Input = port.ReadByte()
                '                'ReadaxbData(0) = CByte(X10Input(0))
                '                ReadaxbData(0) = port.ReadByte()
                '                MsgBox("8")
                '                If ReadaxbData(0) = INTERFACE_CQ Or ReadaxbData(0) = CM11_CLOCK_REQ Or ReadaxbData(0) = CP10_CLOCK_REQ Then
                '                    wait(100) 'on attend 1s
                '                    X10HandleRing()
                '                    ReadaxbData(0) = 0 : Exit Do
                '                End If
                '                MsgBox("9")
                '                XCount = XCount + 1
                '            Loop Until (ReadaxbData(0) = xbCheckSum) Or XCount = 10

                '            isOk = False
                '            MsgBox("10")
                '            If ReadaxbData(0) = xbCheckSum Then
                '                MsgBox("11")
                '                isOk = True
                '                ReadaxbData(0) = ACK
                '                Dim donnee2() As Byte = {ReadaxbData(0)}
                '                port.Write(donnee2, 0, donnee2.Length)
                '                MsgBox("12")
                '                wait(100) 'on attend 1s
                '                'X10Input = port.ReadByte()
                '                'ReadaxbData(0) = CByte(X10Input(0))
                '                ReadaxbData(0) = port.ReadByte()
                '                MsgBox("13")
                '                If ReadaxbData(0) <> INFERFACE_READY And ReadaxbData(0) <> ACK Then isOk = False
                '            End If

                '        Else
                '            'OutDeviceX10 = False
                '        End If
                '    Catch ex As Exception
                '        MsgBox("ERR: X10 : " & ex.Message)
                '    End Try
                'Else
                '    MsgBox("ERR: X10 : Données non valide, annulation de la commande")
                'End If
            Else
                OutPortDevice = False
                Return "ERR: X10: ecriture déjà en cours"
            End If
            'AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf DataReceived)

            OutPortDevice = False
            Return "X10: OK"
        Else
            Return "ERR: X10: Port fermé"
        End If

    End Function

    Private Sub X10HandleRing()
        'On Error Resume Next

        'Dim X10Input As Object
        Dim xbHail(0) As Byte

        'X10Input = port.ReadByte()
        'xbHail(0) = CByte(X10Input(0))
        xbHail(0) = port.ReadByte()

        Select Case xbHail(0)
            Case INTERFACE_CQ
                xbHail(0) = COMPUTER_READY
                Dim donnee2() As Byte = {xbHail(0)}
                port.Write(donnee2, 0, donnee2.Length)

                Do
                    'X10Input = port.ReadByte()
                    'xbHail(0) = CByte(X10Input(0))
                    xbHail(0) = port.ReadByte()
                Loop Until (xbHail(0) = INTERFACE_CQ)

            Case CM11_CLOCK_REQ
            Case CP10_CLOCK_REQ
            Case Else
        End Select

    End Sub

    Public Sub WriteLog(ByVal message As String)
        'utilise la fonction de base pour loguer un event
        If STRGS.InStr(message, "ERR:") > 0 Then
            'domos_svc.log("RFX : " & message, 2)
        Else
            'domos_svc.log("RFX : " & message, 9)
        End If
    End Sub

End Class
