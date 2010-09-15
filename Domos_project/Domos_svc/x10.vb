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
    Dim com_to_hex As New Dictionary(Of String, String)
    Dim hex_to_com As New Dictionary(Of String, String)
    Dim house_to_hex As New Dictionary(Of String, Byte)
    Dim device_to_hex As New Dictionary(Of String, Byte)
    Dim GetPortInput As Boolean
    Dim OutPortDevice As Boolean

#Region "Déclaration"
    'Public Const HouseA As Byte = &H60
    'Public Const HouseB As Byte = &HE0
    'Public Const HouseC As Byte = &H20
    'Public Const HouseD As Byte = &HA0
    'Public Const HouseE As Byte = &H10
    'Public Const HouseF As Byte = &H90
    'Public Const HouseG As Byte = &H50
    'Public Const HouseH As Byte = &HD0
    'Public Const HouseI As Byte = &H70
    'Public Const HouseJ As Byte = &HF0
    'Public Const HouseK As Byte = &H30
    'Public Const HouseL As Byte = &HB0
    'Public Const HouseM As Byte = &H0
    'Public Const HouseN As Byte = &H80
    'Public Const HouseO As Byte = &H40
    'Public Const HouseP As Byte = &HC0

    'Public Const DeviceONE As Byte = &H6
    'Public Const DeviceTWO As Byte = &HE
    'Public Const DeviceTHREE As Byte = &H2
    'Public Const DeviceFOUR As Byte = &HA
    'Public Const DeviceFIVE As Byte = &H1
    'Public Const DeviceSIX As Byte = &H9
    'Public Const DeviceSEVEN As Byte = &H5
    'Public Const DeviceEIGHT As Byte = &HD
    'Public Const DeviceNINE As Byte = &H7
    'Public Const DeviceTEN As Byte = &HF
    'Public Const DeviceELEVEN As Byte = &H3
    'Public Const DeviceTWELVE As Byte = &HB
    'Public Const DeviceTHIRTEEN As Byte = &H0
    'Public Const DeviceFORTEEN As Byte = &H8
    'Public Const DeviceFIFTEEN As Byte = &H4
    'Public Const DeviceSIXTEEN As Byte = &HC

    'Public Const ALL_UNITS_OFF As Byte = &H0
    'Public Const ALL_LIGHTS_ON As Byte = &H1
    'Public Const ONN As Byte = &H2
    'Public Const OFF As Byte = &H3
    'Public Const DIMM As Byte = &H4
    'Public Const BRIGHT As Byte = &H5
    'Public Const ALL_LIGHTS_OFF As Byte = &H6
    'Public Const EXTENDED_CODE As Byte = &H7
    'Public Const HAIL_REQ As Byte = &H8
    'Public Const HAIL_ACK As Byte = &H9
    'Public Const PRESET_DIM_1 As Byte = &HA
    'Public Const PRESET_DIM_2 As Byte = &HB
    'Public Const EXTENDED_DATA_XFER As Byte = &HC
    'Public Const STATUS_REQUEST As Byte = &HF

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

        house_to_hex.Add("A", &H6)
        house_to_hex.Add("B", &HE)
        house_to_hex.Add("C", &H2)
        house_to_hex.Add("D", &HA)
        house_to_hex.Add("E", &H1)
        house_to_hex.Add("F", &H9)
        house_to_hex.Add("G", &H5)
        house_to_hex.Add("H", &HD)
        house_to_hex.Add("I", &H7)
        house_to_hex.Add("J", &HF)
        house_to_hex.Add("K", &H3)
        house_to_hex.Add("L", &HB)
        house_to_hex.Add("M", &H0)
        house_to_hex.Add("N", &H8)
        house_to_hex.Add("O", &H4)
        house_to_hex.Add("P", &HC)

        device_to_hex.Add("1", &H60)
        device_to_hex.Add("2", &HE0)
        device_to_hex.Add("3", &H20)
        device_to_hex.Add("4", &HA0)
        device_to_hex.Add("5", &H10)
        device_to_hex.Add("6", &H90)
        device_to_hex.Add("7", &H50)
        device_to_hex.Add("8", &HD0)
        device_to_hex.Add("9", &H70)
        device_to_hex.Add("10", &HF0)
        device_to_hex.Add("11", &H30)
        device_to_hex.Add("12", &HB0)
        device_to_hex.Add("13", &H0)
        device_to_hex.Add("14", &H80)
        device_to_hex.Add("15", &H40)
        device_to_hex.Add("16", &HC0)

        com_to_hex.Add("ALL_UNITS_OFF", 0)
        com_to_hex.Add("ALL_LIGHTS_ON", 1)
        com_to_hex.Add("ON", 2)
        com_to_hex.Add("OFF", 3)
        com_to_hex.Add("DIM", 4)
        com_to_hex.Add("BRIGHT", 5)
        com_to_hex.Add("ALL_LIGHTS_OFF", 6)
        com_to_hex.Add("EXTENDED_CODE", 7)
        com_to_hex.Add("HAIL_REQ", 8)
        com_to_hex.Add("HAIL_ACK", 9)
        com_to_hex.Add("PRESET_DIM_1", 10)
        com_to_hex.Add("PRESET_DIM_2", 11)
        com_to_hex.Add("EXTENDED_DATA_TRANSFER", 12)
        com_to_hex.Add("STATUS_ON", 13)
        com_to_hex.Add("STATUS_OFF", 14)
        com_to_hex.Add("STATUS_REQUEST", 15)

        hex_to_com.Add(0, "ALL_UNITS_OFF")
        hex_to_com.Add(1, "ALL_LIGHTS_ON")
        hex_to_com.Add(2, "ON")
        hex_to_com.Add(3, "OFF")
        hex_to_com.Add(4, "DIM")
        hex_to_com.Add(5, "BRIGHT")
        hex_to_com.Add(6, "ALL_LIGHTS_OFF")
        hex_to_com.Add(7, "EXTENDED_CODE")
        hex_to_com.Add(8, "HAIL_REQ")
        hex_to_com.Add(9, "HAIL_ACK")
        hex_to_com.Add(10, "PRESET_DIM_1")
        hex_to_com.Add(11, "PRESET_DIM_2")
        hex_to_com.Add(12, "EXTENDED_DATA_TRANSFER")
        hex_to_com.Add(13, "STATUS_ON")
        hex_to_com.Add(14, "STATUS_OFF")
        hex_to_com.Add(15, "STATUS_REQUEST")
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
                port.Encoding = System.Text.Encoding.GetEncoding(1252)  'Extended ASCII (8-bits)
                port.StopBits = StopBits.One
                port.Handshake = Handshake.None
                port.ReadBufferSize = CInt(4096)
                port.ReceivedBytesThreshold = 1
                port.ReadTimeout = 100
                port.WriteTimeout = 500
                port.Open()
                port_ouvert = True
                If port.IsOpen Then
                    port.DtrEnable = True
                    port.RtsEnable = True
                    port.DiscardInBuffer()
                End If
                Return ("Port " & port_name & " ouvert")
            Else
            Return ("Port " & port_name & " dejà ouvert")
            End If
        Catch ex As Exception
            Return ("ERR: " & ex.Message)
        End Try
    End Function

    Public Function fermer() As String
        Try
            If port_ouvert Then
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
                        port_ouvert = False
                        Return ("Port " & port_name & " fermé")
                    End If
                    Return ("Port " & port_name & "  est déjà fermé")
                End If
                Return ("Port " & port_name & " n'existe pas")
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
            WriteLog("ERR: RFXCOM Datareceived : " & Ex.Message)
        End Try
    End Sub

    Public Sub ProcessReceivedChar(ByVal sComChar As Byte)
        'si Domos est actif
        If domos_svc.Serv_DOMOS Then
            'rassemble un message complet pour le traiter ensuite avec displaymess
            Dim temp As Byte
            temp = sComChar
            'WriteLog(VB.Right("0" & Hex(temp), 2))
        End If
    End Sub

    Public Function ecrire(ByVal adresse As String, ByVal commande As String, ByVal data As Integer) As String
        'adresse= adresse du composant : A1
        'commande : ON, OFF...
        'data



        Dim axbData(1) As Byte
        Dim X10Input As Object
        Dim ReadaxbData(0) As Byte
        Dim xbCheckSum As Byte
        Dim XCount As Long
        Dim isOk As Boolean
        Dim _err As Boolean = False

        isOk = False
        If port_ouvert Then
            If Not OutPortDevice Then
                OutPortDevice = True

                'suppression de l'attente de données à lire
                RemoveHandler port.DataReceived, AddressOf DataReceived

                Try
                    axbData(0) = STANDARD_ADDRESS
                    axbData(1) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or device_to_hex(Microsoft.VisualBasic.Right(adresse, adresse.Length - 1))
                    xbCheckSum = (axbData(0) + axbData(1)) And &HFF
                Catch ex As Exception
                    domos_svc.log("X10 : " & adresse & " non valide", 2)
                    _err = True
                End Try
                If Not _err Then

                    Try
                        Do
                            Dim donnee() As Byte = {axbData(0), axbData(1)}
                            port.Write(donnee, 0, donnee.Length)
                            domos_svc.log("X10 : ecrit : " & adresse & " --> " & adresse, 9)
                            domos_svc.wait(3) 'on attend 0.03s
                            X10Input = port.ReadByte()
                            ReadaxbData(0) = CByte(X10Input(0))

                            If ReadaxbData(0) = INTERFACE_CQ Or ReadaxbData(0) = CM11_CLOCK_REQ Or ReadaxbData(0) = CP10_CLOCK_REQ Then
                                domos_svc.wait(100) 'on attend 1s
                                X10HandleRing()
                                ReadaxbData(0) = 0 : Exit Do
                            End If
                            XCount = XCount + 1
                        Loop Until (ReadaxbData(0) = xbCheckSum) Or XCount = 10

                        If ReadaxbData(0) = xbCheckSum Then
                            isOk = True
                            ReadaxbData(0) = ACK
                            Dim donnee2() As Byte = {ReadaxbData(0)}
                            port.Write(donnee2, 0, donnee2.Length)
                            domos_svc.wait(100) 'on attend 1s
                            X10Input = port.ReadByte()
                            ReadaxbData(0) = CByte(X10Input(0))
                            If (ReadaxbData(0) <> INFERFACE_READY) And ReadaxbData(0) <> ACK Then isOk = False
                        End If

                        If isOk Then
                            axbData(0) = ((com_to_hex(commande) * 8) And 255) Or STANDARD_FUNCTION
                            axbData(1) = house_to_hex(Microsoft.VisualBasic.Left(adresse, 1)) Or com_to_hex(commande)
                            xbCheckSum = (axbData(0) + axbData(1)) And &HFF

                            Do
                                Dim donnee() As Byte = {axbData(0), axbData(1)}
                                port.Write(donnee, 0, donnee.Length)
                                domos_svc.wait(3) 'on attend 0.03s

                                X10Input = port.ReadByte()
                                ReadaxbData(0) = CByte(X10Input(0))

                                If ReadaxbData(0) = INTERFACE_CQ Or ReadaxbData(0) = CM11_CLOCK_REQ Or ReadaxbData(0) = CP10_CLOCK_REQ Then
                                    domos_svc.wait(100) 'on attend 1s
                                    X10HandleRing()
                                    ReadaxbData(0) = 0 : Exit Do
                                End If
                                XCount = XCount + 1
                            Loop Until (ReadaxbData(0) = xbCheckSum) Or XCount = 10

                            isOk = False

                            If ReadaxbData(0) = xbCheckSum Then
                                isOk = True
                                ReadaxbData(0) = ACK
                                Dim donnee2() As Byte = {ReadaxbData(0)}
                                port.Write(donnee2, 0, donnee2.Length)
                                domos_svc.wait(100) 'on attend 1s
                                X10Input = port.ReadByte()
                                ReadaxbData(0) = CByte(X10Input(0))

                                If ReadaxbData(0) <> INFERFACE_READY And ReadaxbData(0) <> ACK Then isOk = False
                            End If

                        Else
                            'OutDeviceX10 = False
                        End If
                    Catch ex As Exception
                        Return ("ERR: X10 : " & ex.Message)
                    End Try
                Else
                    Return ("ERR: X10 : Données non valide, annulation de la commande")
                End If

            End If

            OutPortDevice = False
            AddHandler port.DataReceived, New SerialDataReceivedEventHandler(AddressOf DataReceived)
            'OutDeviceX10 = isOk
            Return isOk
        Else
            Return ("ERR: X10 : Port fermé")
        End If

    End Function

    Private Sub X10HandleRing()
        On Error Resume Next

        Dim X10Input As Object
        Dim xbHail(0) As Byte

        X10Input = port.ReadByte()
        xbHail(0) = CByte(X10Input(0))

        Select Case xbHail(0)
            Case INTERFACE_CQ
                xbHail(0) = COMPUTER_READY
                Dim donnee2() As Byte = {xbHail(0)}
                port.Write(donnee2, 0, donnee2.Length)

                Do
                    X10Input = port.ReadByte()
                    xbHail(0) = CByte(X10Input(0))
                Loop Until (xbHail(0) = INTERFACE_CQ)

            Case CM11_CLOCK_REQ
            Case CP10_CLOCK_REQ
            Case Else
        End Select

    End Sub

    Public Sub WriteLog(ByVal message As String)
        'utilise la fonction de base pour loguer un event
        If STRGS.InStr(message, "ERR:") > 0 Then
            domos_svc.log("RFX : " & message, 2)
        Else
            domos_svc.log("RFX : " & message, 9)
        End If
    End Sub

End Class
