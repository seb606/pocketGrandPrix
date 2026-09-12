# Script de génération des voix françaises féminines énergiques
Add-Type -AssemblyName System.Speech

$targetDir = "Assets\PocketGP\Resources\Audio"
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
}

$voices = @(
    @{ File = "voice_missile.wav"; Text = "Missile !" },
    @{ File = "voice_huile.wav"; Text = "Huile !" },
    @{ File = "voice_turbo.wav"; Text = "Super Turbo !" },
    @{ File = "voice_bouclier.wav"; Text = "Bouclier !" }
)

$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer
# Sélection de la voix féminine française
try {
    $speak.SelectVoice("Microsoft Hortense Desktop")
} catch {
    Write-Host "Microsoft Hortense non trouvée, recherche d'une voix française..."
    $frVoice = $speak.GetInstalledVoices() | Where-Object { $_.VoiceInfo.Culture.Name -like "fr*" -and $_.VoiceInfo.Gender -eq "Female" } | Select-Object -First 1
    if ($frVoice) {
        $speak.SelectVoice($frVoice.VoiceInfo.Name)
    }
}

# Débit rapide pour une diction arcade énergique
$speak.Rate = 2
$speak.Volume = 100

foreach ($v in $voices) {
    $outFile = Join-Path $targetDir $v.File
    $speak.SetOutputToWaveFile($outFile)
    $speak.Speak($v.Text)
    Write-Host "Généré : $outFile ($($v.Text))"
}

$speak.Dispose()
Write-Host "Toutes les voix ont été générées avec succès !"
