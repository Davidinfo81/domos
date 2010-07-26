<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class notify
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(notify))
        Me.Domosmenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.OpenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.StartServiceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.StartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.StopToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RestartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.Domosnotify = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.BTN_minimize = New System.Windows.Forms.Button
        Me.BTN_exit = New System.Windows.Forms.Button
        Me.LogsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.Domosmenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'Domosmenu
        '
        Me.Domosmenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenToolStripMenuItem, Me.StartServiceToolStripMenuItem, Me.LogsToolStripMenuItem, Me.ExitToolStripMenuItem})
        Me.Domosmenu.Name = "ContextMenuStrip1"
        Me.Domosmenu.Size = New System.Drawing.Size(121, 92)
        '
        'OpenToolStripMenuItem
        '
        Me.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem"
        Me.OpenToolStripMenuItem.Size = New System.Drawing.Size(120, 22)
        Me.OpenToolStripMenuItem.Text = "Open"
        '
        'StartServiceToolStripMenuItem
        '
        Me.StartServiceToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StartToolStripMenuItem, Me.StopToolStripMenuItem, Me.RestartToolStripMenuItem})
        Me.StartServiceToolStripMenuItem.Name = "StartServiceToolStripMenuItem"
        Me.StartServiceToolStripMenuItem.Size = New System.Drawing.Size(120, 22)
        Me.StartServiceToolStripMenuItem.Text = "Service"
        '
        'StartToolStripMenuItem
        '
        Me.StartToolStripMenuItem.Name = "StartToolStripMenuItem"
        Me.StartToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.StartToolStripMenuItem.Text = "Start"
        '
        'StopToolStripMenuItem
        '
        Me.StopToolStripMenuItem.Name = "StopToolStripMenuItem"
        Me.StopToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.StopToolStripMenuItem.Text = "Stop"
        '
        'RestartToolStripMenuItem
        '
        Me.RestartToolStripMenuItem.Name = "RestartToolStripMenuItem"
        Me.RestartToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.RestartToolStripMenuItem.Text = "Restart"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(120, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'Domosnotify
        '
        Me.Domosnotify.ContextMenuStrip = Me.Domosmenu
        Me.Domosnotify.Icon = CType(resources.GetObject("Domosnotify.Icon"), System.Drawing.Icon)
        Me.Domosnotify.Text = "Domos"
        Me.Domosnotify.Visible = True
        '
        'BTN_minimize
        '
        Me.BTN_minimize.Location = New System.Drawing.Point(13, 238)
        Me.BTN_minimize.Name = "BTN_minimize"
        Me.BTN_minimize.Size = New System.Drawing.Size(75, 23)
        Me.BTN_minimize.TabIndex = 1
        Me.BTN_minimize.Text = "Minimize"
        Me.BTN_minimize.UseVisualStyleBackColor = True
        '
        'BTN_exit
        '
        Me.BTN_exit.Location = New System.Drawing.Point(95, 238)
        Me.BTN_exit.Name = "BTN_exit"
        Me.BTN_exit.Size = New System.Drawing.Size(75, 23)
        Me.BTN_exit.TabIndex = 2
        Me.BTN_exit.Text = "Exit"
        Me.BTN_exit.UseVisualStyleBackColor = True
        '
        'LogsToolStripMenuItem
        '
        Me.LogsToolStripMenuItem.Name = "LogsToolStripMenuItem"
        Me.LogsToolStripMenuItem.Size = New System.Drawing.Size(120, 22)
        Me.LogsToolStripMenuItem.Text = "Logs"
        '
        'notify
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Controls.Add(Me.BTN_exit)
        Me.Controls.Add(Me.BTN_minimize)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "notify"
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Domos CLI"
        Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
        Me.Domosmenu.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Domosmenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents Domosnotify As System.Windows.Forms.NotifyIcon
    Friend WithEvents StartServiceToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StartToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents StopToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RestartToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents BTN_minimize As System.Windows.Forms.Button
    Friend WithEvents BTN_exit As System.Windows.Forms.Button
    Friend WithEvents LogsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
