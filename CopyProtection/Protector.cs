using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;

namespace CopyProtection
{
  /// <summary>
  /// Workflow
  /// 1. User start the SomeApp.exe 
  /// 2. SomeApp.exe adds MARK to the file name 
  /// 3. SomeApp.exe adds MARK + HD serial to the file content
  /// 4. SomeApp.exe kills itself and starts a copy named HDSN-PROTECTION-SomeApp.exe
  /// 5. HDSN-PROTECTION-SomeApp.exe deletes original unprotected file and creates a new SomeApp.exe with HD serial inside 
  /// 6. HDSN-PROTECTION-SomeApp.exe kills itself and starts protected SomeApp.exe 
  /// 7. Now, SomeApp.exe can read its content and compare HD serial inside with serial number of HD where it's currently running
  /// </summary>
  public class Protector
  {
    const string MARK = "HDSN-PROTECTION";

    /// <summary>
    /// Ensure that app is used for the first time or on the same hard drive
    /// </summary>
    /// <returns></returns>
    public static void CheckDrive()
    {
      var serialNumber = GetDrive("Win32_DiskDrive", "SerialNumber");

      if (string.IsNullOrEmpty(serialNumber))
      {
        throw new Exception("No HD");
      }

      var doc = Process.GetCurrentProcess().MainModule.FileName;
      var docLocation = Path.GetDirectoryName(doc);
      var docName = Path.GetFileNameWithoutExtension(doc);
      var docBytes = File.ReadAllBytes(doc);

      // Step 2
      // If EXE name contains MARK, close the app and run correct EXE

      if (docName.Contains(MARK))
      {
        Run(doc, Path.ChangeExtension(doc.Replace(MARK, string.Empty), "exe"), docBytes);
        return;
      }

      // Clean up temporary EXE files 
      // User shouldn't guess which part of the app does the check

      var docReplacement = Path.ChangeExtension(string.Format("{0}\\{1}{2}", docLocation, MARK, docName), "exe");

      if (docName.Contains(MARK) == false && File.Exists(docReplacement))
      {
        File.Delete(docReplacement);
      }

      // Step 3
      // Check presence of the MARK

      var docContent = Encoding.Default.GetString(docBytes);

      if (docContent.Contains(MARK))
      {
        // File contains MARK 
        // Compare HD serial number after MARK with the current HD 

        var docParts = docContent.Split(new string[] { MARK }, StringSplitOptions.None);

        if (Equals(docParts.LastOrDefault(), serialNumber) == false)
        {
          throw new Exception("Illegal copy");
        }

        return;
      }

      // Step 1
      // If EXE is running for the first time, create a temporary EXE with HD mark and rune it instead
      // We can't modify content of the current EXE when it's running, so we create a temporary helper to restart the app after addition of HD mark

      Run(doc, docReplacement, docBytes.Concat(Encoding.Default.GetBytes(MARK + serialNumber)));
    }

    /// <summary>
    /// Copy content to a new EXE and start it
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="content"></param>
    private static void Run(string source, string destination, IEnumerable<byte> content)
    {
      var process = Process.GetCurrentProcess();

      File.WriteAllBytes(destination, content.ToArray());
      Process.Start(destination);
      process.Kill();
    }

    /// <summary>
    /// Get HD number
    /// </summary>
    /// <param name="wmiClass"></param>
    /// <param name="wmiProperty"></param>
    /// <returns></returns>
    private static string GetDrive(string wmiClass, string wmiProperty)
    {
      var response = string.Empty;

      using (var mc = new ManagementClass(wmiClass))
      using (var moc = mc.GetInstances())
      {
        foreach (ManagementObject mo in moc)
        {
          if (response == string.Empty)
          {
            try
            {
              response = $"{ mo[wmiProperty] }";
              break;
            }
            catch { }
          }
        }
      }

      return response;
    }
  }
}
