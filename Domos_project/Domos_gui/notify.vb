Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.ServiceProcess
Imports Microsoft.Win32
Imports System.Threading
Imports System.Globalization

Public Class notify

    Private controller As New ServiceController("DOMOS", ".")
    Dim mysql_ip, mysql_db, mysql_login, mysql_mdp, install_dir As String
    '----------------- FORM MANAGEMENT BUTTONS ----------------------
    'chargement du GUI
    Private Sub notify_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Forcer le . 
        Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US")
        My.Application.ChangeCulture("en-US")

        Try
            Dim x = controller.ServiceName
        Catch ex As Exception
            MsgBox("Service Domos don't exist, reinstall DOMOS !", MsgBoxStyle.Critical, "ERROR")
            Application.Exit()
        End Try

        Try
            Dim regKey, regKey2 As RegistryKey
            regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE")
            If regKey Is Nothing Then
                MsgBox("Error Accessing HKLM\Software registry !", MsgBoxStyle.Critical, "ERROR")
                Application.Exit()
            Else
                regKey2 = regKey.OpenSubKey("Domos")
                If regKey2 Is Nothing Then
                    MsgBox("Error Accessing Domos registry configuration ! Reinstall It.", MsgBoxStyle.Critical, "ERROR")
                    Application.Exit()
                Else
                    mysql_ip = regKey2.GetValue("mysql_ip", "127.0.0.1")
                    mysql_db = regKey2.GetValue("mysql_db", "domos")
                    mysql_login = regKey2.GetValue("mysql_login", "domos")
                    mysql_mdp = regKey2.GetValue("mysql_mdp", "domos")
                    install_dir = regKey2.GetValue("install_dir", "C:\Domos\")
                    regKey2.Close()
                End If
                regKey.Close()
            End If
        Catch ex As Exception
            MsgBox("Error while loading Domos GUI !" & Chr(10) & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "ERROR")
            Application.Exit()
        End Try
    End Sub
    'Exit GUI
    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub
    Private Sub BTN_exit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BTN_exit.Click
        Application.Exit()
    End Sub
    'minimize GUI
    Private Sub BTN_minimize_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BTN_minimize.Click
        Me.WindowState = FormWindowState.Minimized
        Me.Hide()
    End Sub
    'Not used now
    Private Sub Domosnotify_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Domosnotify.MouseClick
        'Domosmenu.Show()
    End Sub
    Private Sub Domosnotify_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Domosnotify.MouseDoubleClick
        'Me.Show()
        'If Me.WindowState = FormWindowState.Minimized Then
        '    Me.WindowState = FormWindowState.Normal
        '    Dim boundWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        '    Dim boundHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        '    Dim x As Integer = boundWidth - Me.Width
        '    Dim y As Integer = boundHeight - Me.Height
        '    Me.Location = New Point(x / 2, y / 2)
        'End If
        'Domosmenu.Show()
    End Sub
    Private Sub OpenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        'Me.Show()
        'If Me.WindowState = FormWindowState.Minimized Then
        '    Me.WindowState = FormWindowState.Normal
        '    Dim boundWidth As Integer = Screen.PrimaryScreen.Bounds.Width
        '    Dim boundHeight As Integer = Screen.PrimaryScreen.Bounds.Height
        '    Dim x As Integer = boundWidth - Me.Width
        '    Dim y As Integer = boundHeight - Me.Height
        '    Me.Location = New Point(x / 2, y / 2)
        'End If
    End Sub
    Private Sub notify_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        'If Me.WindowState = FormWindowState.Normal Then
        '    Dim x = MessageBox.Show("Are you sure to exit?" & Chr(10) & Chr(10) & "Click Yes to Exit, No to minimize", "Exit", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
        '    If x = DialogResult.Yes Then
        '        e.Cancel = False
        '    ElseIf x = DialogResult.No Then
        '        e.Cancel = True
        '        Me.WindowState = FormWindowState.Minimized
        '        Me.Hide()
        '    Else
        '        e.Cancel = True
        '    End If
        'End If
    End Sub

    '------------------- DOMOS service  -----------------------
    'On menu opening : Enable menus depending on service state
    Private Sub Domosmenu_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Domosmenu.Opening
        Try
            controller.Refresh()
            EtatToolStripMenuItem.Text = "Etat : " & controller.Status.ToString
            If controller.Status.Equals(ServiceControllerStatus.Running) Then
                StartToolStripMenuItem.Enabled = False
                StopToolStripMenuItem.Enabled = True
                RestartToolStripMenuItem.Enabled = True
                ActionsToolStripMenuItem.Enabled = True
                SQLToolStripMenuItem.Visible = True
                TablesToolStripMenuItem.Visible = True
            ElseIf controller.Status.Equals(ServiceControllerStatus.Stopped) Then
                StartToolStripMenuItem.Enabled = True
                StopToolStripMenuItem.Enabled = False
                RestartToolStripMenuItem.Enabled = False
                ActionsToolStripMenuItem.Enabled = False
                SQLToolStripMenuItem.Visible = False
                TablesToolStripMenuItem.Visible = False
            ElseIf controller.Status.Equals(ServiceControllerStatus.Paused) Then
                StartToolStripMenuItem.Enabled = True
                StopToolStripMenuItem.Enabled = True
                RestartToolStripMenuItem.Enabled = True
                ActionsToolStripMenuItem.Enabled = False
                SQLToolStripMenuItem.Visible = False
                TablesToolStripMenuItem.Visible = False
            ElseIf controller.Status.Equals(ServiceControllerStatus.StopPending) Or controller.Status.Equals(ServiceControllerStatus.PausePending) Or controller.Status.Equals(ServiceControllerStatus.StartPending) Or controller.Status.Equals(ServiceControllerStatus.ContinuePending) Then
                StartToolStripMenuItem.Enabled = False
                StopToolStripMenuItem.Enabled = False
                RestartToolStripMenuItem.Enabled = False
                ActionsToolStripMenuItem.Enabled = False
                SQLToolStripMenuItem.Visible = False
                TablesToolStripMenuItem.Visible = False
            End If
        Catch ex As Exception
            MsgBox("Error : " & ex.Message, MsgBoxStyle.Critical, "ERROR")
        End Try
    End Sub
    'Start domos service
    Private Sub StartToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StartToolStripMenuItem.Click
        'START Service
        Try
            controller.Refresh()
            If controller.Status.Equals(ServiceControllerStatus.StopPending) Or controller.Status.Equals(ServiceControllerStatus.PausePending) Then
                MsgBox("Wait that Domos service to be completely stopped/paused before starting i !t")
            ElseIf controller.Status.Equals(ServiceControllerStatus.Running) Then
                MsgBox("Domos service is already started !")
            ElseIf controller.Status.Equals(ServiceControllerStatus.Paused) Then
                controller.Continue()
            ElseIf controller.Status.Equals(ServiceControllerStatus.Stopped) Then
                controller.Start()
            End If
            'controller.Refresh()
        Catch ex As Exception
            MsgBox("Error while starting Domos service" & Chr(10) & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Start Domos Service")
        End Try
    End Sub
    'stop domos service
    Private Sub StopToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StopToolStripMenuItem.Click
        'STOP Service
        Try
            controller.Refresh()
            If controller.Status.Equals(ServiceControllerStatus.StartPending) Or controller.Status.Equals(ServiceControllerStatus.PausePending) Then
                MsgBox("Wait that Domos service to be completely started/paused before stoping it !")
            ElseIf controller.Status.Equals(ServiceControllerStatus.Stopped) Or controller.Status.Equals(ServiceControllerStatus.StopPending) Then
                MsgBox("Domos service is already stopped/stoping !")
            ElseIf controller.Status.Equals(ServiceControllerStatus.Running) Or controller.Status.Equals(ServiceControllerStatus.Paused) Then
                controller.Stop()
            End If
            'controller.Refresh()
        Catch ex As Exception
            MsgBox("Error while stoping Domos service" & Chr(10) & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Stop Domos Service")
        End Try
    End Sub
    'restart domos service
    Private Sub RestartToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RestartToolStripMenuItem.Click
        'RESTART Service
        Try
            controller.Refresh()
            If controller.Status.Equals(ServiceControllerStatus.StartPending) Or controller.Status.Equals(ServiceControllerStatus.PausePending) Then
                MsgBox("Wait that Domos service to be completely started/paused before restarting it")
            ElseIf controller.Status.Equals(ServiceControllerStatus.StopPending) Then
                MsgBox("Wait that Domos service to be completely stoped before restarting it")
            ElseIf controller.Status.Equals(ServiceControllerStatus.Running) Or controller.Status.Equals(ServiceControllerStatus.Paused) Then
                controller.Stop()
                controller.Refresh()
                controller.WaitForStatus(ServiceControllerStatus.Stopped)
                controller.Refresh()
                controller.Start()
            ElseIf controller.Status.Equals(ServiceControllerStatus.Stopped) Then
                controller.Start()
            End If
            'controller.Refresh()
        Catch ex As Exception
            MsgBox("Error while restarting Domos service" & Chr(10) & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Restart Domos Service")
        End Try
    End Sub

    '------------------------- MYSQL  -------------------------
    'purge des logs
    Private Sub PurgeDesLogsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PurgeDesLogsToolStripMenuItem.Click
        controller.ExecuteCommand(201)
    End Sub
    'optimise tables
    Private Sub OptimiseToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OptimiseToolStripMenuItem.Click
        controller.ExecuteCommand(200)
    End Sub
    'Reconnect SQL
    Private Sub ReconnectToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ReconnectToolStripMenuItem.Click
        controller.ExecuteCommand(202)
    End Sub

    '----------------------- Divers ---------------------------
    'About DOMOS
    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        About.Show()
    End Sub
    'View logs folders
    Private Sub LogsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LogsToolStripMenuItem.Click
        Try
            System.Diagnostics.Process.Start(install_dir & "logs")
        Catch ex As Exception
            MsgBox("Error While loading Logs directory : " & install_dir & "logs", MsgBoxStyle.Critical, "ERROR")
        End Try
    End Sub
    'Modify domos service configuration
    Private Sub ConfigurationToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConfigurationToolStripMenuItem.Click
        config.Show()
    End Sub

    '---------------------- TABLES ---------------------------
    'Maj tables
    Private Sub MAJToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MAJToolStripMenuItem.Click
        controller.ExecuteCommand(211)
    End Sub
    Private Sub MAJComposantsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MAJComposantsToolStripMenuItem.Click
        controller.ExecuteCommand(212)
    End Sub
    Private Sub MAJCmpBannisToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MAJCmpBannisToolStripMenuItem.Click
        controller.ExecuteCommand(213)
    End Sub
    Private Sub MAJMacrosToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MAJMacrosToolStripMenuItem.Click
        controller.ExecuteCommand(214)
    End Sub
    Private Sub MAJTimersToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MAJTimersToolStripMenuItem.Click
        controller.ExecuteCommand(215)
    End Sub
    'View tables in logs
    Private Sub AfficherToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AfficherToolStripMenuItem.Click
        controller.ExecuteCommand(210)
    End Sub

    '---------------------- TESTS ---------------------------
    Dim xx10 As New x10

    Private Sub X10openToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles X10openToolStripMenuItem.Click
        Try
            MsgBox(xx10.ouvrir("COM1"))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub X10closeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles X10closeToolStripMenuItem.Click
         Try
            MsgBox(xx10.fermer())
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub X10a1onToolstripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles X10a1onToolstripMenuItem.Click
        Try
            MsgBox(xx10.ecrire("A1", "ON", 0))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub X10a1offToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles X10a1offToolStripMenuItem.Click
        Try
            MsgBox(xx10.ecrire("A1", "OFF", 0))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
End Class
