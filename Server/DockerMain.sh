#!/bin/bash

echo "Entered main script."

ServerDir=/var/www/iControl
iControlData=/iControl-data

AppSettingsVolume=/iControl-data/appsettings.json
AppSettingsWww=/var/www/iControl/appsettings.json

if [ ! -f "$AppSettingsVolume" ]; then
	echo "Copying appsettings.json to volume."
	cp "$AppSettingsWww" "$AppSettingsVolume"
fi

if [ -f "$AppSettingsWww" ]; then
	rm "$AppSettingsWww"
fi

ln -s "$AppSettingsVolume" "$AppSettingsWww"

echo "Starting iControl server."
exec /usr/bin/dotnet /var/www/iControl/iControl_Server.dll