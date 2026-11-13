using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickAudioSwitcher.Core
{
    public class AudioDevice : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _systemName = string.Empty;
        private string _adapterName = string.Empty;
        private string _customName = string.Empty;
        private bool _isFavorite;
        private bool _isDefault;
        private bool _isActive = true;
        private float _volume = 1.0f;
        private bool _isMuted;

        public string Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        public string SystemName
        {
            get => _systemName;
            set
            {
                if (_systemName != value)
                {
                    _systemName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string AdapterName
        {
            get => _adapterName;
            set { if (_adapterName != value) { _adapterName = value; OnPropertyChanged(); } }
        }

        public string CustomName
        {
            get => _customName;
            set
            {
                if (_customName != value)
                {
                    _customName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string DisplayName => !string.IsNullOrWhiteSpace(CustomName) ? CustomName : SystemName;

        public bool IsFavorite
        {
            get => _isFavorite;
            set { if (_isFavorite != value) { _isFavorite = value; OnPropertyChanged(); } }
        }

        public bool IsDefault
        {
            get => _isDefault;
            set { if (_isDefault != value) { _isDefault = value; OnPropertyChanged(); } }
        }

        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(); } }
        }

        public float Volume
        {
            get => _volume;
            set { if (Math.Abs(_volume - value) > 0.001f) { _volume = value; OnPropertyChanged(); OnPropertyChanged(nameof(VolumePercent)); } }
        }

        public int VolumePercent => (int)Math.Round(_volume * 100);

        public bool IsMuted
        {
            get => _isMuted;
            set { if (_isMuted != value) { _isMuted = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
