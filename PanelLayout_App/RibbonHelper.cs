using System;
using Autodesk.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using Autodesk.AutoCAD.Runtime;
using System.Windows.Input;

namespace PanelLayout_App
{
    public class RibbonHelper
    {
        public static int countOfRibbonCreator = 0;

        public static Autodesk.Windows.RibbonButton checkShellPlanButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton createShellPlanButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton insertCornerButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton insertPanelButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton panelLayoutOptionButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton insertDeckPanelButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton createElevationPlanButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton createHolecenterlineButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton rebuildPanelButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton updateWallButton = new RibbonButton();//Added on 10/06/2023 by SDM        
        public static Autodesk.Windows.RibbonButton createBOMButton = new RibbonButton();
        public static Autodesk.Windows.RibbonButton readText = new RibbonButton();//Added on 19/06/2023 by PRT

        BitmapImage getBitmap(Bitmap bitmap, int height, int width)
        {
            bitmap.MakeTransparent(bitmap.GetPixel(1, 1));
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new MemoryStream(stream.ToArray());
            bmp.DecodePixelHeight = height;
            bmp.DecodePixelWidth = width;
            bmp.EndInit();

            return bmp;
        }
        public void CreateRibbon()
        {
            string errorMsg = "image is not present";
            string titleName = "AEE AFWD";
            RibbonControl panlLaytCtrl = ComponentManager.Ribbon;
            RibbonTab panlLaytTab = new RibbonTab();
            panlLaytTab.Title = titleName;
            panlLaytTab.Id = "ID_ShellPlan";
            panlLaytCtrl.Tabs.Add(panlLaytTab);

            //Add ribbon panel source
            Autodesk.Windows.RibbonPanelSource panlLaytTabRbnPnlSrce = new RibbonPanelSource();
            panlLaytTabRbnPnlSrce.Title = titleName;
            //Add custom ribbon panel
            RibbonPanel panlLaytRbnPanel = new RibbonPanel();
            panlLaytRbnPanel.Source = panlLaytTabRbnPnlSrce;
            panlLaytTab.Panels.Add(panlLaytRbnPanel);


            ////Panel Layout Opetion button
            //Autodesk.Windows.RibbonButton panelLayoutOptionButton = new RibbonButton();
            panelLayoutOptionButton.ToolTip = "Panel Layout Setting";
            panelLayoutOptionButton.Text = "Setting";
            panelLayoutOptionButton.CommandParameter = "PLSETTING ";
            panelLayoutOptionButton.Size = RibbonItemSize.Large;
            //panelLayoutOptionButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            panelLayoutOptionButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            panelLayoutOptionButton.ShowText = true;
            panelLayoutOptionButton.ShowImage = true;
            panelLayoutOptionButton.CommandHandler = new AdskCommandHandler();

            var panelLayoutOptionImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\Settings.jpg");
            if (File.Exists(panelLayoutOptionImagePath))
            {
                Bitmap panelLayoutOptionBitmap = new Bitmap(panelLayoutOptionImagePath);
                panelLayoutOptionButton.Image = getBitmap(panelLayoutOptionBitmap, 16, 16);
                panelLayoutOptionButton.LargeImage = getBitmap(panelLayoutOptionBitmap, 32, 32);
            }
            else
            {
                string msg = "Panel layout option " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            ////Read Text button
            //Autodesk.Windows.RibbonButton ReadDataButton = new RibbonButton();
            readText.ToolTip = "Read Text";
            readText.Text = "Read";
            readText.CommandParameter = "READDATA ";
            readText.Size = RibbonItemSize.Large;
            //clearDataButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            readText.Orientation = System.Windows.Controls.Orientation.Vertical;
            readText.ShowText = true;
            readText.ShowImage = true;
            readText.CommandHandler = new AdskCommandHandler();

            var readSettingsImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\ReadSettings.jpg");
            if (File.Exists(readSettingsImagePath))
            {
                Bitmap readSettingsBitmap = new Bitmap(readSettingsImagePath);
                readText.Image = getBitmap(readSettingsBitmap, 16, 16);
                readText.LargeImage = getBitmap(readSettingsBitmap, 32, 32);
            }
            else
            {
                string msg = "Read Settings " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            ////Check Shell Plan button
            //Autodesk.Windows.RibbonButton checkShellPlanButton = new RibbonButton();
            checkShellPlanButton.ToolTip = "Check Shell Plan";
            checkShellPlanButton.Text = "Check SP";
            checkShellPlanButton.CommandParameter = "CHECKSHELLPLAN ";
            checkShellPlanButton.Size = RibbonItemSize.Large;
            //checkShellPlanButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            checkShellPlanButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            checkShellPlanButton.ShowText = true;
            checkShellPlanButton.ShowImage = true;
            checkShellPlanButton.CommandHandler = new AdskCommandHandler();

            var checkShellPlanImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\CheckShellPlan.jpg");
            if (File.Exists(checkShellPlanImagePath))
            {
                Bitmap checkShellPlanBitmap = new Bitmap(checkShellPlanImagePath);
                checkShellPlanButton.Image = getBitmap(checkShellPlanBitmap, 16, 16);
                checkShellPlanButton.LargeImage = getBitmap(checkShellPlanBitmap, 32, 32);
            }
            else
            {
                string msg = "Check Shell Plan " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            ////Create Shell Plan button
            ////Autodesk.Windows.RibbonButton createShellPlanButton = new RibbonButton();      
            createShellPlanButton.ToolTip = "Create Shell Plan";
            createShellPlanButton.Text = "Create SP";
            createShellPlanButton.CommandParameter = "CREATESHELLPLAN ";
            createShellPlanButton.Size = RibbonItemSize.Large;
            //createShellPlanButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            createShellPlanButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            createShellPlanButton.ShowText = true;
            createShellPlanButton.ShowImage = true;
            createShellPlanButton.CommandHandler = new AdskCommandHandler();

            var createShellPlanImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\CreateShellPlan.jpg");
            if (File.Exists(createShellPlanImagePath))
            {
                Bitmap createShellPlanBitmap = new Bitmap(createShellPlanImagePath);
                createShellPlanButton.Image = getBitmap(createShellPlanBitmap, 16, 16);
                createShellPlanButton.LargeImage = getBitmap(createShellPlanBitmap, 32, 32);
            }
            else
            {
                string msg = "Create Shell Plan " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            ////Create Insert Corner button
            //Autodesk.Windows.RibbonButton insertCornerButton = new RibbonButton();
            insertCornerButton.ToolTip = "Insert Corner";
            insertCornerButton.Text = "Insert Corner";
            insertCornerButton.CommandParameter = "INSERTCORNER ";
            insertCornerButton.Size = RibbonItemSize.Large;
            //insertCornerButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            insertCornerButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            insertCornerButton.ShowText = true;
            insertCornerButton.ShowImage = true;
            insertCornerButton.CommandHandler = new AdskCommandHandler();

            var insertCornerImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\InsertCorners.jpg");
            if (File.Exists(insertCornerImagePath))
            {
                Bitmap insertCornerBitmap = new Bitmap(insertCornerImagePath);
                insertCornerButton.Image = getBitmap(insertCornerBitmap, 16, 16);
                insertCornerButton.LargeImage = getBitmap(insertCornerBitmap, 32, 32);
            }
            else
            {
                string msg = "Insert Corner " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            ////Create Insert Panel button
            //Autodesk.Windows.RibbonButton insertPanelButton = new RibbonButton();
            insertPanelButton.ToolTip = "Insert Wall Panel";
            insertPanelButton.Text = "Insert Wall Panel";
            insertPanelButton.CommandParameter = "INSERTWALLPANEL ";
            insertPanelButton.Size = RibbonItemSize.Large;
            //insertPanelButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            insertPanelButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            insertPanelButton.ShowText = true;
            insertPanelButton.ShowImage = true;
            insertPanelButton.CommandHandler = new AdskCommandHandler();

            var insertPanelImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\InsertWP.jpg");
            if (File.Exists(insertPanelImagePath))
            {
                Bitmap insertPanelBitmap = new Bitmap(insertPanelImagePath);
                insertPanelButton.Image = getBitmap(insertPanelBitmap, 16, 16);
                insertPanelButton.LargeImage = getBitmap(insertPanelBitmap, 32, 32);
            }
            else
            {
                string msg = "Insert Wall Panel " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            ////Create Insert Deck Panel button
            //Autodesk.Windows.RibbonButton insertDeckPanelButton = new RibbonButton();
            insertDeckPanelButton.ToolTip = "Insert Deck Panel";
            insertDeckPanelButton.Text = "Insert Deck Panel";
            insertDeckPanelButton.CommandParameter = "INSERTDECKPANEL ";
            insertDeckPanelButton.Size = RibbonItemSize.Large;
            //insertDeckPanelButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            insertDeckPanelButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            insertDeckPanelButton.ShowText = true;
            insertDeckPanelButton.ShowImage = true;
            insertDeckPanelButton.CommandHandler = new AdskCommandHandler();

            var insertDeckPanelImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\InsertDP.jpg");
            if (File.Exists(insertDeckPanelImagePath))
            {
                Bitmap insertDeckPanelBitmap = new Bitmap(insertDeckPanelImagePath);
                insertDeckPanelButton.Image = getBitmap(insertDeckPanelBitmap, 16, 16);
                insertDeckPanelButton.LargeImage = getBitmap(insertDeckPanelBitmap, 32, 32);
            }
            else
            {
                string msg = "Insert Deck Panel " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            ////Create Elevation Plan button
            ////Autodesk.Windows.RibbonButton createShellPlanButton = new RibbonButton();      
            createElevationPlanButton.ToolTip = "Create Elevation Plan";
            createElevationPlanButton.Text = "Create EP";
            createElevationPlanButton.CommandParameter = "CREATEELEVATIONPLAN ";
            createElevationPlanButton.Size = RibbonItemSize.Large;
            //createElevationPlanButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            createElevationPlanButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            createElevationPlanButton.ShowText = true;
            createElevationPlanButton.ShowImage = true;
            createElevationPlanButton.CommandHandler = new AdskCommandHandler();

            var createElevationPlanImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\CreateElevationPlan.jpg");
            if (File.Exists(createElevationPlanImagePath))
            {
                Bitmap createElevationPlanBitmap = new Bitmap(createElevationPlanImagePath);
                createElevationPlanButton.Image = getBitmap(createElevationPlanBitmap, 16, 16);
                createElevationPlanButton.LargeImage = getBitmap(createElevationPlanBitmap, 32, 32);
            }
            else
            {
                string msg = "Create Elevation Plan " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            ////Create Hole centerline button
            //Autodesk.Windows.RibbonButton rebuildPanelButton = new RibbonButton();
            createHolecenterlineButton.ToolTip = "Hole Centerlines";
            createHolecenterlineButton.Text = "Hole Centerlines";
            createHolecenterlineButton.CommandParameter = "AEE_HoleCenterlines ";
            createHolecenterlineButton.Size = RibbonItemSize.Large;
            createHolecenterlineButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            createHolecenterlineButton.ShowText = true;
            createHolecenterlineButton.ShowImage = true;
            createHolecenterlineButton.IsEnabled = false;
            createHolecenterlineButton.CommandHandler = new AdskCommandHandler();

            var HolecenterlineButtonImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\HoleCenterlines.jpg");
            if (File.Exists(HolecenterlineButtonImagePath))
            {
                Bitmap HolecenterlineBitmap = new Bitmap(HolecenterlineButtonImagePath);
                createHolecenterlineButton.Image = getBitmap(HolecenterlineBitmap, 16, 16);
                createHolecenterlineButton.LargeImage = getBitmap(HolecenterlineBitmap, 32, 32);
            }
            else
            {
                string msg = "Hole Centerlines " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            ////Create Rebuild Panel button
            //Autodesk.Windows.RibbonButton rebuildPanelButton = new RibbonButton();
            rebuildPanelButton.ToolTip = "Rebuild panels and corners";
            rebuildPanelButton.Text = "Rebuild";
            rebuildPanelButton.CommandParameter = "AEE_Rebuild ";
            rebuildPanelButton.Size = RibbonItemSize.Large;
            //rebuildPanelButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            rebuildPanelButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            rebuildPanelButton.ShowText = true;
            rebuildPanelButton.ShowImage = true;
            rebuildPanelButton.CommandHandler = new AdskCommandHandler();

            var rebuildPanelImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\ReBuild.jpg");
            if (File.Exists(rebuildPanelImagePath))
            {
                Bitmap rebuildPanelBitmap = new Bitmap(rebuildPanelImagePath);
                rebuildPanelButton.Image = getBitmap(rebuildPanelBitmap, 16, 16);
                rebuildPanelButton.LargeImage = getBitmap(rebuildPanelBitmap, 32, 32);
            }
            else
            {
                string msg = "Rebuild " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            //Added on 10/06/2023 by SDM
            ////Update Wall button
            //Autodesk.Windows.RibbonButton updateWallButton = new RibbonButton();
            updateWallButton.ToolTip = "Update wall";
            updateWallButton.Text = "Update";
            updateWallButton.CommandParameter = "AEE_Update ";
            updateWallButton.Size = RibbonItemSize.Large;
            updateWallButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            updateWallButton.ShowText = true;
            updateWallButton.ShowImage = true;
            updateWallButton.CommandHandler = new AdskCommandHandler();

            var updateWallImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\Update.jpg");
            if (File.Exists(updateWallImagePath))
            {
                Bitmap updateWallBitmap = new Bitmap(updateWallImagePath);
                updateWallButton.Image = getBitmap(updateWallBitmap, 16, 16);
                updateWallButton.LargeImage = getBitmap(updateWallBitmap, 32, 32);
            }
            else
            {
                string msg = "Update " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            ////Create Create BOM button
            //Autodesk.Windows.RibbonButton createBOMButton = new RibbonButton();
            createBOMButton.ToolTip = "Create Bill Of Material";
            createBOMButton.Text = "Create BOM";
            createBOMButton.CommandParameter = "CREATEBOM ";
            createBOMButton.Size = RibbonItemSize.Large;
            //createBOMButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            createBOMButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            createBOMButton.ShowText = true;
            createBOMButton.ShowImage = true;
            createBOMButton.CommandHandler = new AdskCommandHandler();


            var createBOMImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\CreateBOM.jpg");
            if (File.Exists(createBOMImagePath))
            {
                Bitmap createBOMBitmap = new Bitmap(createBOMImagePath);
                createBOMButton.Image = getBitmap(createBOMBitmap, 16, 16);
                createBOMButton.LargeImage = getBitmap(createBOMBitmap, 32, 32);
            }
            else
            {
                string msg = "Create BOM " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            //////Read Text button
            ////Autodesk.Windows.RibbonButton ReadDataButton = new RibbonButton();
            //readText.ToolTip = "Read Text";
            //readText.Text = "Read";
            //readText.CommandParameter = "READDATA ";
            //readText.Size = RibbonItemSize.Large;
            ////clearDataButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            //readText.Orientation = System.Windows.Controls.Orientation.Vertical;
            //readText.ShowText = true;
            //readText.ShowImage = true;
            //readText.CommandHandler = new AdskCommandHandler();

            //var readSettingsImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\ReadSettings.jpg");
            //if (File.Exists(readSettingsImagePath))
            //{
            //    Bitmap readSettingsBitmap = new Bitmap(readSettingsImagePath);
            //    readText.Image = getBitmap(readSettingsBitmap, 16, 16);
            //    readText.LargeImage = getBitmap(readSettingsBitmap, 32, 32);
            //}
            //else
            //{
            //    string msg = "Read Settings " + errorMsg;
            //    MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}

            ////Clear Data button
            Autodesk.Windows.RibbonButton clearDataButton = new RibbonButton();
            clearDataButton.ToolTip = "Clear Data";
            clearDataButton.Text = "Clear Data";
            clearDataButton.CommandParameter = "CLEARDATA ";
            clearDataButton.Size = RibbonItemSize.Large;
            //clearDataButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
            clearDataButton.Orientation = System.Windows.Controls.Orientation.Vertical;
            clearDataButton.ShowText = true;
            clearDataButton.ShowImage = true;
            clearDataButton.CommandHandler = new AdskCommandHandler();

            var clearDataImagePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), @"AEE_Images\PanelLayout\ClearData.jpg");
            if (File.Exists(clearDataImagePath))
            {
                Bitmap clearDataBitmap = new Bitmap(clearDataImagePath);
                clearDataButton.Image = getBitmap(clearDataBitmap, 16, 16);
                clearDataButton.LargeImage = getBitmap(clearDataBitmap, 32, 32);
            }
            else
            {
                string msg = "Clear Data " + errorMsg;
                MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            panlLaytTabRbnPnlSrce.Items.Add(panelLayoutOptionButton);
            panlLaytTabRbnPnlSrce.Items.Add(readText);
            panlLaytTabRbnPnlSrce.Items.Add(checkShellPlanButton);
            panlLaytTabRbnPnlSrce.Items.Add(createShellPlanButton);
            panlLaytTabRbnPnlSrce.Items.Add(insertCornerButton);
            panlLaytTabRbnPnlSrce.Items.Add(insertPanelButton);
            panlLaytTabRbnPnlSrce.Items.Add(insertDeckPanelButton);
            panlLaytTabRbnPnlSrce.Items.Add(createElevationPlanButton);
            panlLaytTabRbnPnlSrce.Items.Add(createHolecenterlineButton);
            panlLaytTabRbnPnlSrce.Items.Add(rebuildPanelButton);
            panlLaytTabRbnPnlSrce.Items.Add(updateWallButton); //Added on 10/06/2023 by SDM           
            panlLaytTabRbnPnlSrce.Items.Add(createBOMButton);
            panlLaytTabRbnPnlSrce.Items.Add(clearDataButton);
        }
    }

    public class AdskCommandHandler : System.Windows.Input.ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            // call only one button, because if all button are called in this method, UI is opening in multiple of creating button.
            // All button are working if call one button.
            RibbonButton checkShellPlanButton = parameter as RibbonButton;
            if (checkShellPlanButton != null)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)checkShellPlanButton.CommandParameter, true, false, true);
            }
        }
    }


}
