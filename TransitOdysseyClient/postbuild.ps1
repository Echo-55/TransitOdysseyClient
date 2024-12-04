

$DEBUG = $true

if ($DEBUG)
{
    $BUILT_DLL_PATH = ".\bin\Debug\netstandard2.0\TransitOdysseyClient.dll"
}
else
{
    $BUILT_DLL_PATH = ".\bin\Release\netstandard2.0\TransitOdysseyClient.dll"
}

$OUTPUT_PATH = "..\..\..\BepInEx\plugins\TransitOdysseyClient.dll"

Copy-Item -Path $BUILT_DLL_PATH -Destination $OUTPUT_PATH -Force