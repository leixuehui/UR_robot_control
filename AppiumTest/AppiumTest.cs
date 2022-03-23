using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Management;
//using System.Runtime.InteropServices;
using System.Diagnostics;
//using OpenQA.Selenium.Appium.Interfaces;
//using OpenQA.Selenium.Appium.MultiTouch;
//using OpenQA.Selenium.Remote;
using TouchAction = OpenQA.Selenium.Appium.MultiTouch.TouchAction;
//using System.Windows.Forms;
using OpenCvSharp;
using System.Drawing;
using LogManage;
using static Detect.SquareDetect;
using static Config.ConfigParam;
using System.Drawing.Imaging;

namespace AppiumTest
{
    public class AppTest
    {
        public enum retcode
        {
            ERROR_MOK = 0,
            ERROR_FOLDER_EXISTS = -10,
            ERROR_NO_INPUT_DIALOG = -11,
            ERROR_FAIL_TO_ENTER_VERIFY = -12,
            ERROR_NO_RESULT = -13,
            ERROR_NOT_MATCH = 2,
            ERROR_MATCH = 1,
            ERROR_QUICK = 3,
            ERROR_PARTIAL = 4,
            ERROR_LIGHT = 5,
            ERROR_FAKE = 6,
            ERROR_ELEMENT_NOT_FOUND = 7,
            ERROR_OTHER = 8,
            ERROR_FAIL_TO_CONNECT = 9,
            ERROR_FORCE_QUIT = 10,
            ERROR_UNEXPECTED_DESKTOP = 11,
            ERROR_UNEXPECTED_SMALLWINDOW = 12,
            ERROR_UNEXPECTED_SCREENSHOT = 13,
            ERROR_SERVER_NO_RESPONSE = 14
        }
        public enum screencode
        {
            SYS_DESKTOP = 0,
            APP_HOME = 1,
            APP_VERIFY = 2,
            APP_SETTING = 3,
            UNKNOWN = 4,
            SYS_SCREENSHOT =5,
            SYS_SMALLWINDOW = 6

        }
        public static AndroidDriver<AppiumWebElement> _driver;
        //private static AppiumLocalService _appiumLocalService;
        public static int width = 0, height = 0;
        public static string deviceId = "";
        public static string fingerId = "";
        public static bool Setup()
        {
            deviceId = GetDeviceID().Split('\n')[1].Trim().Split('\t')[0];
            if (deviceId == "")
                return false;

            var appiumOptions = new AppiumOptions();
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.DeviceName, deviceId);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "Android");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformVersion, "10");
            appiumOptions.AddAdditionalCapability("appPackage", "com.arcsoft.fingerprint_app");
            appiumOptions.AddAdditionalCapability("appActivity", "com.arcsoft.home.HomeActivity");
           //appiumOptions.AddAdditionalCapability("automationName", "UiAutomator1");
            //options.AddAdditionalCapability("noSign", "True");
            appiumOptions.AddAdditionalCapability("noReset", "True");
            appiumOptions.AddAdditionalCapability("unlockType", "password");
            appiumOptions.AddAdditionalCapability("unlockKey", "2580");
            appiumOptions.AddAdditionalCapability("newCommandTimeout", 600);
            appiumOptions.AddAdditionalCapability("unicodeKeyboard", "True");
            appiumOptions.AddAdditionalCapability("resetKeyboard", "True");
            try
            {
                Uri url = new Uri("http://127.0.0.1:4723/wd/hub");
                _driver = new AndroidDriver<AppiumWebElement>(url, appiumOptions);
                //_driver = new AndroidDriver<AppiumWebElement>(_appiumLocalService, appiumOptions);
                //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                
                width = _driver.Manage().Window.Size.Width;
                height = _driver.Manage().Window.Size.Height;
               
            }
            catch(Exception e)
            {
                Log.E("AppiumException", e.ToString());
                return false;
            }            
            return true;
        }
        
        public static retcode SaveData()
        {
            Log.I("Appium", "设置保存simulator数据");
            retcode ret = WaitFindAndClick(_driver, By.Id("com.arcsoft.fingerprint_app:id/btn_setting"), 3);
            if (ret != retcode.ERROR_MOK)
            {
                Log.E("Appium", "未找到setting按钮");
                return ret;
            }
            try
            {
                while (true)//一直等待直到能找到按钮
                {
                    if (WaitUntil("com.arcsoft.fingerprint_app:id/cb_dump_simulate") == true)
                        break;
                    else
                    {
                        //注册手指多时，按钮需要滚到页面下面才能找到
                        TouchAction touchAction = new TouchAction(_driver);
                        touchAction.Press(width / 2, height * 3 / 4);
                        touchAction.Wait(200);
                        touchAction.MoveTo(width / 2, height * 1 / 4);
                        touchAction.Release();
                        touchAction.Perform();
                        //_driver.Swipe(Startx, Starty, Startx, Endy, 3000);
                    }
                }
                var btn_simu = _driver.FindElementById("com.arcsoft.fingerprint_app:id/cb_dump_simulate");
                btn_simu.Click();
            }
            catch (Exception e)
            {
                Log.E("AppiumException", e.ToString());
                Log.E("AppiumException", "未找到保存数据按钮");
                return retcode.ERROR_ELEMENT_NOT_FOUND;
            }
            ret = WaitFindAndClick(_driver, By.Id("com.arcsoft.fingerprint_app:id/iv_back"), 3);
            if (ret != retcode.ERROR_MOK)
            {
                Log.E("Appium", "未找到返回按钮");
                return ret;
            }
            return retcode.ERROR_MOK;
        }

        public static retcode BackHome()//待优化
        {
            Log.I("Appium", "返回Home页面");
            By by = By.Id("com.arcsoft.fingerprint_app:id/iv_back");
            By byOK = By.Id("com.arcsoft.fingerprint_app:id/dialog_list_tv_top");
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
                wait.Until(ExpectedConditions.ElementToBeClickable(by));
                var control = _driver.FindElement(by);
                if (control.Displayed)
                {
                    control.Click();
                    try
                    {
                        wait.Until(ExpectedConditions.ElementToBeClickable(byOK));
                        var ok = _driver.FindElement(byOK);
                        if (ok.Displayed)
                        {
                            ok.Click();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.E("AppiumException", "未找到返回对话框确定按钮");
                        Log.E("AppiumException", e.ToString());
                        return retcode.ERROR_ELEMENT_NOT_FOUND;
                    }
                    return retcode.ERROR_MOK;
                }
            }
            catch (Exception e)
            {
                Log.E("Appium", "未找到back按钮");
                Log.E("Appium", e.ToString());
                Unexpected();
                BackHome();//再次调用返回首页，然后进行下一个假手指测试
            }
            
            return retcode.ERROR_MOK;
        }
        public static retcode EntryVerify()
        {
            Log.I("Appium", "进入识别页面");
            retcode ret = WaitFindAndClick(_driver, By.Id("com.arcsoft.fingerprint_app:id/btn_cam"), 3);
            if (ret != retcode.ERROR_MOK)
            {
                Log.E("Appium", "未找到识别按钮");
                return ret;
            }
            return retcode.ERROR_MOK;
        }
        public static retcode CheckDuplicated()
        {
            retcode ret = WaitFindAndClick(_driver, By.Id("android:id/button1"), 3);
            if (ret != retcode.ERROR_MOK)
            {
                Log.E("Appium", "未找到重复确定按钮");
                return ret;
            }
            Log.E("Appium", "文件名重复");
            return retcode.ERROR_FOLDER_EXISTS;
            
        }
        public static retcode EnterName(string name)
        {
            //截屏幕分析是否在app界面上
            try
            {
                fingerId = name;
                retcode ret = WaitFindAndInput(_driver, By.Id("com.arcsoft.fingerprint_app:id/dialog_et_content"), 3, name);
                if (ret != retcode.ERROR_MOK)
                {
                    Log.E("Appium", "未找到文本编辑控件");
                    return ret;
                }
                _driver.FindElementById("com.arcsoft.fingerprint_app:id/dialog_list_tv_top").Click();
                
                if (CheckDuplicated() == retcode.ERROR_FOLDER_EXISTS)
                {
                    ret = WaitFindAndInput(_driver, By.Id("com.arcsoft.fingerprint_app:id/dialog_et_content"), 3, name+Log.GetTime());
                    if (ret != retcode.ERROR_MOK)
                    {
                        Log.E("Appium", "未找到文本编辑控件");
                        return ret;
                    }
                    _driver.FindElementById("com.arcsoft.fingerprint_app:id/dialog_list_tv_top").Click();
                    Log.I("Appium", "输入文件名+时间戳");
                }
                return retcode.ERROR_MOK;
            }
            catch (Exception e)
            {
                Log.E("AppiumException", e.ToString());
                return retcode.ERROR_ELEMENT_NOT_FOUND;
            }
        }
        
        public static retcode GetResult()
        {
            try
            {
                //string loc = "new UiSelector().resourceId(\"com.arcsoft.fingerprint_app:id/tv_tip_tile\")";
                //var text_value = _driver.FindElementByAndroidUIAutomator(loc);
                string result = WaitFindAndGetText(_driver, By.Id("com.arcsoft.fingerprint_app:id/tv_tip_tile"), 1);
                if(result == string.Empty)
                {
                    Log.E("Appium", "没有找到识别结果");
                   
                    //1.截屏->save 2.server不响应->Restart 3.app处于后台->Recover
                    //Recover();
                    retcode ret = Unexpected();
                    return ret;
                }
           
                Log.I("Appium", result);
                if (result == "按压时请稍稍用力")
                    return retcode.ERROR_LIGHT;
                else if (result == "不匹配")
                    return retcode.ERROR_NOT_MATCH;
                else if (result == "识别成功")
                    return retcode.ERROR_MATCH;
                else if (result == "请勿过快移动手指")
                    return retcode.ERROR_QUICK;
                else if (result == "按压时请将手指完全覆盖指纹识别区")
                    return retcode.ERROR_PARTIAL;
                else if (result == "非真手指")
                    return retcode.ERROR_FAKE;
                else
                    return retcode.ERROR_OTHER;
            }
            catch (Exception e)
            {
                Log.E("AppiumException", e.ToString());
                //1.截屏->save 2.server不响应->Restart 3.app处于后台->Recover
                //Restart();
                retcode ret = Unexpected();
                return ret;

            }
            //return retcode.ERROR_NO_RESULT;
        }
        public static Dictionary<string, int> GetCount()//未找到结果时？？？？？？？？？
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            string result = WaitFindAndGetText(_driver, By.Id("com.arcsoft.fingerprint_app:id/tv_verify_number"), 1);
            if (result == string.Empty)
            {
                //1.截屏->save 2.server不响应->Restart 3.app处于后台->Recover 4.小窗
                //Restart();
                Log.E("Appium", "未找到测试总结果");
                retcode ret = Unexpected();
                
                return d;
            }
            try
            {
                string[] p = result.Split();
                string[] q = p[1].Split(new char[2] { ',', ':' }); //Regex.Split(p[1], ":", RegexOptions.IgnoreCase);
                string key = "";
                for (int i = 0; i < q.Length; i++)
                {
                    if (i % 2 == 0)
                        key = q[i];
                    else
                    {
                        int value = int.Parse(q[i]);
                        d.Add(key, value);
                    }
                }
            }
            catch (Exception e)
            {
                Log.E("AppiumException", e.ToString());
                Log.E("AppiumException", "未找到测试总结果");
                //1.截屏->save 2.server不响应->Restart 3.检测在桌面app处于后台->Recover 4.小窗
                //Restart();
                retcode ret = Unexpected();
                return d;
            }
            return d;//字典
        }
        public static retcode Unexpected()
        {
            string path = "screenshot/getresultfailed" + Log.GetTime() + ".bmp";
            //string path = "screenshot/6.png";
            screencode code = screencode.UNKNOWN;
            try
            {
                _driver.GetScreenshot().SaveAsFile(path, ScreenshotImageFormat.Bmp);
                code = GetColor(path);
            }
            catch(Exception e)
            {
                code = screencode.APP_VERIFY;
                Log.E("Appium", e.ToString());
            }
            
            if(code==screencode.APP_VERIFY)
            {
                Restart();
                Log.E("Appium", "手机处于识别界面，但appium server不响应");
                return retcode.ERROR_SERVER_NO_RESPONSE;//在识别界面，但是appium server不响应
            }
            else if(code == screencode.SYS_SCREENSHOT)
            {
                //是否在截屏
                try
                {
                    By by = By.Id("com.meizu.media.gallery:id/save");
                    WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                    wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(by));
                    var control = _driver.FindElement(by);
                    if (control.Displayed)
                    {
                        control.Click();
                        return retcode.ERROR_UNEXPECTED_SCREENSHOT;
                    }

                }
                catch (Exception e)
                {
                    Log.E("AppiumException", e.ToString());
                }
            }
            //是否有小窗
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("com.flyme.systemuitools:id/slide_icon")));
                Log.E("Appium", "手机开启了小窗");
                _driver.PressKeyCode(4);//返回键
                return retcode.ERROR_UNEXPECTED_SMALLWINDOW;
            }
            catch (Exception e)
            {
                Log.E("AppiumException", e.ToString());
            }
            /*
            //是否在桌面
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id("com.meizu.flyme.launcher:id/workspace")));
                Log.E("Appium", "手机处于桌面");
                Recover();
                return retcode.ERROR_UNEXPECTED_DESKTOP;
            }
            catch (Exception e)
            {
                Log.E("AppiumException", e.ToString());
            }*/
            Log.E("AppiumException", "无法判断出错原因");
            Recover();
            return retcode.ERROR_OTHER;
        }
        public static retcode Recover()
        {
            _driver.ActivateApp("com.arcsoft.fingerprint_app");
            Log.I("Appium", "重新使app前置");
            SaveData();
            EntryVerify();
            EnterName(fingerId + Log.GetTime());

            return retcode.ERROR_MOK;
        }
        public static bool Restart()//不重新启动app的情况下重新连接
        {
            if (deviceId == "")
                return false;
            //QuitDriver();
            var appiumOptions = new AppiumOptions();
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.DeviceName, deviceId);
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformName, "Android");
            appiumOptions.AddAdditionalCapability(MobileCapabilityType.PlatformVersion, "10");
            appiumOptions.AddAdditionalCapability("appPackage", "com.arcsoft.fingerprint_app");
            appiumOptions.AddAdditionalCapability("appActivity", "com.arcsoft.home.HomeActivity");
            appiumOptions.AddAdditionalCapability("autoLaunch", "False");
            //appiumOptions.AddAdditionalCapability("automationName", "UiAutomator1");
            //options.AddAdditionalCapability("noSign", "True");
            appiumOptions.AddAdditionalCapability("noReset", "True");
            appiumOptions.AddAdditionalCapability("unlockType", "password");
            appiumOptions.AddAdditionalCapability("unlockKey", "2580");
            appiumOptions.AddAdditionalCapability("newCommandTimeout", 600);
            appiumOptions.AddAdditionalCapability("unicodeKeyboard", "True");
            appiumOptions.AddAdditionalCapability("resetKeyboard", "True");
            try
            {
                Uri url = new Uri("http://127.0.0.1:4723/wd/hub");
                _driver = new AndroidDriver<AppiumWebElement>(url, appiumOptions);
            }
            catch (Exception e)
            {
                Log.E("AppiumException", e.ToString());
                return false;
            }
            Log.I("Appium", "Restart driver");
            return true;
        }
        //[TearDown]
        public static void QuitDriver()
        {
            if (_driver != null)
            {
                _driver.Quit();
                Log.I("Appium","Quit driver");
            }
        }
        private static bool WaitUntil(string id)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.Id(id)));
            }
            catch(Exception e)
            {
                Log.E("AppiumException",e.ToString());
                return false;
            }
            return true;
        }
        public static string WaitFindAndGetText(IWebDriver parent, By by, int counter)
        {
            bool Displayed = false;
            for (int v = 0; v < counter; counter++)
            {
                try
                {
                    WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                    wait.Until(ExpectedConditions.ElementExists(by));
                    //wait.Until((d) => { return parent.FindElement(by); });
                    var control = parent.FindElement(by);
                    Displayed = control.Displayed;
                   
                    if (Displayed)
                    {
                        return control.Text;
                    }
                }
                catch (Exception e)
                {
                    Log.E("AppiumException", e.ToString());
                    return string.Empty;
                }
            }
            return string.Empty;
        }
        public static retcode WaitFindAndClick(IWebDriver parent, By by, int counter)
        {
            bool Displayed = false;
            for (int v = 0; v < counter; counter++)
            {
                try
                {
                    WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                    wait.Until(ExpectedConditions.ElementToBeClickable(by));
                    //wait.Until((d) => { return parent.FindElement(by); });
                    var control = parent.FindElement(by);
                    Displayed = control.Displayed;
                    if (Displayed)
                    {
                        control.Click();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Log.E("AppiumException", e.ToString());
                    return retcode.ERROR_ELEMENT_NOT_FOUND;
                }
            }
            return retcode.ERROR_MOK;
        }
        public static retcode WaitFindAndInput(IWebDriver parent, By by, int counter, string name)
        {
            bool Displayed = false;
            for (int v = 0; v < counter; counter++)
            {
                try
                {
                    WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(2));
                    wait.Until(ExpectedConditions.ElementToBeClickable(by));
                    //wait.Until((d) => { return parent.FindElement(by); });
                    var control = parent.FindElement(by);
                    Displayed = control.Displayed;
                    if (Displayed)
                    {
                        control.Clear();
                        control.SendKeys(name);
                        //_driver.HideKeyboard();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Log.E("AppiumException", e.ToString());
                    return retcode.ERROR_ELEMENT_NOT_FOUND;
                }
            }
            return retcode.ERROR_MOK;
        }
        private static string GetDeviceID()
        {
            String cmd ="adb.exe";
            Process p = new Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo();
            p.StartInfo.FileName = cmd;//设定程序名
            p.StartInfo.Arguments = " devices";
            p.StartInfo.UseShellExecute = false; //关闭shell的使用
            p.StartInfo.RedirectStandardInput = true; //重定向标准输入
            p.StartInfo.RedirectStandardOutput = true; //重定向标准输出
            p.StartInfo.RedirectStandardError = true; //重定向错误输出
            p.StartInfo.CreateNoWindow = true;//设置不显示窗口
            p.Start();
            string id = p.StandardOutput.ReadToEnd();//获取device id
            p.Close();
            return id;
        }
        private static screencode GetColor(string path)
        {
            //Bitmap bmp24 = new Bitmap(bmp32.Width, bmp32.Height, PixelFormat.Format24bppRgb);
            using (Image bmp32 = Image.FromFile(path))//appium 截屏为32位，转成24位给ImageDetect.dll检测
            using (Bitmap bmp24 = new Bitmap(bmp32.Width, bmp32.Height, PixelFormat.Format24bppRgb))
            using (Graphics g = Graphics.FromImage(bmp24))
            {
                g.DrawImage(bmp32, new Rectangle(0, 0, bmp32.Width, bmp32.Height));
                int black = 0;
                for (int x = 0; x < bmp24.Width; x++)
                {
                    for (int y = 0; y < bmp24.Height; y++)
                    {
                        Color pixelColor = bmp24.GetPixel(x, y);
                        byte red = pixelColor.R;
                        byte green = pixelColor.G;
                        byte blue = pixelColor.B;
                        //Console.WriteLine(string.Format("{0}", red + green + blue));
                        if (red + green + blue < 40)
                        {
                            black++;
                        }
                    }
                }
                double percent = 1.0 * black / (bmp24.Width * bmp24.Height);
                if (percent > 0.85)
                {
                    MYPOINT[] ptCur = new MYPOINT[4];
                    ptCur = ScreenShotDetect(bmp24);
                    if (ptCur[0].x == 0 && ptCur[0].y == 0)
                        return screencode.APP_VERIFY;//是识别界面
                    else
                        return screencode.SYS_SCREENSHOT;//是截屏界面
                }
            }
            return screencode.UNKNOWN;
        }
        public static MYPOINT[] ScreenShotDetect(Bitmap bm)
        {
            MYPOINT[] ptCur = new MYPOINT[4];
            ptCur = image_detect(bm, 742, 1620, 10);
            return ptCur;
        }
    }
}
    
