﻿Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.ServiceProcess
Imports Microsoft.Win32

Public Class notify

    Private controller As New ServiceController("DOMOS", ".")
    Dim mysql_ip, mysql_db, mysql_login, mysql_mdp, install_dir As String
    '----------------- FORM MANAGEMENT BUTTONS ----------------------

    Private Sub notify_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
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

    Private Sub Domosnotify_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Domosnotify.MouseClick
        Domosmenu.Show()
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
        Domosmenu.Show()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
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

    Private Sub BTN_exit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BTN_exit.Click
        Application.Exit()
    End Sub

    Private Sub BTN_minimize_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BTN_minimize.Click
        Me.WindowState = FormWindowState.Minimized
        Me.Hide()
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

    '-------------------- Services BUTTONS ------------------------

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
            controller.Refresh()
        Catch ex As Exception
            MsgBox("Error while starting Domos service" & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Start Domos Service")
        End Try
    End Sub

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
            controller.Refresh()
        Catch ex As Exception
            MsgBox("Error while stoping Domos service" & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Stop Domos Service")
        End Try
    End Sub

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
            controller.Refresh()
        Catch ex As Exception
            MsgBox("Erreur lors du redémarrage du service" & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Restart Domos Service")
        End Try
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        About.Show()
    End Sub

    Private Sub LogsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LogsToolStripMenuItem.Click
        Try
            System.Diagnostics.Process.Start(install_dir & "logs")
        Catch ex As Exception
            MsgBox("Error While loading Logs directory : " & install_dir & "logs", MsgBoxStyle.Critical, "ERROR")
        End Try
    End Sub

    Private Sub ConfigurationToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConfigurationToolStripMenuItem.Click
        config.Show()
    End Sub
End Class