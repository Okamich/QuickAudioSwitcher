using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace QuickAudioSwitcher.Core
{
    public enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    [Guid("f8679f50-850a-41cf-9c72-430f290290c8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfig
    {
        [PreserveSig] int GetMixFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr ppFormat);
        [PreserveSig] int GetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] bool bDefault, IntPtr ppFormat);
        [PreserveSig] int ResetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);
        [PreserveSig] int SetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pEndpointFormat, IntPtr pMixFormat);
        [PreserveSig] int GetProcessingPeriod([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] bool bDefault, out IntPtr pmftDefaultPeriod, out IntPtr pmftMinimumPeriod);
        [PreserveSig] int SetProcessingPeriod([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] IntPtr mftPeriod);
        [PreserveSig] int GetShareMode([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pMode);
        [PreserveSig] int SetShareMode([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pMode);
        [PreserveSig] int GetPropertyValue([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] IntPtr pKey, IntPtr pProp);
        [PreserveSig] int SetPropertyValue([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] IntPtr pKey, IntPtr pProp);
        [PreserveSig] int SetDefaultEndpoint([In][MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, [In][MarshalAs(UnmanagedType.U4)] ERole role);
        [PreserveSig] int SetEndpointVisibility([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] bool bVisible);
    }

    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    internal class CPolicyConfigClient { }

    public class AudioEngine : IDisposable, IMMNotificationClient
    {
        private readonly MMDeviceEnumerator _enumerator;
        public event Action? AudioEndpointsChanged;
        public event Action<string>? DefaultDeviceChanged;

        public AudioEngine()
        {
            _enumerator = new MMDeviceEnumerator();
            _enumerator.RegisterEndpointNotificationCallback(this);
        }

        public List<AudioDevice> GetPlaybackDevices()
        {
            var result = new List<AudioDevice>();
            try
            {
                var defaultId = GetDefaultDeviceId();
                var endpoints = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var (sysName, adapterName) = ParseDeviceNames(endpoint);

                        result.Add(new AudioDevice
                        {
                            Id = endpoint.ID,
                            SystemName = sysName,
                            AdapterName = adapterName,
                            IsDefault = string.Equals(endpoint.ID, defaultId, StringComparison.OrdinalIgnoreCase),
                            IsActive = true,
                            Volume = 1.0f,
                            IsMuted = false
                        });
                    }
                    catch
                    {
                        // Skip problematic endpoint
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enumerating devices: {ex.Message}");
            }

            return result;
        }

        public string? GetDefaultDeviceId()
        {
            try
            {
                var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                return defaultDevice?.ID;
            }
            catch
            {
                return null;
            }
        }

        public MMDevice? GetDeviceById(string deviceId)
        {
            try
            {
                return _enumerator.GetDevice(deviceId);
            }
            catch
            {
                return null;
            }
        }

        public async Task SetDefaultDeviceAsync(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return;

            await Task.Run(() =>
            {
                try
                {
                    var policyConfig = (IPolicyConfig)new CPolicyConfigClient();
                    policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole);
                    policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
                    policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to set default audio device: {ex.Message}");
                }
            });

            DefaultDeviceChanged?.Invoke(deviceId);
        }

        public void SetDefaultDevice(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return;

            try
            {
                var policyConfig = (IPolicyConfig)new CPolicyConfigClient();
                policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole);
                policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
                policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications);

                DefaultDeviceChanged?.Invoke(deviceId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set default audio device: {ex.Message}");
                throw;
            }
        }

        public float GetDeviceVolume(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return 1.0f;
            try
            {
                var dev = _enumerator.GetDevice(deviceId);
                return dev?.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 1.0f;
            }
            catch
            {
                return 1.0f;
            }
        }

        public float GetCurrentMasterVolume()
        {
            try
            {
                var dev = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                return dev?.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 1.0f;
            }
            catch
            {
                return 1.0f;
            }
        }

        public void SetCurrentMasterVolume(float volumeScalar)
        {
            try
            {
                var dev = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                if (dev?.AudioEndpointVolume != null)
                {
                    dev.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(volumeScalar, 0.0f, 1.0f);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting volume: {ex.Message}");
            }
        }

        public bool GetCurrentMute()
        {
            try
            {
                var dev = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                return dev?.AudioEndpointVolume?.Mute ?? false;
            }
            catch
            {
                return false;
            }
        }

        public void SetCurrentMute(bool mute)
        {
            try
            {
                var dev = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                if (dev?.AudioEndpointVolume != null)
                {
                    dev.AudioEndpointVolume.Mute = mute;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting mute: {ex.Message}");
            }
        }

        private static (string sysName, string adapter) ParseDeviceNames(MMDevice device)
        {
            string friendly = device.FriendlyName ?? "Audio Device";
            string adapter = string.Empty;

            try
            {
                adapter = device.DeviceFriendlyName ?? string.Empty;
            }
            catch
            {
                // Fallback
            }

            int parenIndex = friendly.IndexOf('(');
            if (parenIndex > 0 && friendly.EndsWith(")"))
            {
                string namePart = friendly.Substring(0, parenIndex).Trim();
                string insideParen = friendly.Substring(parenIndex + 1, friendly.Length - parenIndex - 2).Trim();
                return (namePart, insideParen);
            }

            return (friendly, adapter);
        }

        #region IMMNotificationClient implementation
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow == DataFlow.Render && role == Role.Multimedia)
            {
                DefaultDeviceChanged?.Invoke(defaultDeviceId);
            }
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            AudioEndpointsChanged?.Invoke();
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            AudioEndpointsChanged?.Invoke();
        }

        public void OnDeviceRemoved(string deviceId)
        {
            AudioEndpointsChanged?.Invoke();
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            // Property changed (e.g. format, volume, name)
        }
        #endregion

        public void Dispose()
        {
            try
            {
                _enumerator.UnregisterEndpointNotificationCallback(this);
                _enumerator.Dispose();
            }
            catch
            {
                // Suppress on shutdown
            }
        }
    }
}
