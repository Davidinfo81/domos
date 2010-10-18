'
' La dll ZibaseDll.dll est développée par la société APITRONIC revendeur de la Zibase et de ses accessoires : http://www.planete-domotique.com
'

Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports System.Math
Imports System.Threading
Imports System.Globalization
Imports ZibaseDll

Public Class zibasemodule
    Private WithEvents zba As ZiBase = New ZiBase
    Private messagelast, adresselast, valeurlast As String
    Private nblast As Integer = 0

#Region "Structure"
    Public Structure SensorInfo
        Dim sName As String
        Dim sType As String
        Dim sID As String
        Dim dwValue As Long
        Dim sValue As String
    End Structure

    Public Enum State
        STATE_OFF = 0
        STATE_ON = 1
        STATE_DIM = 3
    End Enum

    Public Enum Protocol
        PROTOCOL_BROADCAST = 0
        PROTOCOL_VISONIC433 = 1
        PROTOCOL_CHACON = 3
        PROTOCOL_DOMIA = 4
        PROTOCOL_X10 = 5
    End Enum
#End Region

    Public Function lancer() As String
        Try
            zba.StartZB()
            Return "Zibase lancée"
        Catch ex As Exception
            Return "ERR: lancement de l'initialisation : " & ex.Message
        End Try
    End Function

    Public Function lancer_research() As String
        Try
            zba.RestartZibaseSearch()
            Return "Research OK"
        Catch ex As Exception
            Return "ERR: research : " & ex.Message
        End Try
    End Function

    Public Function fermer() As String
        Try
            zba.StopZB()
            Return "Zibase fermée"
        Catch ex As Exception
            Return "ERR: Femeture des zibases : " & ex.Message
        End Try
    End Function

    'interroger la zibase sur l'etat d'un device
    Public Function zba_getsensorinfo(ByVal composants_id As Integer) As String
        Dim sei As ZiBase.SensorInfo
        Dim adressetype As String() = Nothing
        Dim tabletmp() As DataRow
        Try
            tabletmp = domos_svc.table_composants.Select("composants_id = '" & composants_id.ToString & "'")
            If tabletmp.GetUpperBound(0) >= 0 Then
                adressetype = Split(tabletmp(0)("composants_addresse"), "_")
                sei = zba.GetSensorInfo(adressetype(0), adressetype(1))
                Return sei.sValue
            Else
                'erreur d'adresse composant
                tabletmp = domos_svc.table_composants_bannis.Select("composants_bannis_id = '" & composants_id.ToString & "'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    'on ne logue pas car c'est une adresse bannie
                    Return ("")
                Else
                    Return ("ERR: GetSensorInfo : Composant non trouvé : " & composants_id.ToString)
                End If
            End If
        Catch ex As Exception
            Return "ERR: GetSensorInfo : " & ex.Message
        End Try
    End Function

    'reception d'une valeur -> analyse
    Private Sub zba_UpdateSensorInfo(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.UpdateSensorInfo
        WriteLog("updatesensorinfo:" & seInfo.sID)
        WriteRetour(seInfo.sID & "_" & seInfo.sType, seInfo.sValue)
    End Sub
    Private Sub zba_NewSensorDetected(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.NewSensorDetected
        'si on detecte une nouveau device
        'Dim message As String
        'message = "Nouveau device : adresse:" & seInfo.sID & "-" & seInfo.sType & " Nom:" & seInfo.sName & " Valeur:" & seInfo.sValue
        'WriteLog("ZIB : " & message)
        WriteLog("updatesensorinfo:" & seInfo.sID)
        WriteRetour(seInfo.sID & "_" & seInfo.sType, seInfo.sValue)
    End Sub

    'nouvelle zibase detecté -> Log
    Private Sub zba_newzibasedetected(ByVal ZiInfo As ZibaseDll.ZiBase.ZibaseInfo) Handles zba.NewZibaseDetected
        WriteLog("Nouvelle Zibase détecté : " & ZiInfo.sLabelBase & "-" & ZiInfo.lIpAddress)
    End Sub

    'la zibase a qqch à logger
    Private Sub ZibaseLog(ByVal sMsg As String, ByVal level As Integer) Handles zba.WriteMessage
        WriteLog("Log : " & sMsg & " - " & level)
    End Sub

    'ecrire device
    Public Function Ecrirecommand(ByVal composants_id As Integer, ByVal ordre As String, ByVal iDim As Integer)
        'composants_id : id du composant
        'ordre : ordre à envoyer
        'iDim: nombre de 0 à 100 pour l'ordre DIM sur chacon
        Dim protocole As ZiBase.Protocol
        Dim adresse(), modele() As String
        Dim tabletmp() As DataRow

        Try
            tabletmp = domos_svc.table_composants.Select("composants_id = '" & composants_id.ToString & "'")
            If tabletmp.GetUpperBound(0) >= 0 Then
                modele = Split(tabletmp(0)("composants_modele_nom"), "_")
                adresse = Split(tabletmp(0)("composants_addresse"), "_")
                Select Case UCase(modele(0))
                    Case "BROADCAST" : protocole = ZiBase.Protocol.PROTOCOL_BROADCAST
                    Case "CHACON" : protocole = ZiBase.Protocol.PROTOCOL_CHACON
                    Case "DOMIA" : protocole = ZiBase.Protocol.PROTOCOL_DOMIA
                    Case "VISONIC433" : protocole = ZiBase.Protocol.PROTOCOL_VISONIC433
                    Case "VISONIC868" : protocole = ZiBase.Protocol.PROTOCOL_VISONIC868
                    Case "X10" : protocole = ZiBase.Protocol.PROTOCOL_X10
                    Case Else
                        Return ("ERR: protocole incorrect : " & adresse(1))
                End Select
                'ecriture sur la zibase
                Select Case UCase(ordre)
                    Case "ON"
                        zba.SendCommand(adresse(0), ZiBase.State.STATE_ON, 0, protocole, 1)
                    Case "OFF"
                        zba.SendCommand(adresse(0), ZiBase.State.STATE_OFF, 0, protocole, 1)
                    Case "DIM"
                        If UCase(adresse(1)) <> "CHACON" Then
                            zba.SendCommand(adresse(0), ZiBase.State.STATE_DIM, 0, protocole, 1)
                        Else
                            zba.SendCommand(adresse(0), ZiBase.State.STATE_DIM, iDim, protocole, 1)
                        End If
                    Case Else
                        Return ("ERR: ordre incorrect : " & ordre)
                End Select
                'modification de l'etat en memoire
                tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                tabletmp(0)("composants_etat") = ordre
                tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                Return (" -> ecrit " & adresse(0) & " -> " & ordre)
            Else
                'erreur d'adresse composant
                tabletmp = domos_svc.table_composants_bannis.Select("composants_bannis_id = '" & composants_id.ToString & "'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    'on ne logue pas car c'est une adresse bannie
                Else
                    Return ("ERR: Composant non trouvé : " & composants_id.ToString & " : " & ordre)
                End If
            End If

            Return " -> zibase write Composants_id: " & composants_id & " -> " & ordre
        Catch ex As Exception
            Return "ERR: Ecrirecommand" & ex.Message
        End Try

    End Function

    'executer un script stocké sur la zibase
    Public Function ExecScript(ByVal sScript As String)
        'sScript : nom du script sur la zibase
        Try
            zba.ExecScript(sScript)
            Return " -> Executé : " & sScript
        Catch ex As Exception
            Return "Err: ExecScript: " & ex.Message
        End Try
    End Function

    'executer un scénario stocké sur la zibase
    Public Function RunScenario(ByVal sCmd As String)
        'sCmd : nom du scenario sur la zibase
        Try
            zba.RunScenario(sCmd)
            Return " -> Executé : " & sCmd
        Catch ex As Exception
            Return "Err: RunScenario: " & ex.Message
        End Try
    End Function

#Region "Write"

    Public Sub WriteLog(ByVal message As String)
        'utilise la fonction de base pour loguer un event
        If STRGS.InStr(message, "ERR:") > 0 Then
            domos_svc.log("ZIB : " & message, 2)
        Else
            domos_svc.log("ZIB : " & message, 9)
        End If
    End Sub

    Public Sub WriteRetour(ByVal adresse As String, ByVal valeur As String)
        'Forcer le . 
        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
        My.Application.ChangeCulture("en-US")

        Dim tabletmp() As DataRow
        Dim dateheure, Err As String
        Try
            tabletmp = domos_svc.table_composants.Select("composants_adresse = '" & adresse.ToString & "' AND composants_modele_norme = 'ZIB'")
            If tabletmp.GetUpperBound(0) >= 0 Then
                If VB.Left(valeur, 4) <> "ERR:" Then 'si y a pas erreur d'acquisition
                    '--- Remplacement de , par .
                    valeur = STRGS.Replace(valeur, ",", ".")
                    '--- Correction si besoin ---
                    If (tabletmp(0)("composants_correction") <> "" And tabletmp(0)("composants_correction") <> "0") Then
                        valeur = valeur + CDbl(tabletmp(0)("composants_correction"))
                    End If
                    '--- comparaison du relevé avec le dernier etat ---
                    '--- si la valeur a changé ou (autre chose qu'un nombre et different de humidité) --- 
                    If valeur.ToString <> tabletmp(0)("composants_etat").ToString() Or (Not IsNumeric(valeur) And (VB.Right(tabletmp(0)("composants_adresse"), 4) <> "_HUM")) Then
                        'si nombre alors 
                        If (IsNumeric(valeur) And IsNumeric(tabletmp(0)("lastetat")) And IsNumeric(tabletmp(0)("composants_etat"))) Then
                            'on vérifie que la valeur a changé par rapport a l'avant dernier etat (lastetat) si domos.lastetat (table config)
                            If domos_svc.lastetat And valeur.ToString = tabletmp(0)("lastetat").ToString() Then
                                domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé lastetat " & tabletmp(0)("lastetat").ToString() & ")", 8)
                                '--- Modification de la date dans la base SQL ---
                                dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                            Else
                                'on vérifie que la valeur a changé de plus de composants_precision sinon inchangé
                                If (CDbl(valeur) + CDbl(tabletmp(0)("composants_precision"))) >= CDbl(tabletmp(0)("composants_etat")) And (CDbl(valeur) - CDbl(tabletmp(0)("composants_precision"))) <= CDbl(tabletmp(0)("composants_etat")) Then
                                    'log de "inchangé précision"
                                    domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé precision " & tabletmp(0)("composants_etat").ToString & "+-" & tabletmp(0)("composants_precision").ToString & ")", 8)
                                    '--- Modification de la date dans la base SQL ---
                                    dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                    Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                    If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                                Else
                                    domos_svc.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
                                    '  --- modification de l'etat du composant dans la table en memoire ---
                                    tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                    tabletmp(0)("composants_etat") = valeur.ToString
                                    tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                End If
                            End If
                        Else
                            domos_svc.log("RFX : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
                            '  --- modification de l'etat du composant dans la table en memoire ---
                            If VB.Left(valeur, 4) <> "CFG:" Then
                                tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                tabletmp(0)("composants_etat") = valeur.ToString
                                tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                            End If
                        End If
                        'Domos.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString)
                        ''  --- modification de l'etat du composant dans la table en memoire ---
                        'tabletmp(0)("composants_etat") = valeur.ToString
                        'tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                    Else
                        'la valeur n'a pas changé, on log en 7 et on maj la date dans la base sql
                        domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé " & tabletmp(0)("composants_etat").ToString() & ")", 7)
                        '--- Modification de la date dans la base SQL ---
                        dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                        Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                        If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                    End If
                Else
                    'erreur d'acquisition
                    domos_svc.log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & valeur.ToString, 2)
                End If
            Else
                'erreur d'adresse composant
                tabletmp = domos_svc.table_composants_bannis.Select("composants_bannis_adresse = '" & adresse.ToString & "' AND composants_bannis_norme = 'RFX'")
                If tabletmp.GetUpperBound(0) >= 0 Then
                    'on ne logue pas car c'est une adresse bannie
                Else
                    domos_svc.log("ZIB : Adresse composant : " & adresse.ToString & " : " & valeur.ToString, 2)
                End If
            End If
        Catch ex As Exception
            domos_svc.log("ZIB : Writeretour Exception : " & ex.Message, 2)
        End Try
        adresselast = adresse
        valeurlast = valeur
        nblast = 1

    End Sub

#End Region

End Class
