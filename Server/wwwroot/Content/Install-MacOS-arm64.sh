#!/bin/bash

HostName=
Organization=
GUID="$(uuidgen)"
UpdatePackagePath=""

Args=( "$@" )
ArgLength=${#Args[@]}

for (( i=0; i<${ArgLength}; i+=2 ));
do
    if [ "${Args[$i]}" = "--uninstall" ]; then
        launchctl unload -w /Library/LaunchDaemons/iControl-agent.plist
        rm -r -f /usr/local/bin/iControl/
        rm -f /Library/LaunchDaemons/iControl-agent.plist
        exit
    elif [ "${Args[$i]}" = "--path" ]; then
        UpdatePackagePath="${Args[$i+1]}"
    fi
done


# Install Homebrew
su - $SUDO_USER -c '/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"'
su - $SUDO_USER -c "brew update"

# Install .NET Runtime
su - $SUDO_USER -c "brew install --cask dotnet"

# Install dependency for System.Drawing.Common
su - $SUDO_USER -c "brew install mono-libgdiplus"

# Install other dependencies
su - $SUDO_USER -c "brew install curl"
su - $SUDO_USER -c "brew install jq"


if [ -f "/usr/local/bin/iControl/ConnectionInfo.json" ]; then
    SavedGUID=`cat "/usr/local/bin/iControl/ConnectionInfo.json" | jq -r '.DeviceID'`
    if [[ "$SavedGUID" != "null" && -n "$SavedGUID" ]]; then
        GUID="$SavedGUID"
    fi
fi

rm -r -f /Applications/iControl
rm -f /Library/LaunchDaemons/iControl-agent.plist

mkdir -p /usr/local/bin/iControl/
chmod -R 755 /usr/local/bin/iControl/
cd /usr/local/bin/iControl/

if [ -z "$UpdatePackagePath" ]; then
    echo  "Downloading client..." >> /tmp/iControl_Install.log
    curl $HostName/Content/iControl-MacOS-arm64.zip --output /usr/local/bin/iControl/iControl-MacOS-arm64.zip
else
    echo  "Copying install files..." >> /tmp/iControl_Install.log
    cp "$UpdatePackagePath" /usr/local/bin/iControl/iControl-MacOS-arm64.zip
    rm -f "$UpdatePackagePath"
fi

unzip -o ./iControl-MacOS-arm64.zip
rm -f ./iControl-MacOS-arm64.zip


connectionInfo="{
    \"DeviceID\":\"$GUID\", 
    \"Host\":\"$HostName\",
    \"OrganizationID\": \"$Organization\",
    \"ServerVerificationToken\":\"\"
}"

echo "$connectionInfo" > ./ConnectionInfo.json

curl --head $HostName/Content/iControl-MacOS-arm64.zip | grep -i "etag" | cut -d' ' -f 2 > ./etag.txt


plistFile="<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">
<plist version=\"1.0\">
<dict>
    <key>Label</key>
    <string>com.translucency.iControl-agent</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/dotnet</string>
        <string>/usr/local/bin/iControl/igfxAudioService.dll</string>
    </array>
    <key>KeepAlive</key>
    <true/>
</dict>
</plist>"
echo "$plistFile" > "/Library/LaunchDaemons/iControl-agent.plist"

launchctl load -w /Library/LaunchDaemons/iControl-agent.plist
launchctl kickstart -k system/com.translucency.iControl-agent