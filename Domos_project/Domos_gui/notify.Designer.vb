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
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ConfigurationToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.StartServiceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.StartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.StopToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RestartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.EtatToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ActionsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.SQLToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OptimiseToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.PurgeDesLogsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ReconnectToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TablesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.AfficherToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MAJToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MAJComposantsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MAJCmpBannisToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MAJMacrosToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MAJTimersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.LogsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TestsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.X10openToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.X10closeToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.X10a1onToolstripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.X10a1offToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.Domosnotify = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.BTN_minimize = New System.Windows.Forms.Button
        Me.BTN_exit = New System.Windows.Forms.Button
        Me.Domosmenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'Domosmenu
        '
        Me.Domosmenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AboutToolStripMenuItem, Me.ConfigurationToolStripMenuItem, Me.StartServiceToolStripMenuItem, Me.ActionsToolStripMenuItem, Me.LogsToolStripMenuItem, Me.ExitToolStripMenuItem, Me.TestsToolStripMenuItem})
        Me.Domosmenu.Name = "ContextMenuStrip1"
        Me.Domosmenu.Size = New System.Drawing.Size(153, 180)
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Image = CType(resources.GetObject("AboutToolStripMenuItem.Image"), System.Drawing.Image)
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.AboutToolStripMenuItem.Text = "About"
        '
        'ConfigurationToolStripMenuItem
        '
        Me.ConfigurationToolStripMenuItem.Image = CType(resources.GetObject("ConfigurationToolStripMenuItem.Image"), System.Drawing.Image)
        Me.ConfigurationToolStripMenuItem.Name = "ConfigurationToolStripMenuItem"
        Me.ConfigurationToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ConfigurationToolStripMenuItem.Text = "Configuration"
        '
        'StartServiceToolStripMenuItem
        '
        Me.StartServiceToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StartToolStripMenuItem, Me.StopToolStripMenuItem, Me.RestartToolStripMenuItem, Me.EtatToolStripMenuItem})
        Me.StartServiceToolStripMenuItem.Image = CType(resources.GetObject("StartServiceToolStripMenuItem.Image"), System.Drawing.Image)
        Me.StartServiceToolStripMenuItem.Name = "StartServiceToolStripMenuItem"
        Me.StartServiceToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.StartServiceToolStripMenuItem.Text = "Service"
        '
        'StartToolStripMenuItem
        '
        Me.StartToolStripMenuItem.Image = CType(resources.GetObject("StartToolStripMenuItem.Image"), System.Drawing.Image)
        Me.StartToolStripMenuItem.Name = "StartToolStripMenuItem"
        Me.StartToolStripMenuItem.Size = New System.Drawing.Size(110, 22)
        Me.StartToolStripMenuItem.Text = "Start"
        '
        'StopToolStripMenuItem
        '
        Me.StopToolStripMenuItem.Image = CType(resources.GetObject("StopToolStripMenuItem.Image"), System.Drawing.Image)
        Me.StopToolStripMenuItem.Name = "StopToolStripMenuItem"
        Me.StopToolStripMenuItem.Size = New System.Drawing.Size(110, 22)
        Me.StopToolStripMenuItem.Text = "Stop"
        '
        'RestartToolStripMenuItem
        '
        Me.RestartToolStripMenuItem.Image = CType(resources.GetObject("RestartToolStripMenuItem.Image"), System.Drawing.Image)
        Me.RestartToolStripMenuItem.Name = "RestartToolStripMenuItem"
        Me.RestartToolStripMenuItem.Size = New System.Drawing.Size(110, 22)
        Me.RestartToolStripMenuItem.Text = "Restart"
        '
        'EtatToolStripMenuItem
        '
        Me.EtatToolStripMenuItem.Name = "EtatToolStripMenuItem"
        Me.EtatToolStripMenuItem.Size = New System.Drawing.Size(110, 22)
        Me.EtatToolStripMenuItem.Text = "Etat : "
        '
        'ActionsToolStripMenuItem
        '
        Me.ActionsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SQLToolStripMenuItem, Me.TablesToolStripMenuItem})
        Me.ActionsToolStripMenuItem.Image = CType(resources.GetObject("ActionsToolStripMenuItem.Image"), System.Drawing.Image)
        Me.ActionsToolStripMenuItem.Name = "ActionsToolStripMenuItem"
        Me.ActionsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ActionsToolStripMenuItem.Text = "Actions"
        '
        'SQLToolStripMenuItem
        '
        Me.SQLToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OptimiseToolStripMenuItem, Me.PurgeDesLogsToolStripMenuItem, Me.ReconnectToolStripMenuItem})
        Me.SQLToolStripMenuItem.Image = CType(resources.GetObject("SQLToolStripMenuItem.Image"), System.Drawing.Image)
        Me.SQLToolStripMenuItem.Name = "SQLToolStripMenuItem"
        Me.SQLToolStripMenuItem.Size = New System.Drawing.Size(118, 22)
        Me.SQLToolStripMenuItem.Text = "MY-SQL"
        '
        'OptimiseToolStripMenuItem
        '
        Me.OptimiseToolStripMenuItem.Name = "OptimiseToolStripMenuItem"
        Me.OptimiseToolStripMenuItem.Size = New System.Drawing.Size(151, 22)
        Me.OptimiseToolStripMenuItem.Text = "Optimise"
        '
        'PurgeDesLogsToolStripMenuItem
        '
        Me.PurgeDesLogsToolStripMenuItem.Name = "PurgeDesLogsToolStripMenuItem"
        Me.PurgeDesLogsToolStripMenuItem.Size = New System.Drawing.Size(151, 22)
        Me.PurgeDesLogsToolStripMenuItem.Text = "Purge des logs"
        '
        'ReconnectToolStripMenuItem
        '
        Me.ReconnectToolStripMenuItem.Name = "ReconnectToolStripMenuItem"
        Me.ReconnectToolStripMenuItem.Size = New System.Drawing.Size(151, 22)
        Me.ReconnectToolStripMenuItem.Text = "Reconnect"
        '
        'TablesToolStripMenuItem
        '
        Me.TablesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AfficherToolStripMenuItem, Me.MAJToolStripMenuItem, Me.MAJComposantsToolStripMenuItem, Me.MAJCmpBannisToolStripMenuItem, Me.MAJMacrosToolStripMenuItem, Me.MAJTimersToolStripMenuItem})
        Me.TablesToolStripMenuItem.Image = CType(resources.GetObject("TablesToolStripMenuItem.Image"), System.Drawing.Image)
        Me.TablesToolStripMenuItem.Name = "TablesToolStripMenuItem"
        Me.TablesToolStripMenuItem.Size = New System.Drawing.Size(118, 22)
        Me.TablesToolStripMenuItem.Text = "Tables"
        '
        'AfficherToolStripMenuItem
        '
        Me.AfficherToolStripMenuItem.Name = "AfficherToolStripMenuItem"
        Me.AfficherToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.AfficherToolStripMenuItem.Text = "Afficher"
        '
        'MAJToolStripMenuItem
        '
        Me.MAJToolStripMenuItem.Name = "MAJToolStripMenuItem"
        Me.MAJToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.MAJToolStripMenuItem.Text = "MAJ - ALL"
        '
        'MAJComposantsToolStripMenuItem
        '
        Me.MAJComposantsToolStripMenuItem.Name = "MAJComposantsToolStripMenuItem"
        Me.MAJComposantsToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.MAJComposantsToolStripMenuItem.Text = "MAJ - Composants"
        '
        'MAJCmpBannisToolStripMenuItem
        '
        Me.MAJCmpBannisToolStripMenuItem.Name = "MAJCmpBannisToolStripMenuItem"
        Me.MAJCmpBannisToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.MAJCmpBannisToolStripMenuItem.Text = "MAJ - Cmp bannis"
        '
        'MAJMacrosToolStripMenuItem
        '
        Me.MAJMacrosToolStripMenuItem.Name = "MAJMacrosToolStripMenuItem"
        Me.MAJMacrosToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.MAJMacrosToolStripMenuItem.Text = "MAJ - Macros"
        '
        'MAJTimersToolStripMenuItem
        '
        Me.MAJTimersToolStripMenuItem.Name = "MAJTimersToolStripMenuItem"
        Me.MAJTimersToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.MAJTimersToolStripMenuItem.Text = "MAJ - Timers"
        '
        'LogsToolStripMenuItem
        '
        Me.LogsToolStripMenuItem.Image = CType(resources.GetObject("LogsToolStripMenuItem.Image"), System.Drawing.Image)
        Me.LogsToolStripMenuItem.Name = "LogsToolStripMenuItem"
        Me.LogsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.LogsToolStripMenuItem.Text = "Logs"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Image = CType(resources.GetObject("ExitToolStripMenuItem.Image"), System.Drawing.Image)
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'TestsToolStripMenuItem
        '
        Me.TestsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.X10openToolStripMenuItem, Me.X10closeToolStripMenuItem, Me.X10a1onToolstripMenuItem, Me.X10a1offToolStripMenuItem})
        Me.TestsToolStripMenuItem.Name = "TestsToolStripMenuItem"
        Me.TestsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.TestsToolStripMenuItem.Text = "tests"
        '
        'X10openToolStripMenuItem
        '
        Me.X10openToolStripMenuItem.Name = "X10openToolStripMenuItem"
        Me.X10openToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.X10openToolStripMenuItem.Text = "open"
        '
        'X10closeToolStripMenuItem
        '
        Me.X10closeToolStripMenuItem.Name = "X10closeToolStripMenuItem"
        Me.X10closeToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.X10closeToolStripMenuItem.Text = "close"
        '
        'X10a1onToolstripMenuItem
        '
        Me.X10a1onToolstripMenuItem.Name = "X10a1onToolstripMenuItem"
        Me.X10a1onToolstripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.X10a1onToolstripMenuItem.Text = "1on"
        '
        'X10a1offToolStripMenuItem
        '
        Me.X10a1offToolStripMenuItem.Name = "X10a1offToolStripMenuItem"
        Me.X10a1offToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.X10a1offToolStripMenuItem.Text = "1off"
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
        Me.BTN_minimize.Location = New System.Drawing.Point(12, 12)
        Me.BTN_minimize.Name = "BTN_minimize"
        Me.BTN_minimize.Size = New System.Drawing.Size(75, 23)
        Me.BTN_minimize.TabIndex = 1
        Me.BTN_minimize.Text = "Minimize"
        Me.BTN_minimize.UseVisualStyleBackColor = True
        '
        'BTN_exit
        '
        Me.BTN_exit.Location = New System.Drawing.Point(98, 12)
        Me.BTN_exit.Name = "BTN_exit"
        Me.BTN_exit.Size = New System.Drawing.Size(75, 23)
        Me.BTN_exit.TabIndex = 2
        Me.BTN_exit.Text = "Exit"
        Me.BTN_exit.UseVisualStyleBackColor = True
        '
        'notify
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(185, 46)
        Me.Controls.Add(Me.BTN_minimize)
        Me.Controls.Add(Me.BTN_exit)
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
    Friend WithEvents BTN_minimize As System.Windows.Forms.Button
    Friend WithEvents BTN_exit As System.Windows.Forms.Button
    Friend WithEvents LogsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ConfigurationToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ActionsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SQLToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OptimiseToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PurgeDesLogsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ReconnectToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TablesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MAJToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AfficherToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EtatToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TestsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MAJComposantsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MAJCmpBannisToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MAJMacrosToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents MAJTimersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents X10openToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents X10closeToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents X10a1onToolstripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents X10a1offToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
