$env:TEMP='D:\CodexProjects\AliceWork\Temp'
$env:TMP=$env:TEMP
$env:UPM_CACHE_ROOT='D:\CodexProjects\ChibiWork\upm'
$aliceRoot=Split-Path $PSScriptRoot -Parent
& 'D:\Unity\6000.4.7f1\Editor\Unity.exe' -batchmode -force-d3d11 -quit -projectPath $aliceRoot -buildTarget WebGL -executeMethod ChibiBuild.Web -logFile (Join-Path $aliceRoot 'QA\build-webgl.log')
