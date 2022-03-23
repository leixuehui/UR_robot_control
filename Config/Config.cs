using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toml;

namespace Config
{
    public class ConfigParam
    {
        public static string tomlPath = ".\\cfg\\config.toml";
        //config
        const int num = 6;
        public static double aboveQRCode = 0.0;
        public static double aboveTray = 0.0;
        public static double moveA = 0.0;
        public static double moveV = 0.0;
        public static double[] baseTrayPostion = new double[num];
        public static double aboveFacula = 0.0;
        public static double[] baseFaculaPostion = new double[num];
        public static double[] baseFaculaPostionL = new double[num];
        public static double[] baseFakePostion = new double[num];
        public static int xCount = 0;
        public static int yCount = 0;
        public static int dCount1 = 0;
        public static int dCount2 = 0;
        public static double spaceX = 0.0;
        public static double spaceY = 0.0;
        public static double[] spaceD1 = new double[2];
        public static double[] spaceD2 = new double[2];
        public static double moveUpVSpeed = 0.0;
        public static double moveDownVSpeed = 0.0;
        public static double moveUp = 0.0;
        public static int boxColNum = 0;//行
        public static int boxRowNum = 0;//排
        public static double spaceVertical = 0;
        public static double spaceHorizontal = 0;
        public static double spaceBetweenBox = 0;
        public static string[] namelist = new string[100];
        public static string airPumpServerPath;
        public static int airPumpPort = 0;
        public static int urPort = 30003;
        public static int widthInImage = 0;
        public static int heightInImage = 0;
        public static int error = 0;
        public static int[] ptStd = new int[8];
        public static void LoadConfig()//分到另一个工程
        {
            var doc = Toml.Document.Create(tomlPath);// ".\\cfg\\config_v2.toml");
            aboveQRCode = double.Parse(doc.GetValue("Tray.AboveQRCode").SourceText);
            aboveTray = double.Parse(doc.GetValue("Tray.AboveTray").SourceText);
            for (int i = 0; i < num; i++)
            {
                baseTrayPostion[i] = doc.GetArrayValue<double>("Tray.BasePosition")[i];
            }
            moveA = double.Parse(doc.GetValue("Move.MoveANormalSpeed").SourceText);
            moveV = double.Parse(doc.GetValue("Move.MoveVNormalSpeed").SourceText);
            aboveFacula = double.Parse(doc.GetValue("Facula.AbovePhone").SourceText);
            for (int i = 0; i < num; i++)
            {
                baseFaculaPostion[i] = doc.GetArrayValue<double>("Facula.BasePosition")[i];
            }
            for (int i = 0; i < num; i++)
            {
                baseFakePostion[i] = doc.GetArrayValue<double>("Facula.AboveFakeFinger")[i];
            }
            for (int i = 0; i < num; i++)
            {
                baseFaculaPostionL[i] = doc.GetArrayValue<double>("Facula.BasePositionL")[i];
            }

            xCount = int.Parse(doc.GetValue("Move.X").SourceText);
            yCount = int.Parse(doc.GetValue("Move.Y").SourceText);
            dCount1 = int.Parse(doc.GetValue("Move.D1").SourceText);
            dCount2 = int.Parse(doc.GetValue("Move.D2").SourceText);
            spaceX = double.Parse(doc.GetValue("Move.SpaceX").SourceText);
            spaceY = double.Parse(doc.GetValue("Move.SpaceY").SourceText);

            moveUpVSpeed = double.Parse(doc.GetValue("Move.MoveUpVSpeed").SourceText);
            moveDownVSpeed = double.Parse(doc.GetValue("Move.MoveDownVSpeed").SourceText);

            moveUp = double.Parse(doc.GetValue("Move.MoveUp").SourceText);

            for (int i = 0; i < 2; i++)
            {
                spaceD1[i] = doc.GetArrayValue<double>("Move.SpaceD1")[i];
            }
            for (int i = 0; i < 2; i++)
            {
                spaceD2[i] = doc.GetArrayValue<double>("Move.SpaceD2")[i];
            }
            boxColNum = int.Parse(doc.GetValue("Tray.BoxesVertical").SourceText);
            boxRowNum = int.Parse(doc.GetValue("Tray.BoxesHorizontal").SourceText);
            spaceVertical = double.Parse(doc.GetValue("Tray.SpaceVertical").SourceText);
            spaceHorizontal = double.Parse(doc.GetValue("Tray.SpaceHorizontal").SourceText);
            spaceBetweenBox = double.Parse(doc.GetValue("Tray.SpaceBetweenBox").SourceText);

            for (int i = 0; i < boxColNum * boxRowNum; i++)
            {
                namelist[i] = doc.GetArrayValue<string>("Dev.FakeFingerNameList")[i];
            }
            airPumpServerPath = doc.GetValue("AirPump.ServerPath").SourceText.ToString();
            airPumpPort = int.Parse(doc.GetValue("AirPump.Port").SourceText);

            widthInImage = int.Parse(doc.GetValue("Facula.WidthInImage").SourceText);
            heightInImage = int.Parse(doc.GetValue("Facula.HeightInImage").SourceText);
            error = int.Parse(doc.GetValue("Facula.Error").SourceText);
            for (int i = 0; i < 8; i++)
            {
                ptStd[i] = doc.GetArrayValue<int>("Facula.PositionInImage")[i];

            }
        }
    }
}
