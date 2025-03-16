using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using NSSM.Core.Constants.AppConstants;

namespace NSSM.WPF.ViewModels
{
    /// <summary>
    /// View model for the About page
    /// </summary>
    public class AboutViewModel : ViewModelBase
    {
        private string _applicationVersion;
        private string _copyright;
        private string _description;

        /// <summary>
        /// Initializes a new instance of the AboutViewModel class
        /// </summary>
        public AboutViewModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();

            _applicationVersion = $"Version {assemblyName.Version}";
            _description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ??
                "Modern UI for the Non-Sucking Service Manager";
            _copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ??
                $"Copyright © {DateTime.Now.Year}";

            OpenUrlCommand = new RelayCommand( OpenUrl );
        }

        /// <summary>
        /// Gets the application name
        /// </summary>
        public string ApplicationName => DisplayConstants.ApplicationName;

        /// <summary>
        /// Gets the application version
        /// </summary>
        public string ApplicationVersion => _applicationVersion;

        /// <summary>
        /// Gets the copyright information
        /// </summary>
        public string Copyright => _copyright;

        /// <summary>
        /// Gets the application description
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Gets the GitHub repository URL
        /// </summary>
        public string GitHubUrl => "https://github.com/rohit-kuinkel1/Nssm-CSharp";

        /// <summary>
        /// Gets the command for opening a URL
        /// </summary>
        public ICommand OpenUrlCommand { get; }

        /// <summary>
        /// Opens a URL in the default web browser
        /// </summary>
        /// <param name="parameter">URL to open</param>
        private void OpenUrl( object? parameter )
        {
            if( parameter is string url && !string.IsNullOrWhiteSpace( url ) )
            {
                try
                {
                    Process.Start( new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    } );
                }
                catch( Exception )
                {
                    //handle exception if needed
                }
            }
        }
    }
}