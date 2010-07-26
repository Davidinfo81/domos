Imports STRGS = Microsoft.VisualBasic.Strings
Imports System.ServiceProcess

Public Class notify
    Private controller As New ServiceController

    '----------------- FORM MANAGEMENT BUTTONS ----------------------

    Private Sub notify_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

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
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub

    Private Sub OpenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenToolStripMenuItem.Click
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
            controller.MachineName = "."
            controller.ServiceName = "DOMOS"
            If controller.Status = ServiceControllerStatus.Stopped Or controller.Status = ServiceControllerStatus.Paused Then
                controller.Start()
            End If
        Catch ex As Exception
            MsgBox("Erreur lors du démarrage du service" & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Start Domos Service")
        End Try
    End Sub

    Private Sub StopToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StopToolStripMenuItem.Click
        'STOP Service
        Try
            controller.MachineName = "."
            controller.ServiceName = "DOMOS"
            If controller.Status = ServiceControllerStatus.Running Then
                controller.Stop()
            End If
        Catch ex As Exception
            MsgBox("Erreur lors de l'arret du service" & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Stop Domos Service")
        End Try
    End Sub

    Private Sub RestartToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RestartToolStripMenuItem.Click
        'RESTART Service
        Try
            controller.MachineName = "."
            controller.ServiceName = "DOMOS"
            If controller.Status = ServiceControllerStatus.Running Then
                controller.Stop()
                controller.Start()
            End If
        Catch ex As Exception
            MsgBox("Erreur lors du redémarrage du service" & Chr(10) & ex.ToString, MsgBoxStyle.Critical, "Restart Domos Service")
        End Try
    End Sub

End Class
