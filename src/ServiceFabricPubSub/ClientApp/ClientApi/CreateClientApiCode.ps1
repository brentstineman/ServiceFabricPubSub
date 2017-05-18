# This script creates the client for the FrontEnd API automatically
# it uses AUTOREST (https://github.com/Azure/AutoRest)
#   - starting from json file from swagger creates C# files
#  to run first you need to install AUTOREST with npm install -g autorest
#  then deploy the FrontEndType service and execute this script

$autorestExe= $env:APPDATA + "\npm\autorest.cmd";
$swaggerAdminFile = "swagger_admin_api.json";
$swaggerRouterFile = "swagger_router_api.json";

#clean old definition files
If (Test-Path $swaggerAdminFile){
	Remove-Item $swaggerAdminFile
}

If (Test-Path $swaggerRouterFile){
	Remove-Item $swaggerRouterFile
}

Remove-Item *.log
Remove-Item *.cs

Invoke-WebRequest -Uri http://localhost:8979/swagger/docs/v2 -OutFile $swaggerAdminFile;
Invoke-WebRequest -Uri http://localhost:8277/swagger/docs/v1 -OutFile $swaggerRouterFile;

Start-Process -FilePath $autorestExe -NoNewWindow -ArgumentList "--input-file=$swaggerAdminFile --csharp --output-folder=. --namespace=ClientApi.Admin" -RedirectStandardOutput log_admin.log -RedirectStandardError logError_admin.log
Start-Process -FilePath $autorestExe -NoNewWindow -ArgumentList "--input-file=$swaggerRouterFile --csharp --output-folder=. --namespace=ClientApi.Router" -RedirectStandardOutput log_router.log -RedirectStandardError logError_router.log
