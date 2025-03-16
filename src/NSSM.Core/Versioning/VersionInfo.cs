using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NSSM.Core.Versioning;

/// <summary>
/// Provides version information for the NSSM application.
/// Not really related to the core functionality of the tool.
/// This is a C# implementation of the functionality in version.cmd
/// </summary>
public static class VersionInfo
{
    /// <summary>
    /// Gets the full version string for NSSM
    /// </summary>
    public static string Version => GetVersionFromGit();

    /// <summary>
    /// Gets the version information components for use in assembly info
    /// </summary>
    public static (int Major, int Minor, int Build, int Revision) VersionComponents => ParseVersionComponents( Version );

    /// <summary>
    /// Gets the build date
    /// </summary>
    public static string BuildDate => DateTime.Now.ToString( "yyyy-MM-dd" );

    /// <summary>
    /// Gets the copyright information
    /// </summary>
    public static string Copyright => $"Public Domain; Author Iain Patterson 2003-{DateTime.Now.Year}";

    /// <summary>
    /// Gets the file flags for the version info
    /// </summary>
    public static int FileFlags => IsPreRelease() ? 1 : 0; //VS_FF_PRERELEASE = 1

    /// <summary>
    /// Attempts to get the version from git tags
    /// </summary>
    private static string GetVersionFromGit()
    {
        try
        {
            //try to get version from git
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "describe --tags --long",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start( startInfo );
            if( process == null )
                return "0.0-0-prerelease";

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if( string.IsNullOrEmpty( output ) || process.ExitCode != 0 )
                return "0.0-0-prerelease";

            //strip leading v if present
            return output.StartsWith( 'v' ) ? output[1..] : output;
        }
        catch
        {
            //default version if git isn't available
            return "0.0-0-prerelease";
        }
    }

    /// <summary>
    /// Parses the version string into components
    /// </summary>
    private static (int Major, int Minor, int Build, int Revision) ParseVersionComponents( string version )
    {
        //parse version like 2.21-24-g2c60e53
        var match = Regex.Match( version, @"^(\d+)\.(\d+)(?:-(\d+)-([a-z0-9]+))?$" );

        if( !match.Success )
            return (0, 0, 0, 0);

        var major = int.Parse( match.Groups[1].Value );
        var minor = int.Parse( match.Groups[2].Value );

        //if we have the commit count and hash
        var build = match.Groups.Count > 3 && match.Groups[3].Success
            ? int.Parse( match.Groups[3].Value )
            : 0;

        //use the build number from CI if available
        var buildNumberStr = Environment.GetEnvironmentVariable( "BUILD_NUMBER" );
        var revision = !string.IsNullOrEmpty( buildNumberStr ) && int.TryParse( buildNumberStr, out var buildNum )
            ? buildNum
            : 0;

        return (major, minor, build, revision);
    }

    /// <summary>
    /// Determines if this is a pre-release version
    /// </summary>
    private static bool IsPreRelease()
    {
        var version = Version;

        //if it contains prerelease or has commits since the tag
        if( version.Contains( "prerelease", StringComparison.OrdinalIgnoreCase ) )
            return true;

        var match = Regex.Match( version, @"^\d+\.\d+-([1-9]\d*)-[a-z0-9]+$" );
        return match.Success; //if we have commits since the tag (non-zero)
    }
}