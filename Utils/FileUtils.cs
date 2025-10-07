using System;
using System.IO;

namespace CloverAPI.Utils;

public class FileUtils
{
    public static string[] FindFiles(string fileName)
    {
        if (Path.IsPathRooted(fileName))
        {
            if (File.Exists(fileName))
            {
                return [fileName];
            }

            return [];
        }

        return Directory.GetFiles(Paths.PluginPath, fileName, SearchOption.AllDirectories);
    }

    public static string FindFile(string fileName, string onNotFound = "error", string onMultipleFound = "warn")
    {
        string[] files = FindFiles(fileName);
        if (files.Length < 1)
        {
            if (onNotFound == "error")
            {
                throw new FileNotFoundException("File '" + fileName + "' not found in plugin directory.");
            }

            if (onNotFound == "warn")
            {
                LogWarning("File '" + fileName + "' not found in plugin directory.");
            }

            return null;
        }

        if (files.Length > 1)
        {
            if (onMultipleFound == "error")
            {
                throw new Exception("Multiple files named '" + fileName + "' found in plugin directory.");
            }

            if (onMultipleFound == "warn")
            {
                LogWarning("Multiple files named '" + fileName + "' found. Using the first one found at '" + files[0] +
                           "'.");
            }
        }

        return files[0];
    }

    public static byte[] LoadFileBytes(string filePath, string onNotFound = "error", string onMultipleFound = "warn")
    {
        string foundPath = FindFile(filePath, onNotFound, onMultipleFound);
        if (!File.Exists(foundPath))
        {
            throw new FileNotFoundException("File '" + filePath + "' not found.");
        }

        return File.ReadAllBytes(foundPath);
    }
}