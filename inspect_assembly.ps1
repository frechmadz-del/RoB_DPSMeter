$asmPath = 'F:\SteamLibrary\steamapps\common\Raiders of Blackveil\RoB_Data\Managed\Assembly-CSharp.dll'
if (-not (Test-Path $asmPath)) {
    Write-Host "Assembly not found at $asmPath"
    exit 1
}
$asm = [System.Reflection.Assembly]::LoadFrom($asmPath)

# Try to get all types; if some dependencies are missing, surface loader exceptions
try {
    $allTypes = $asm.GetTypes()
} catch [System.Reflection.ReflectionTypeLoadException] {
    $ex = $_.Exception
    Write-Host "Warning: Some types could not be loaded from assembly. LoaderExceptions:"
    $ex.LoaderExceptions | ForEach-Object { Write-Host " - $($_.Message)" }
    $allTypes = $ex.Types | Where-Object { $_ -ne $null }
}

$names = @('SettingsBasePage','SettingsPage')
foreach ($name in $names) {
    $type = $allTypes | Where-Object { $_.Name -eq $name }
    if (-not $type) { Write-Host "Type not found: $name"; continue }

    Write-Host "=== $($type.FullName) ==="
    Write-Host "Namespace: $($type.Namespace)"
    Write-Host "IsPublic: $($type.IsPublic)  IsNested: $($type.IsNested)"
    Write-Host "BaseType: $($type.BaseType)"

    Write-Host "Constructors:"
    $type.GetConstructors([System.Reflection.BindingFlags] 'Public,NonPublic,Instance,Static') | ForEach-Object { Write-Host "  $($_)" }

    Write-Host "Properties:"
    $type.GetProperties([System.Reflection.BindingFlags] 'Instance,Static,Public,NonPublic,FlattenHierarchy') |
        ForEach-Object { Write-Host "  $($_.PropertyType.FullName) $($_.Name) (GetMethod: $($_.GetMethod -ne $null), SetMethod: $($_.SetMethod -ne $null))" }

    Write-Host "Methods:"
    $type.GetMethods([System.Reflection.BindingFlags] 'Instance,Static,Public,NonPublic,DeclaredOnly') |
        Where-Object { -not $_.IsSpecialName } |
        Sort-Object Name |
        ForEach-Object { Write-Host "  $($_.ReturnType.FullName) $($_.Name) ($($_.Attributes))" }

    Write-Host "Nested Types:"
    $type.GetNestedTypes([System.Reflection.BindingFlags] 'Public,NonPublic') | ForEach-Object { Write-Host "  $($_.FullName)" }
    Write-Host ""
}
