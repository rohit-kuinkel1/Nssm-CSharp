namespace NSSM.Core.Helpers
{
    /// <summary>
    /// Provides helper methods for working with file and directory paths.
    /// This class centralizes path-related logic and improves code reuse.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Ensures a path is wrapped in quotes if it contains spaces
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns>The path with quotes if necessary</returns>
        public static string EnsureQuotedPath( string path )
        {
            if( string.IsNullOrEmpty( path ) )
            {
                return path;
            }

            //if the path contains spaces and doesn't already have quotes, add them
            if( path.Contains( " " ) && !( path.StartsWith( "\"" ) && path.EndsWith( "\"" ) ) )
            {
                return $"\"{path}\"";
            }

            return path;
        }

        /// <summary>
        /// Removes quotes from a path if present
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns>The path without quotes</returns>
        public static string RemoveQuotes( string path )
        {
            if( string.IsNullOrEmpty( path ) )
            {
                return path;
            }

            if( path.StartsWith( "\"" ) && path.EndsWith( "\"" ) )
            {
                return path.Substring( 1, path.Length - 2 );
            }

            return path;
        }

        /// <summary>
        /// Combines a file path and command-line arguments safely
        /// </summary>
        /// <param name="path">The path to the executable</param>
        /// <param name="arguments">The command-line arguments</param>
        /// <returns>The combined path and arguments</returns>
        public static string CombinePathAndArguments( string path, string arguments )
        {
            string quotedPath = EnsureQuotedPath( path );

            if( string.IsNullOrEmpty( arguments ) )
            {
                return quotedPath;
            }

            return $"{quotedPath} {arguments}";
        }

        /// <summary>
        /// Splits a command line into path and arguments
        /// </summary>
        /// <param name="commandLine">The full command line</param>
        /// <returns>A tuple containing (path, arguments)</returns>
        public static (string Path, string Arguments) SplitCommandLine( string commandLine )
        {
            if( string.IsNullOrEmpty( commandLine ) )
            {
                return (string.Empty, string.Empty);
            }

            //check if the path is quoted
            if( commandLine.StartsWith( "\"" ) )
            {
                int endQuoteIndex = commandLine.IndexOf( "\"", 1 );
                if( endQuoteIndex > 0 )
                {
                    string path = commandLine.Substring( 1, endQuoteIndex - 1 );
                    string arguments = endQuoteIndex + 1 < commandLine.Length
                        ? commandLine.Substring( endQuoteIndex + 1 ).TrimStart()
                        : string.Empty;

                    return (path, arguments);
                }
            }

            //if not quoted, split on first space
            int spaceIndex = commandLine.IndexOf( " " );
            if( spaceIndex > 0 )
            {
                string path = commandLine.Substring( 0, spaceIndex );
                string arguments = commandLine.Substring( spaceIndex + 1 ).TrimStart();

                return (path, arguments);
            }

            //no arguments
            return (commandLine, string.Empty);
        }

        /// <summary>
        /// Converts a relative path to an absolute path
        /// </summary>
        /// <param name="relativePath">The relative path to convert</param>
        /// <param name="basePath">The base path to use (defaults to current directory)</param>
        /// <returns>The absolute path</returns>
        public static string ToAbsolutePath(
            string relativePath,
            string? basePath = null
        )
        {
            if( string.IsNullOrEmpty( relativePath ) )
            {
                return string.Empty;
            }

            //if already an absolute path, return as is
            if( Path.IsPathRooted( relativePath ) )
            {
                return relativePath;
            }

            //use the provided base path or the current directory
            string baseDirectory = basePath ?? Directory.GetCurrentDirectory();

            return Path.GetFullPath( Path.Combine( baseDirectory, relativePath ) );
        }

        /// <summary>
        /// Ensures a directory exists, creating it if necessary
        /// </summary>
        /// <param name="directoryPath">The path to the directory</param>
        /// <returns>True if the directory exists or was created, false otherwise</returns>
        public static bool EnsureDirectoryExists( string directoryPath )
        {
            if( string.IsNullOrEmpty( directoryPath ) )
            {
                return false;
            }

            try
            {
                if( !Directory.Exists( directoryPath ) )
                {
                    Directory.CreateDirectory( directoryPath );
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that a path is valid and can be accessed
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <param name="mustExist">Whether the path must already exist</param>
        /// <returns>True if the path is valid, false otherwise</returns>
        public static bool IsValidPath(
            string path,
            bool mustExist = true
        )
        {
            if( string.IsNullOrEmpty( path ) )
            {
                return false;
            }

            try
            {
                //will throw if the path contains invalid characters
                Path.GetFullPath( path );

                if( mustExist )
                {
                    return File.Exists( path ) || Directory.Exists( path );
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Expands environment variables in a path
        /// </summary>
        /// <param name="path">The path containing environment variables</param>
        /// <returns>The expanded path</returns>
        public static string ExpandEnvironmentVariables( string path )
        {
            if( string.IsNullOrEmpty( path ) )
            {
                return path;
            }

            return Environment.ExpandEnvironmentVariables( path );
        }
    }
}