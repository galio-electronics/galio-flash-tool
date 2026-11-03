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
    $fileGroup = $form.GetType().GetField('groupBox_fwFile', $fields).GetValue($form)
    $upload = $form.GetType().GetField('btn_upload', $fields).GetValue($form)
    if (!$fileGroup.Enabled -or $upload.Enabled) { throw 'Initial firmware selection state is incorrect' }
    'PASS form construction, rendering and initial controls'
} finally { $form.Dispose() }

