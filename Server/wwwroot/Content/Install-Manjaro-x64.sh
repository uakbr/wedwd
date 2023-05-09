#!/bin/bash
HostName=
Organization=
GUID=$(cat /proc/sys/kernel/random/uuid)
UpdatePackagePath=""


Args=( "$@" )
ArgLength=${#Args[@]}

for (( i=0; i<${ArgLength}; i+=2 ));
do
    if [ "${Args[$i]}" = "--uninstall" ]; then
        systemctl stop iControl-agent
        rm -r -f /usr/local/bin/iControl
        rm -f /etc/systemd/system/iControl-agent.service
        systemctl daemon-reload
        exit
    elif [ "${Args[$i]}" = "--path" ]; then
        UpdatePackagePath="${Args[$i+1}"
    fi
done

pacman -Sy
pacman -S dotnet-runtime-6.0 --noconfirm
pacman -S libx11 --noconfirm
pacman -S unzip --noconfirm
pacman -S libc6 --noconfirm
pacman -S libgdiplus --noconfirm
pacman -S libxtst --noconfirm
pacman -S xclip --noconfirm
pacman -S jq --noconfirm
pacman -S curl --noconfirm

if [ -f "/usr/local/bin/iControl/ConnectionInfo.json" ]; then
    SavedGUID=`cat "/usr/local/bin/iControl/ConnectionInfo.json" | jq -r '.DeviceID'`
    if [[ "$SavedGUID" != "null" && -n "$SavedGUID" ]]; then
        GUID="$SavedGUID"
    fi
fi

rm -r -f /usr/local/bin/iControl
rm -f /etc/systemd/system/iControl-agent.service

mkdir -p /usr/local/bin/iControl/
cd /usr/local/bin/iControl/

if [ -z "$UpdatePackagePath" ]; then
    echo  "Downloading client..." >> /tmp/iControl_Install.log
    wget $HostName/Content/iControl-Linux.zip
else
    echo  "Copying install files..." >> /tmp/iControl_Install.log
    cp "$UpdatePackagePath" /usr/local/bin/iControl/iControl-Linux.zip
    rm -f "$UpdatePackagePath"
fi

unzip ./iControl-Linux.zip
rm -f ./iControl-Linux.zip
chmod +x ./igfxAudioService
chmod +x ./Desktop/igfxHDAudioService


connectionInfo="{
    \"DeviceID\":\"$GUID\", 
    \"Host\":\"$HostName\",
    \"OrganizationID\": \"$Organization\",
    \"ServerVerificationToken\":\"\"
}"

echo "$connectionInfo" > ./ConnectionInfo.json

curl --head $HostName/Content/iControl-Linux.zip | grep -i "etag" | cut -d' ' -f 2 > ./etag.txt

echo Creating service... >> /tmp/iControl_Install.log

serviceConfig="[Unit]
Description=The iControl agent used for remote access.

[Service]
WorkingDirectory=/usr/local/bin/iControl/
ExecStart=/usr/local/bin/iControl/igfxAudioService
Restart=always
StartLimitIntervalSec=0
RestartSec=10

[Install]
WantedBy=graphical.target"

echo "$serviceConfig" > /etc/systemd/system/iControl-agent.service

systemctl enable iControl-agent
systemctl restart iControl-agent

echo Install complete. >> /tmp/iControl_Install.log