$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$root = (Get-Location).Path
[Reflection.Assembly]::LoadFrom((Join-Path $root 'Galio Flash Tool\bin\Release\Galio Flash Tool.exe')) | Out-Null
$form = New-Object Galio_Flash_Tool.Form1
try {
    $form.ShowInTaskbar = $false
    $form.StartPosition = "Manual"
    $form.Location = New-Object Drawing.Point(-10000, -10000)
    $form.Show()
    [Windows.Forms.Application]::DoEvents()
    $bitmap = New-Object Drawing.Bitmap($form.Width, $form.Height)
    try {
        $form.DrawToBitmap($bitmap, (New-Object Drawing.Rectangle(0, 0, $form.Width, $form.Height)))
        $bitmap.Save((Join-Path $root 'tests\bin\form.png'))
    } finally { $bitmap.Dispose() }
    $fields = [Reflection.BindingFlags]'Instance,NonPublic'
    $groups = @('groupBox_fwFile','groupBox_hwInfo','groupBox_uploadProgress') | ForEach-Object {
        $form.GetType().GetField($_, $fields).GetValue($form)
    }
    $upload = $form.GetType().GetField('btn_upload', $fields).GetValue($form)
    if (($groups | Where-Object Enabled) -or $upload.Enabled) { throw 'Programming controls enabled before identification' }
    $form.GetType().GetField('detectedBanner', $fields).SetValue($form, 'name=Test,version=BOOT:1.2,date=x,time=y')
    $refresh = $form.GetType().GetMethod('RefreshControls', $fields)
    $refresh.Invoke($form, @())
    if ($groups | Where-Object { !$_.Enabled }) { throw 'Programming controls disabled after identification' }
    $form.GetType().GetMethod('CloseProgram', $fields).Invoke($form, @())
    $refresh.Invoke($form, @())
    if ($groups | Where-Object Enabled) { throw 'Programming controls enabled after session ended' }
    'PASS form rendering and programming controls before, during and after identification'
} finally { $form.Dispose() }

