Imports STRGS = Microsoft.VisualBasic.Strings
Imports VB = Microsoft.VisualBasic
Imports System.Threading
Imports System.Globalization
Imports ZibaseDll
Imports System.ComponentModel

Public Class zibasemodule : Implements ISynchronizeInvoke
    Public WithEvents zba As New ZibaseDll.ZiBase
    Public Shared table_composants As New DataTable
    Private messagelast, adresselast, valeurlast As String
    Private nblast As Integer = 0

    Public Function BeginInvoke(ByVal method As System.Delegate, ByVal args() As Object) As System.IAsyncResult Implements System.ComponentModel.ISynchronizeInvoke.BeginInvoke
        BeginInvoke = Nothing
    End Function
    Public Function EndInvoke(ByVal result As System.IAsyncResult) As Object Implements System.ComponentModel.ISynchronizeInvoke.EndInvoke
        EndInvoke = Nothing
    End Function
    Public Function Invoke(ByVal method As System.Delegate, ByVal args() As Object) As Object Implements System.ComponentModel.ISynchronizeInvoke.Invoke
        Invoke = Nothing
    End Function
    Public ReadOnly Property InvokeRequired() As Boolean Implements System.ComponentModel.ISynchronizeInvoke.InvokeRequired
        Get
            Return False
        End Get
    End Property

    Public Sub lancer()

        table_composants.Dispose()
        Dim x As New DataColumn
        x.ColumnName = "composant_id"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "composants_adresse"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "composants_modele_norme"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "composants_correction"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "composants_etat"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "lastetat"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "composants_nom"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "composants_etatdate"
        table_composants.Columns.Add(x)
        x = New DataColumn
        x.ColumnName = "composants_precision"
        table_composants.Columns.Add(x)

        Dim newRow As DataRow
        newRow = table_composants.NewRow()
        newRow.Item("composant_id") = "1"
        newRow.Item("composants_nom") = "test temperature"
        newRow.Item("composants_adresse") = "OS439167490_tem"
        newRow.Item("composants_modele_norme") = "ZIB"
        newRow.Item("composants_correction") = ""
        newRow.Item("composants_precision") = "0"
        newRow.Item("composants_etat") = ""
        newRow.Item("composants_nom") = "test temperature"
        newRow.Item("composants_etatdate") = ""
        newRow.Item("lastetat") = ""
        table_composants.Rows.Add(newRow)

        newRow = table_composants.NewRow()
        newRow.Item("composant_id") = "2"
        newRow.Item("composants_nom") = "test humidite"
        newRow.Item("composants_adresse") = "OS439167490_hum"
        newRow.Item("composants_modele_norme") = "ZIB"
        newRow.Item("composants_correction") = ""
        newRow.Item("composants_precision") = "0"
        newRow.Item("composants_etat") = ""
        newRow.Item("composants_nom") = "test humidite"
        newRow.Item("composants_etatdate") = ""
        newRow.Item("lastetat") = ""
        table_composants.Rows.Add(newRow)

        newRow = table_composants.NewRow()
        newRow.Item("composant_id") = "3"
        newRow.Item("composants_nom") = "test temperature 2"
        newRow.Item("composants_adresse") = "OS393897154_tem"
        newRow.Item("composants_modele_norme") = "ZIB"
        newRow.Item("composants_correction") = ""
        newRow.Item("composants_precision") = "0"
        newRow.Item("composants_etat") = ""
        newRow.Item("composants_nom") = "test temperature 2"
        newRow.Item("composants_etatdate") = ""
        newRow.Item("lastetat") = ""
        table_composants.Rows.Add(newRow)

        zba.StartZB()
        MsgBox("Zibase lancée")
    End Sub
    Public Sub lancer_research()
        zba.RestartZibaseSearch()
        MsgBox("Research OK")
    End Sub
    Public Sub fermer()
        zba.StopZB()
        MsgBox("Zibase fermée")
    End Sub

    Private Sub zba_NewSensorDetected(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.NewSensorDetected
        'MsgBox("NEW:" & seInfo.sID & " / " & seInfo.sType & " : " & seInfo.sValue)
        'Form1.ListBox1.Items.Add("NewSensorDetected : " & seInfo.sID & "_" & seInfo.sType & " --> " & seInfo.sValue)
        'Form1.ListBox1.TopIndex = Form1.ListBox1.Items.Count - 1
        'log("NewSensorDetected : " & seInfo.sID & "_" & seInfo.sType & " --> " & seInfo.sValue, "")
        WriteRetour(seInfo.sID & "_" & seInfo.sType, seInfo.sValue)
    End Sub
    Private Sub zba_NewZibaseDetected(ByVal zbInfo As ZibaseDll.ZiBase.ZibaseInfo) Handles zba.NewZibaseDetected
        'MsgBox("NZB:" & zbInfo.sLabelBase & " / " & zbInfo.lIpAddress)
        'Form1.ListBox1.Items.Add("NZB:" & zbInfo.sLabelBase & " / " & zbInfo.lIpAddress)
        'Form1.ListBox1.TopIndex = Form1.ListBox1.Items.Count - 1
        log("NZB:" & zbInfo.sLabelBase & " / " & zbInfo.lIpAddress, "")
    End Sub
    Private Sub zba_UpdateSensorInfo(ByVal seInfo As ZibaseDll.ZiBase.SensorInfo) Handles zba.UpdateSensorInfo
        'MsgBox("UPD:" & seInfo.sID & " / " & seInfo.sType & " : " & seInfo.sValue)
        'Form1.ListBox1.Items.Add("NewSensorDetected : " & seInfo.sID & "_" & seInfo.sType & " --> " & seInfo.sValue)
        'Form1.ListBox1.TopIndex = Form1.ListBox1.Items.Count - 1
        'log("NewSensorDetected : " & seInfo.sID & "_" & seInfo.sType & " --> " & seInfo.sValue, "")
        WriteRetour(seInfo.sID & "_" & seInfo.sType, seInfo.sValue)
    End Sub
    Private Sub zba_WriteMessage(ByVal sMsg As String, ByVal level As Integer) Handles zba.WriteMessage
        MsgBox("write : " & sMsg)
        'Form1.ListBox1.Items.Add("Write:" & sMsg)
        'Form1.ListBox1.TopIndex = Form1.ListBox1.Items.Count - 1
        'log("Write:" & sMsg, "")

    End Sub

    Public Sub log(ByVal message As String, ByVal type As String)
        'Form1.ListBox1.Items.Add(message)
        'Form1.ListBox1.TopIndex = Form1.ListBox1.Items.Count - 1
        MsgBox(message)
        'Form1.log(message)

    End Sub

    Public Sub WriteRetour(ByVal adresse As String, ByVal valeur As String)

        'Forcer le . 
        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
        My.Application.ChangeCulture("en-US")

        Dim tabletmp() As DataRow
        Dim dateheure As String
        Try
            tabletmp = table_composants.Select("composants_adresse = '" & adresse.ToString & "' AND composants_modele_norme = 'ZIB'")
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
                            'If domos_svc.lastetat And valeur.ToString = tabletmp(0)("lastetat").ToString() Then
                            If valeur.ToString = tabletmp(0)("lastetat").ToString() Then
                                log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé lastetat " & tabletmp(0)("lastetat").ToString() & ")", 8)
                                '--- Modification de la date dans la base SQL ---
                                dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                'Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                'If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                            Else
                                'on vérifie que la valeur a changé de plus de composants_precision sinon inchangé
                                If (CDbl(valeur) + CDbl(tabletmp(0)("composants_precision"))) >= CDbl(tabletmp(0)("composants_etat")) And (CDbl(valeur) - CDbl(tabletmp(0)("composants_precision"))) <= CDbl(tabletmp(0)("composants_etat")) Then
                                    'log de "inchangé précision"
                                    log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé precision " & tabletmp(0)("composants_etat").ToString & "+-" & tabletmp(0)("composants_precision").ToString & ")", 8)
                                    '--- Modification de la date dans la base SQL ---
                                    'dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                    'Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                                    'If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                                Else
                                    log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
                                    '  --- modification de l'etat du composant dans la table en memoire ---
                                    tabletmp(0)("lastetat") = tabletmp(0)("composants_etat") 'on garde l'ancien etat en memoire pour le test de lastetat
                                    tabletmp(0)("composants_etat") = valeur.ToString
                                    tabletmp(0)("composants_etatdate") = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                                End If
                            End If
                        Else
                            log("ZIB : string " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString, 6)
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
                        log("ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & tabletmp(0)("composants_adresse").ToString() & " : " & valeur.ToString & " (inchangé " & tabletmp(0)("composants_etat").ToString() & ")", 7)
                        '--- Modification de la date dans la base SQL ---
                        'dateheure = DateAndTime.Now.Year.ToString() & "-" & DateAndTime.Now.Month.ToString() & "-" & DateAndTime.Now.Day.ToString() & " " & STRGS.Left(DateAndTime.Now.TimeOfDay.ToString(), 8)
                        'Err = domos_svc.mysql.mysql_nonquery("UPDATE composants SET composants_etatdate='" & dateheure & "' WHERE composants_id='" & tabletmp(0)("composants_id") & "'")
                        'If Err <> "" Then Log("SQL: table_comp_changed " & Err, 2)
                    End If
                Else
                    'erreur d'acquisition
                    log("ERRRRR : ZIB : " & tabletmp(0)("composants_nom").ToString() & " : " & valeur.ToString, 2)
                End If
            Else
                'log("ZIB : " & adresse.ToString & " non trouvé ", 2)
                'erreur d'adresse composant
                'tabletmp = table_composants_bannis.Select("composants_bannis_adresse = '" & adresse.ToString & "' AND composants_bannis_norme = 'RFX'")
                'If tabletmp.GetUpperBound(0) >= 0 Then
                ''on ne logue pas car c'est une adresse bannie
                'Else
                'domos_svc.log("ZIB : Adresse composant : " & adresse.ToString & " : " & valeur.ToString, 2)
                'End If
            End If
        Catch ex As Exception
            log("ZIB : Writeretour Exception : " & ex.Message, 2)
        End Try
        adresselast = adresse
        valeurlast = valeur
        nblast = 1

    End Sub

End Class
