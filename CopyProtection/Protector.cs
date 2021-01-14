using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;

namespace CopyProtection
{
  public class Protector
  {
    const string PACKER_MARK = "HDSN";
    const string PACKER_START = "START";
    const string PACKER_MESSAGE_SETUP = "SETUP";
    const string PACKER_MESSAGE_ERROR = "ERROR";
    const string PACKER_MESSAGE_SUCCESS = "SUCCESS";

    /// <summary>
    /// Ensure that app is used for the first time or on the same hard drive
    /// </summary>
    /// <returns></returns>
    public static string CheckDrive()
    {
      try
      {
        var location = Assembly.GetEntryAssembly().Location;
        var locationRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var locationName = Path.GetFileName(Assembly.GetEntryAssembly().Location).Replace(".exe", string.Empty);
        var locationReplacement = string.Format("{0}\\{1}{2}.exe", locationRoot, PACKER_START, locationName);

        // Check

        var contentMark = Encoding.Default.GetBytes(PACKER_MARK);
        var contentCurrent = File.ReadAllBytes(location).ToList();
        var contentSerial = GetDrive("Win32_DiskDrive", "SerialNumber");
        var exeMark = contentCurrent.Skip(contentCurrent.Count - contentMark.Length).Take(contentMark.Length).ToArray();
        var exeSerial = contentCurrent.Skip(contentCurrent.Count - contentMark.Length - contentSerial.Length).Take(contentSerial.Length).ToArray();

        if (Encoding.Default.GetString(exeMark).Equals(PACKER_MARK))
        {
          if (locationName.Substring(0, PACKER_START.Length).Equals(PACKER_START))
          {
            var locationSetup = string.Format("{0}\\{1}.exe", locationRoot, locationName.Substring(PACKER_START.Length));
            var process = Process.GetCurrentProcess();

            File.WriteAllBytes(locationSetup, contentCurrent.ToArray());
            Process.Start(locationSetup);
            process.Kill();
          }
          else
          {
            File.Delete(locationReplacement);
          }

          return Encoding.Default.GetString(exeSerial).Equals(contentSerial) ? PACKER_MESSAGE_SUCCESS : PACKER_MESSAGE_ERROR;
        }
        else
        {
          contentCurrent.AddRange(Encoding.Default.GetBytes(contentSerial));
          contentCurrent.AddRange(contentMark);
          File.WriteAllBytes(locationReplacement, contentCurrent.ToArray());
          Process.Start(locationReplacement);
          Process.GetCurrentProcess().Kill();
        }
      }
      catch (Exception e)
      {
        return e.Message;
      }

      return PACKER_MESSAGE_SETUP;
    }

    /// <summary>
    /// Get HD number
    /// </summary>
    /// <param name="wmiClass"></param>
    /// <param name="wmiProperty"></param>
    /// <returns></returns>
    private static string GetDrive(string wmiClass, string wmiProperty)
    {
      var result = string.Empty;
      var mc = new ManagementClass(wmiClass);
      var moc = mc.GetInstances();

      foreach (ManagementObject mo in moc)
      {
        if (result == string.Empty)
        {
          try
          {
            result = $"{ mo[wmiProperty] }";
            break;
          }
          catch { }
        }
      }

      return result;
    }
  }
}
