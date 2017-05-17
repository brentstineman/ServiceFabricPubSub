# This script creates the client for the FrontEnd API automatically
# it uses AUTOREST (https://github.com/Azure/AutoRest)
#   - starting from json file from swagger creates C# files
#  to run first you need to install AUTOREST with npm install -g autorest
#  then deploy the FrontEndType service and execute this script

$autorestExe= $env:APPDATA + "\npm\autorest.cmd";
$swaggerFile = "swagger_api.json";
$outputCSFile =  "ClientApi.cs";

If (Test-Path $outputCSFile){
	Remove-Item $outputCSFile
}

If (Test-Path $swaggerFile){
	Remove-Item $swaggerFile
}

Invoke-WebRequest -Uri http://localhost:8979/swagger/docs/v2 -OutFile $swaggerFile;
Start-Process -FilePath $autorestExe -NoNewWindow -ArgumentList "--input-file=$swaggerFile --csharp --output-folder=. --namespace=ClientApi" -RedirectStandardOutput log.log -RedirectStandardError logError.log
