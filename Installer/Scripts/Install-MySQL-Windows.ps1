Write-Output "This is MySQL installation script for Windows.";
[String] $choice = Read-Host "Do you have winget? This can make it simpler. y/n";
If ($choice -eq "y") {
	# Use winget
	# WinGetProcess;
	winget install Oracle.MySQL;
	Exit;
}

Write-Output "You will not use WinGet so please install it manually. When finished, hit enter to go back to the main installer.";
Start-Process https://dev.mysql.com/downloads/installer/
Read-Host "Press ENTER when finished...";
Exit;
