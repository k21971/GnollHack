echo Start executing copy-assetpack.bat

if not exist ..\xpl\GnollHackX\GnollHackX.Android\assetpacks mkdir ..\xpl\GnollHackX\GnollHackX.Android\assetpacks
copy c:\wsl-in\assetpack\installtimeassetpack.zip ..\xpl\GnollHackX\GnollHackX.Android\assetpacks
copy c:\wsl-in\assetpack2\ondemandassetpack.zip ..\xpl\GnollHackX\GnollHackX.Android\assetpacks

rem if not exist ..\xpl\GnollHackM\Platforms\Android\assetpacks mkdir ..\xpl\GnollHackM\Platforms\Android\assetpacks
rem copy c:\wsl-in\assetpack-maui\installtimeassetpack.zip ..\xpl\GnollHackM\Platforms\Android\assetpacks
rem copy c:\wsl-in\assetpack-maui2\ondemandassetpack.zip ..\xpl\GnollHackM\Platforms\Android\assetpacks