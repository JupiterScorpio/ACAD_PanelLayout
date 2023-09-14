﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Text.RegularExpressions;
using PanelLayout_App.CivilModel;
using PanelLayout_App.Licensing;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using TextBox = System.Windows.Forms.TextBox;
using GroupBox = System.Windows.Forms.GroupBox;
using System.Reflection;
using Autodesk.AutoCAD.EditorInput;

namespace PanelLayout_App
{
    public partial class PanelLayout_UI : UserControl
    {
        public static Dictionary<string, (string, double, double, double, double, double, double, double, double, double)> VariablesData = new Dictionary<string, (string, double, double, double, double, double, double, double, double, double)>();//Added made on 30/04/2023 by SDM
        public static double slabThickness = 0;
        public static double floorHeight = 0;

        public static double FFL = 0;
        public static double SL = 0;
        public static double RC = 0;
        public static double kickerHt = 0;
        public static double bottomSplitHeight = 0;//PBAC
        //public static double extrusionLength = 2700;//TSK
        // public static double extrusionLength = 2650;
        public static double defaultFloorHeight = 3150;
        public static double minHeightOfPanel = 300;
        public static double maxHeightOfPanel = 3000;// bottomSplitHeight + 300;//3000;
        public static double maxHeightOfWindowAndDoor = 1200;
        public static double maxValueToConvertHorzToVert = 750;

        public static double SL_MaxLength = 0;
        public static double maxHeightOfCorner = maxHeightOfPanel;


        public static double f_dist = 200;
        public static double sec_dist = 300;

        public static double ver_max_pitch = 100;
        public static double ver_min_pitch = 50;

        public static double ext_wall_hgt = 0;
        public static double int_wall_hgt = 0;

        public static double maxWallThickness = 0;
        public static double standardPanelWidth = 0;
        public static double minWidthOfPanel = 75;
        public static double maxWidthOfPanel = 600;
        public static double minSJWidthOfPanel = 300;
        public static double maxSJWidthOfPanel = 1200;
        public static double standardPanelHeight = 0;
        public static double standardPanelThickness = 0;

       

        public static double PanelRailThickness = 6;

        public static bool flagOfHorzPanel_ForDoorWindowBeam = false;
        public static bool flagOfPanelWithRC = false;
        public static bool flagOfPanelLayoutOption = false;
        public static bool flagOfExtendPanelAtExternalCornersOption = false; //Added by SDM 2022-08-06

        public static string wallPanelName = "";
        public static string wallTopPanelName = "";
        public static string windowPanelName = "";
        public static string windowTopPanelName = "";
        public static string doorTopPanelName = "";
        public static string beamPanelName = "";
        public static double beamSideMaxPanelLength = 0; //Beam Side Panel Max Length whn its Horizontal.Added on 13/07/2023 by PRT
        public static string beamSidePanelName = "";
        public static string deckPanelName = "";
        public static string kickerWallName = "Kicker";
        public static string slabJointWallName = "";
        public static string deckPanel_MSP1_Name = "MSP1";
        public static string deckPanel_ESP1_Name = "ESP1";
        public static string deckPanel_SF_Name = "SF";
        public static string doorWindowBottomPanelNameStandard = "BB";
        public static string doorWindowBottomPanelName = "BBX";
        public static string windowThickBottomWallPanelText = "WBX";
        public static string beamBottomPropPanelName = "BHX";
        public static string beamBottomPanelName = "BBX1";
        bool isnewdoc = false;

        PanelLayout_UI_Helper panelLayout_UI_Helper = new PanelLayout_UI_Helper();
        Window_UI_Helper window_UI_Helper = new Window_UI_Helper();
        Door_UI_Helper door_UI_Helper = new Door_UI_Helper();
        Beam_UI_Helper beam_UI_Helper = new Beam_UI_Helper();
        InternalSunkanSlab_UI_Helper internalSunkanSlab_UI_Helper = new InternalSunkanSlab_UI_Helper();
        ExternalSunkanSlab_UI_Helper externalSunkanSlab_UI_Helper = new ExternalSunkanSlab_UI_Helper();
        InternalWallSlab_UI_Helper internalWallSlab_UI_Helper = new InternalWallSlab_UI_Helper();

        Parapet_UI_Helper parapet_UI_Helper = new Parapet_UI_Helper();
        StairCase_UI_Helper stairCase_UI_Helper = new StairCase_UI_Helper();
        Lift_UI_Helper lift_UI_Helper = new Lift_UI_Helper();

        string emptyText = "Feel in value.";
        Color backColor = Color.WhiteSmoke;
        Color inActiveColor = Color.LightGray;

        string textFileName = "PanelLayoutInput";
        string textFileExtension = ".txt";
        private static string textOldFilePath = "";
        private string txtUIControlOldValue = "";  //Modified by RTJ on May 15, 2021


        public PanelLayout_UI()
        {
            InitializeComponent();
        }


        private void PaneLayout_UI_Load(object sender, EventArgs e)
        {
            if (MacAddressUtility.IsLicenseCheckNeeded())
            {
                settingButton.Visible = true;
            }
            else
            {
                settingButton.Visible = true;
            }
            LoadSettings();
        }

        public void LoadSettings()
        {
            //Changes made on 10/06/2023 by SDM
            isnewdoc = false;
            var appPath = LicenseUtilities.getAppPath();

            textOldFilePath = appPath + textFileName + textFileExtension;
            setUIValueFromTextFile();
            DefaultVariables();

            if (flagOfPanelLayoutOption == false)
            {
                callMethodOfDefaultValue();
            }
            else
            {
                window_UI_Helper.previousDataInWindowGridView(windowDataGridView);
                window_UI_Helper.previousDataInChajjaGridView(chajjaDataGridView);
                door_UI_Helper.previousDataInDoorGridView(doorDataGridView);
                beam_UI_Helper.previousDataInBeamGridView(beamDataGridView);//Added on 08/03/2023 by SDM
                beam_UI_Helper.previousDataInOffsetBeamGridView(offsetBeamDataGridView);
                internalSunkanSlab_UI_Helper.previousDataInSunkanSlabGridView(internalSunkanSlabDataGridView);
                externalSunkanSlab_UI_Helper.previousDataInExternalSunkanSlabGridView(externalSunkanSlabDataGridView);
                internalWallSlab_UI_Helper.previousDataInInternalWallSlabGridView(internalWallSlabDataGridView);
                parapet_UI_Helper.previousDataInParapetGridView(parapetDataGridView);
                stairCase_UI_Helper.previousDataInStairCaseGridView(stairCaseDataGridView);
                lift_UI_Helper.previousDataInLiftGridView(liftDataGridView);
            }
            if (isnewdoc)
            {
                setDefaultValue();
            }
            setVariablesInUI();
            InternalWallSlab_UI_Helper.updateSlabThickness(internalWallSlabDataGridView, floorHeightTextBox);
            //Modified by RTJ on May 15, 2021 for making Apply button disabled
            txtUIControlOldValue = floorHeightTextBox.Text;
            floorHeightTextBox.Focus();
            applyButton.Enabled = false;
            //Modification ends
        }


        public static bool ReadUIValuesFromExcel() //Added made on 30/04/2023 by SDM
        {
            //Reade values----------------------------
            var strError = string.Empty;
            string strMasterBomError = string.Empty;
            try
            {

                string tempPath = System.IO.Path.GetTempFileName();

                var stream = Utils.GetResourceFileStream("PanelLayoutCodes.xlsx");
                byte[] b;

                using (BinaryReader br = new BinaryReader(stream))
                {
                    b = br.ReadBytes((int)stream.Length);
                }
                System.IO.File.WriteAllBytes(tempPath, b);
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();

                Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Open(tempPath, ReadOnly: true);

                var lst = new Dictionary<string, (string, double, double, double, double, double, double, double, double, double)>();
                Microsoft.Office.Interop.Excel._Worksheet masterBomWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets["Sheet1"];
                Microsoft.Office.Interop.Excel.Range xlRange = masterBomWorksheet.UsedRange;


                try
                {

                    int t = 0;
#if WNPANEL
                    t = 10;
#endif
                    var maxEmpty = 0;
                    for (int iRow = 3; iRow < 25000; ++iRow)
                    {
                        if (maxEmpty > 3)
                            break;

                        var name = ((object)xlRange[iRow, 2].Value2)?.ToString();
                        if (name.IsEmpty())
                        {
                            maxEmpty++;
                            continue;
                        }
                        maxEmpty = 0;

                        var _name = ((object)xlRange[iRow, t + 2].Value2)?.ToString();
                        double Xdef = ((object)xlRange[iRow, t + 3].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Xmin = ((object)xlRange[iRow, t + 4].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Xmax = ((object)xlRange[iRow, t + 5].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Ydef = ((object)xlRange[iRow, t + 6].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Ymin = ((object)xlRange[iRow, t + 7].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Ymax = ((object)xlRange[iRow, t + 8].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Zdef = ((object)xlRange[iRow, t + 9].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Zmin = ((object)xlRange[iRow, t + 10].Value2)?.ToString().ToDoubleOrZero() ?? 0;
                        double Zmax = ((object)xlRange[iRow, t + 11].Value2)?.ToString().ToDoubleOrZero() ?? 0;

                        if (lst.ContainsKey(name))
                            continue;
                        lst.Add(name, (_name, Xdef, Xmin, Xmax, Ydef, Ymin, Ymax, Zdef, Zmin, Zmax));

                    }
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                }
                workbook.Close(true);
                excel.Quit();
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);
                VariablesData = lst;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            if (string.IsNullOrEmpty(strError))
                return true;

            return false;
        }
        public static void SetUIValuesFromExcel() //Added made on 30/04/2023 by SDM
        {
            if (VariablesData.Count == 0)
                ReadUIValuesFromExcel();

            //Set values ----------------------------------------------------------
            if (VariablesData.ContainsKey("WP"))
            {
                var data = VariablesData["WP"];
                wallPanelName = data.Item1;
                //width X
                standardPanelWidth = data.Item2;// 450;
                minWidthOfPanel = data.Item3;
                maxWidthOfPanel = data.Item4;
                //height Y
                standardPanelHeight = data.Item8;// 3000;//Added on 10/03/2023 by SDM
                minHeightOfPanel = data.Item9;
                maxHeightOfPanel = data.Item10;
                bottomSplitHeight = maxHeightOfPanel;
                maxHeightOfCorner = maxHeightOfPanel;

                //thickness Z
                standardPanelThickness = data.Item5;// 50;//Added on 10/03/2023 by SDM

            }
            if (VariablesData.ContainsKey("TP"))
            {
                var data = VariablesData["TP"];
                wallTopPanelName = data.Item1;
            }
            if (VariablesData.ContainsKey("IAX"))
            {
                var data = VariablesData["IAX"];
                CommonModule.internalCornerText = data.Item1;
                CommonModule.internalCorner = data.Item2;
                CommonModule.internalCorner_MinLngth = data.Item3;
                CommonModule.internalCorner_MaxLngth = data.Item4;
            }
            if (VariablesData.ContainsKey("IA"))
            {
                var data = VariablesData["IA"];
                CommonModule.internalCornerText2 = data.Item1;
            }
            if (VariablesData.ContainsKey("BIAX"))
            {
                var data = VariablesData["BIAX"];
                CommonModule.beamInternalCornerText = data.Item1;
            }
            if (VariablesData.ContainsKey("EA"))
            {
                var data = VariablesData["EA"];
                CommonModule.externalCornerText = data.Item1;
                CommonModule.externalCorner = data.Item2;
            }
            if (VariablesData.ContainsKey("BS"))
            {
                var data = VariablesData["BS"];
                beamPanelName = data.Item1;
                beamSideMaxPanelLength = data.Item4;//Added on 13/07/2023 by PRT
            }
            if (VariablesData.ContainsKey("BA"))
            {
                var data = VariablesData["BA"];
                beamSidePanelName = data.Item1;
            }
            if (VariablesData.ContainsKey("SJ"))
            {
                var data = VariablesData["SJ"];
                slabJointWallName = data.Item1;
                minSJWidthOfPanel = data.Item3;
                maxSJWidthOfPanel = data.Item4;
                CommonModule.slabJointPanelDepth = data.Item5;
            }
            if (VariablesData.ContainsKey("SP"))
            {
                var data = VariablesData["SP"];
                deckPanelName = data.Item1;
                CommonModule.deckPanel_Span_StndrdLngth = data.Item2;
                CommonModule.deckPanel_Span_MinLngth = data.Item3;//600
                CommonModule.deckPanel_Span_MaxLngth = data.Item4;

                //height Y
                CommonModule.deckPanelMinWidth = data.Item8;//Added on 10/07/2023 by PRT
            }
            if (VariablesData.ContainsKey("MSP1"))
            {
                var data = VariablesData["MSP1"];
                deckPanel_MSP1_Name = data.Item1;
                CommonModule.deckPanel_MSP1_StndrdLngth = data.Item2;
                CommonModule.deckPanel_MSP1_MinLngth = data.Item3;
                CommonModule.deckPanel_MSP1_MaxLngth = data.Item4;
            }
            if (VariablesData.ContainsKey("ESP1"))
            {
                var data = VariablesData["ESP1"];
                deckPanel_ESP1_Name = data.Item1;
                CommonModule.deckPanel_ESP1_StndrdLngth = data.Item2;
                CommonModule.deckPanel_ESP1_MinLngth = data.Item3;
                CommonModule.deckPanel_ESP1_MaxLngth = data.Item4;
            }
            if (VariablesData.ContainsKey("CCX"))
            {
                var data = VariablesData["CCX"];
                CommonModule.doorWindowThickTopCrnrText = data.Item1;
                CommonModule.doorWindowTopCornerHt = data.Item5;
            }
            if (VariablesData.ContainsKey("BCCX"))
            {
                var data = VariablesData["BCCX"];
                CommonModule.beamThickTopCrnrText = data.Item1;
            }
            if (VariablesData.ContainsKey("LCX"))
            {
                var data = VariablesData["LCX"];
                CommonModule.windowThickBottomCrnrText = data.Item1;
            }
            if (VariablesData.ContainsKey("BBX"))
            {
                var data = VariablesData["BBX"];
                CommonModule.doorWindowBottomPanelName = data.Item1;
                doorWindowBottomPanelName = data.Item1;
                beamBottomPanelName = data.Item1;
            }

            if (VariablesData.ContainsKey("BHX"))
            {
                var data = VariablesData["BHX"];
                CommonModule.doorWindowThickTopPropText = data.Item1;
                beamBottomPropPanelName = data.Item1;
            }
            if (VariablesData.ContainsKey("WBH"))
            {
                var data = VariablesData["WBH"];
                CommonModule.windowThickBottomPropText = data.Item1;
            }
            if (VariablesData.ContainsKey("WBX"))
            {
                var data = VariablesData["WBX"];
                windowThickBottomWallPanelText = data.Item1;
            }
            if (VariablesData.ContainsKey("ECSJ"))
            {
                var data = VariablesData["ECSJ"];
                CommonModule.slabJointCornerConvexText = data.Item1;
            }
            if (VariablesData.ContainsKey("CSJ"))
            {
                var data = VariablesData["CSJ"];
                CommonModule.slabJointCornerConcaveText = data.Item1;
            }
            if (VariablesData.ContainsKey("SF"))
            {
                var data = VariablesData["SF"];
                deckPanel_SF_Name = data.Item1;
                CommonModule.deckPanel_SF_Lngth = data.Item2;
            }
            if (VariablesData.ContainsKey("K"))
            {
                var data = VariablesData["K"];
                kickerWallName = data.Item1;
                CommonModule.minLngthOfKickerPanel = data.Item3;
                CommonModule.maxLngthOfKickerPanel = data.Item4;
                kickerHt = data.Item8;
            }
            if (VariablesData.ContainsKey("KC"))
            {
                var data = VariablesData["KC"];
                CommonModule.kickerCornerText = data.Item1;
                CommonModule.kickerCornr_Flange1 = data.Item2;
                CommonModule.kickerCornr_Flange2 = data.Item2;
                CommonModule.minKickerCornrFlange = data.Item3;
            }
            if (VariablesData.ContainsKey("EKC"))
            {
                var data = VariablesData["EKC"];
                CommonModule.externalKickerCornerText = data.Item1;
                CommonModule.extendKickerCornrFlangeUpto = data.Item4;
            }
            if (VariablesData.ContainsKey("DSX"))
            {
                var data = VariablesData["DSX"];
                CommonModule.doorThickWallPanelText = data.Item1;
                CommonModule.minPanelThick = data.Item3;
                CommonModule.maxPanelThick = data.Item4;
            }
            if (VariablesData.ContainsKey("WSX"))
            {
                var data = VariablesData["WSX"];
                CommonModule.windowThickWallPanelText = data.Item1;
                CommonModule.minPanelThick = data.Item3;
                CommonModule.maxPanelThick = data.Item4;
            }
            if (VariablesData.ContainsKey("WWP"))
            {
                var data = VariablesData["WWP"];
                windowPanelName = data.Item1;
            }
            if (VariablesData.ContainsKey("WTP"))
            {
                var data = VariablesData["WTP"];
                windowTopPanelName = data.Item1;
            }
            if (VariablesData.ContainsKey("DTP"))
            {
                var data = VariablesData["DTP"];
                doorTopPanelName = data.Item1;
            }
            SL_MaxLength = 2000;
            f_dist = 200;
            sec_dist = 300;
            ver_max_pitch = 100;
            ver_min_pitch = 50;


        }


        private void doorDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[Door_UI_Helper.doorLayerColumnIndex].Value = Door_UI_Helper.doorStrtText + (Convert.ToString(Door_UI_Helper.doorLayerCount));
            Door_UI_Helper.doorLayerCount++;
            applyButton.Enabled = true; //Modified by RTJ on May 15, 2021
        }

        private void doorDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (doorDataGridView.CurrentCell.ColumnIndex == Door_UI_Helper.doorLintelLevelColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }

        private void doorDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //string value = Convert.ToString(e.FormattedValue);
            //if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            //{
            //    MessageBox.Show(emptyText, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    e.Cancel = true;
            //}
        }
        private void beamDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)//Added on 08/03/2023 by SDM
        {
        }
        private void beamDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)//Added on 08/03/2023 by SDM
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (beamDataGridView.CurrentCell.ColumnIndex == Beam_UI_Helper.beamHeightColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }
        private void beamDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)//Added on 08/03/2023 by SDM
        {
            e.Row.Cells[Beam_UI_Helper.beamLayerColumnIndex].Value = Beam_UI_Helper.beamStrtText + (Convert.ToString(Beam_UI_Helper.beamLayerCount));
            Beam_UI_Helper.beamLayerCount++;
            applyButton.Enabled = true;
        }
        private void offsetBeamDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[Beam_UI_Helper.offsetBeamLayerColumnIndex].Value = Beam_UI_Helper.offsetBeamStrtText + (Convert.ToString(Beam_UI_Helper.offsetBeamLayerCount));
            Beam_UI_Helper.offsetBeamLayerCount++;
            applyButton.Enabled = true;//Modified by RTJ on May 15, 2021
        }

        private void offsetBeamDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.TextChanged += new System.EventHandler(this.offsetBeamDataGridView_TextChanged);

            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (offsetBeamDataGridView.CurrentCell.ColumnIndex == Beam_UI_Helper.offsetBeamBottomColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }

        private void offsetBeamDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //string value = Convert.ToString(e.FormattedValue);
            //if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            //{
            //    MessageBox.Show(emptyText, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    e.Cancel = true;
            //}
        }

        private void parapetDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {

        }

        public void Column_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void slabBottomTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            panelLayout_UI_Helper.keyPressValidate(sender, e, floorHeightTextBox);
        }


        private void kickerTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            panelLayout_UI_Helper.keyPressValidate(sender, e, kickerTextBox);
        }

        private void SLTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            panelLayout_UI_Helper.keyPressValidate(sender, e, SLTextBox);
        }

        private void RCTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            panelLayout_UI_Helper.keyPressValidate(sender, e, RCTextBox);
        }

        private void standardPanelHghtTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void slabBottomTextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, floorHeightTextBox, emptyText);
            InternalWallSlab_UI_Helper.updateSlabThickness(internalWallSlabDataGridView, floorHeightTextBox);
        }

        private void kickerTextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, kickerTextBox, emptyText);
            //if (flag == true)
            //{
            //    kicker = Convert.ToDouble(kickerTextBox.Text);
            //}
        }

        private void SLTextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, SLTextBox, emptyText);
            //if (flag == true)
            //{
            //    SL = Convert.ToDouble(SLTextBox.Text);
            //}
        }

        private void RCTextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, RCTextBox, emptyText);
            //if (flag == true)
            //{
            //    RC = Convert.ToDouble(RCTextBox.Text);
            //}
        }

        private void standardPanelHghtTextbox_Validating(object sender, CancelEventArgs e)
        {
            string message = "panel heigth";
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, standardPanelHghtTextbox, minHeightOfPanel, maxHeightOfPanel, message);
        }
        private void slabBottomTextBox_Click(object sender, EventArgs e)
        {
            floorHeightTextBox.BackColor = backColor;
        }

        private void kickerTextBox_Click(object sender, EventArgs e)
        {
            kickerTextBox.BackColor = backColor;
        }

        private void SLTextBox_Click(object sender, EventArgs e)
        {
            SLTextBox.BackColor = backColor;
        }

        private void RCTextBox_Click(object sender, EventArgs e)
        {
            RCTextBox.BackColor = backColor;
        }
        private void standardPanelHghtTextbox_Click(object sender, EventArgs e)
        {
            standardPanelHghtTextbox.BackColor = backColor;
        }

        private void RC_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //flagOfPanelWithRC = PanelRC_CheckBox.Checked;
        }
        //Added by SDM 2022-08-06
        private void Extend_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            flagOfExtendPanelAtExternalCornersOption = ExtendPanelAtExternalCorners_CheckBox.Checked;
        }

        private void doorDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            //door_UI_Helper.getDataFromGridView(doorDataGridView);
        }
        private void beamDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            //beam_UI_Helper.getDataFromBeamGridView(beamDataGridView);
        }

        private void offsetBeamDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            //beam_UI_Helper.getDataFromGridView(offsetBeamDataGridView);
        }

        private void offsetBeamDataGridView_TextChanged(object sender, EventArgs e)
        {
            if (offsetBeamDataGridView.CurrentCell.ColumnIndex == Beam_UI_Helper.offsetBeamTopColumnIndex)
            {
                var text = offsetBeamDataGridView.EditingControl.Text;
                if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                {

                }
                else
                {
                    int output = 0;
                    var flagOfInt = int.TryParse(text, out output);
                    if (flagOfInt == false)
                    {
                        offsetBeamDataGridView.EditingControl.Text = Beam_UI_Helper.defaultOffsetBeamTopValue;
                    }
                }
            }
        }

        private void parapetDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            List<GroupBox> listOfGroupBox = new List<GroupBox>();
            listOfGroupBox.Add(sizeGroupBox);
            listOfGroupBox.Add(annotationGroupBox);
            listOfGroupBox.Add(civilParametersGroupBox);

            bool flagOfUI = panelLayout_UI_Helper.checkPanelLayout_UI(listOfGroupBox);
            if (flagOfUI == true)
            {
                double fh = Convert.ToDouble(floorHeightTextBox.Text);
                var strErrMsg = InternalWallSlab_UI_Helper.ValidDateSlabBottom(internalWallSlabDataGridView, fh);
                if (!String.IsNullOrEmpty(strErrMsg))
                {
                    MessageBox.Show(strErrMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                getVariablesFromUI();

                createLayerOfCivilModel();

                flagOfPanelLayoutOption = true;

                PanelLayoutInputHelper panelLayoutInputHlp = new PanelLayoutInputHelper();
                panelLayoutInputHlp.setDataFromUI_ForTextFile();
                //panelLayoutInputHlp.write_UI_ValueInTextFile(textFilePath);
                panelLayoutInputHlp.storeinlutactivedoc();

            }
            //Modified by RTJ on May 15, 2021 
            txtUIControlOldValue = "";
            applyButton.Enabled = false;
            //Ends modifiation by RTJ
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
        }

        private void PanelRC_Label_Click(object sender, EventArgs e)
        {
            if (PanelRC_CheckBox.Checked == true)
            {
                PanelRC_CheckBox.Checked = false;
            }
            else
            {
                PanelRC_CheckBox.Checked = true;
            }
        }

        private void parapetDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[Parapet_UI_Helper.parapetLayerColumnIndex].Value = Parapet_UI_Helper.parapetStrtText + (Convert.ToString(Parapet_UI_Helper.parapetLayerCount));
            Parapet_UI_Helper.parapetLayerCount++;
            applyButton.Enabled = true; // Modified by RTJ on May 15, 2021
        }

        private void parapetDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.TextChanged += new System.EventHandler(this.parapetDataGridView_TextChanged);

            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (parapetDataGridView.CurrentCell.ColumnIndex == Parapet_UI_Helper.parapetTopColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }

        private void parapetDataGridView_TextChanged(object sender, EventArgs e)
        {
            if (parapetDataGridView.CurrentCell.ColumnIndex == Parapet_UI_Helper.parapetBottomColumnIndex)
            {
                var text = parapetDataGridView.EditingControl.Text;
                if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                {

                }
                else
                {
                    int output = 0;
                    var flagOfInt = int.TryParse(text, out output);
                    if (flagOfInt == false)
                    {
                        parapetDataGridView.EditingControl.Text = Parapet_UI_Helper.defaultParapetBottomValue;
                    }
                }
            }
        }

        private void stairCaseDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[StairCase_UI_Helper.stairCaseLayerColumnIndex].Value = StairCase_UI_Helper.stairCaseStrtText + (Convert.ToString(StairCase_UI_Helper.stairCaseLayerCount));
            StairCase_UI_Helper.stairCaseLayerCount++;
            applyButton.Enabled = true; //Modified by RTJ on May 15, 2021
        }

        private void windowDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            //window_UI_Helper.getDataFromGridView(windowDataGridView);
        }

        private void windowDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //string value = Convert.ToString(e.FormattedValue);
            //if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            //{
            //    MessageBox.Show(emptyText, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    e.Cancel = true;           
            //} 
        }

        private void windowDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[Window_UI_Helper.windowLayerColumnIndex].Value = Window_UI_Helper.windowStrtText + (Convert.ToString(Window_UI_Helper.windowLayerCount));
            Window_UI_Helper.windowLayerCount++;
            applyButton.Enabled = true;//Modified by RTJ on May 15, 2021
        }

        private void windowDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (windowDataGridView.CurrentCell.ColumnIndex == Window_UI_Helper.sillLevelColumnIndex || windowDataGridView.CurrentCell.ColumnIndex == Window_UI_Helper.lintelLevelColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }

        private void PanelLayout_UI_SizeChanged(object sender, EventArgs e)
        {
            //////var width = this.Size.Width;
            //////var height = this.Size.Height;
            ////if (this.Size.Width < 500)
            ////{
            ////    this.Size = new Size(500, this.Size.Height);              
            ////}
        }

        private void liftDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[Lift_UI_Helper.liftLayerColumnIndex].Value = Lift_UI_Helper.liftStrtText + (Convert.ToString(Lift_UI_Helper.liftLayerCount));
            Lift_UI_Helper.liftLayerCount++;
            applyButton.Enabled = true; // Modified by RTJ on May 15, 2021
        }

        private void SL_MaxLengthTextBox_Click(object sender, EventArgs e)
        {
            SL_MaxLengthTextBox.BackColor = backColor;
        }

        private void SL_MaxLengthTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            panelLayout_UI_Helper.keyPressValidate(sender, e, SL_MaxLengthTextBox);

        }

        private void SL_MaxLengthTextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, SL_MaxLengthTextBox, emptyText);

        }

        private void wallPanelName_TextBox_Click(object sender, EventArgs e)
        {
            wallPanelName_TextBox.BackColor = backColor;
        }

        private void wallTopPanelName_TextBox_Click(object sender, EventArgs e)
        {
            wallTopPanelName_TextBox.BackColor = backColor;
        }

        private void windowPanelName_TextBox_Click(object sender, EventArgs e)
        {
            windowPanelName_TextBox.BackColor = backColor;
        }

        private void windowTopPanelName_TextBox_Click(object sender, EventArgs e)
        {
            windowTopPanelName_TextBox.BackColor = backColor;
        }

        private void doorTopPanelName_TextBox_Click(object sender, EventArgs e)
        {
            doorTopPanelName_TextBox.BackColor = backColor;
        }

        private void beamPanelName_TextBox_Click(object sender, EventArgs e)
        {
            beamPanelName_TextBox.BackColor = backColor;
        }

        private void deckPanelName_TextBox_Click(object sender, EventArgs e)
        {
            deckPanelName_TextBox.BackColor = backColor;
        }

        private void wallPanelName_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, wallPanelName_TextBox, emptyText);
        }

        private void wallTopPanelName_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, wallTopPanelName_TextBox, emptyText);
        }

        private void windowPanelName_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, windowPanelName_TextBox, emptyText);
        }

        private void windowTopPanelName_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, windowTopPanelName_TextBox, emptyText);
        }

        private void doorTopPanelName_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, doorTopPanelName_TextBox, emptyText);
        }

        private void beamPanelName_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, beamPanelName_TextBox, emptyText);
        }

        private void deckPanelName_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, deckPanelName_TextBox, emptyText);
        }

        private void bottomSplitHeight_TextBox_Click(object sender, EventArgs e)
        {
            bottomSplitHeight_TextBox.BackColor = backColor;
        }

        private void bottomSplitHeight_TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            panelLayout_UI_Helper.keyPressValidate(sender, e, bottomSplitHeight_TextBox);
        }

        private void bottomSplitHeight_TextBox_Validating(object sender, CancelEventArgs e)
        {
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, bottomSplitHeight_TextBox, emptyText);
        }

        private void internalSunkanSlabDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            //internalSunkanSlab_UI_Helper.getDataFromGridView(externalSunkanSlabDataGridView);
        }

        private void internalSunkanSlabDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //string value = Convert.ToString(e.FormattedValue);
            //if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            //{              
            //    MessageBox.Show(emptyText, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    e.Cancel = true;
            //}
        }

        private void internalSunkanSlabDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[InternalSunkanSlab_UI_Helper.internalSunkanSlabLayerColumnIndex].Value = InternalSunkanSlab_UI_Helper.internalSunkanSlabStrtText + (Convert.ToString(InternalSunkanSlab_UI_Helper.internalSunkanSlabNameCount));
            InternalSunkanSlab_UI_Helper.internalSunkanSlabNameCount++;
            applyButton.Enabled = true;//Modified by RTJ on May 15, 2021
        }

        private void internalSunkanSlabDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (internalSunkanSlabDataGridView.CurrentCell.ColumnIndex == InternalSunkanSlab_UI_Helper.internalSunkanSlabLevelDifferenceColumnIndex || internalSunkanSlabDataGridView.CurrentCell.ColumnIndex == InternalSunkanSlab_UI_Helper.internalSunkanSlabThickColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }

        private void settingButton_Click(object sender, EventArgs e)
        {
            SettingUI settingUI = new SettingUI();
            settingUI.ShowDialog();
        }

        private void standardPanelWidthTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void standardPanelWidthTextbox_Click(object sender, EventArgs e)
        {
            standardPanelWidthTextbox.BackColor = backColor;
        }

        private void standardPanelWidthTextbox_Validating(object sender, CancelEventArgs e)
        {
            string message = "panel width";
            var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, standardPanelWidthTextbox, minWidthOfPanel, maxWidthOfPanel, message);
        }
        private void tb_panel_height_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void tb_panel_height_Click(object sender, EventArgs e)
        {
            tb_panel_height.BackColor = backColor;
        }

        private void tb_panel_height_Validating(object sender, CancelEventArgs e)
        {
            string message = "panel height";
            //var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, tb_panel_height, minWidthOfPanel, maxWidthOfPanel, message);
        }
        private void tb_panel_thickness_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void tb_panel_thickness_Click(object sender, EventArgs e)
        {
            tb_panel_thickness.BackColor = backColor;
        }

        private void tb_panel_thickness_Validating(object sender, CancelEventArgs e)
        {
            string message = "panel thickness";
            // var flag = panelLayout_UI_Helper.textBox_Validating(sender, e, tb_panel_thickness, minWidthOfPanel, maxWidthOfPanel, message);
        }
        private void sizeGroupBox_Enter(object sender, EventArgs e)
        {

        }

        public void setDefaultValue()
        {
            DefaultVariables();
            createLayerOfCivilModel();
        }

        public void DefaultVariables()
        {
            window_UI_Helper.setDefaultValueInWindowGridView(windowDataGridView);
            window_UI_Helper.setDefaultValueInChajjaGridView(chajjaDataGridView);
            door_UI_Helper.setDefaultValueInDoorGridView(doorDataGridView);
            beam_UI_Helper.setDefaultValueInBeamGridView(beamDataGridView);
            beam_UI_Helper.setDefaultValueInOffsetBeamGridView(offsetBeamDataGridView);
            internalSunkanSlab_UI_Helper.setDefaultValueInSunkanSlabGridView(internalSunkanSlabDataGridView);
            InternalWallSlab_UI_Helper.setDefaultValueInInternalWallSlabGridView(internalWallSlabDataGridView);
            externalSunkanSlab_UI_Helper.setDefaultValueInExternalSunkanSlabGridView(externalSunkanSlabDataGridView);
            parapet_UI_Helper.setDefaultValueInParapetGridView(parapetDataGridView);
            stairCase_UI_Helper.setDefaultValueInStairCaseGridView(stairCaseDataGridView);
            lift_UI_Helper.setDefaultValueInLiftGridView(liftDataGridView);
#if WNPANEL //Added on 11/05/2023 by SDM
            maxWallThickness = 501;
#else
            maxWallThickness = 400;
#endif
            slabThickness = 150;
            floorHeight = 3150;
            SL = 125;
            RC = 50;
            kickerHt = 200;
            standardPanelWidth = 450;
            flagOfPanelWithRC = true;
#if !WNPANEL //Added on 26/04/2023 by SDM
            flagOfHorzPanel_ForDoorWindowBeam = true;
            flagOfExtendPanelAtExternalCornersOption = true;//Added by SDM 2022-08-10
#endif
            SL_MaxLength = 2000;
            bottomSplitHeight = 2700;
            f_dist = 200;
            sec_dist = 300;
            standardPanelHeight = 3000;//Added on 10/03/2023 by SDM
            standardPanelThickness = 50;//Added on 10/03/2023 by SDM
            ver_max_pitch = 100;
            ver_min_pitch = 50;

            wallPanelName = "WP";
            wallTopPanelName = "TP";
            windowPanelName = "WWP";
            windowTopPanelName = "WTP";
            doorTopPanelName = "DTP";
            beamPanelName = "BS";
            beamSidePanelName = "BA";
            deckPanelName = "SP";
            slabJointWallName = "SJ";
            SetUIValuesFromExcel();
        }

        public void setVariablesInUI()
        {
            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count!=0)
            {
                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Floor"))
                    {
                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];

                        string[] arr = data.Split(':');
                        floorHeight = Convert.ToDouble(arr[1]);

                        break;
                    }
                }
            }
           

            floorHeightTextBox.Text = Convert.ToString(floorHeight);
            kickerTextBox.Text = Convert.ToString(kickerHt);
            SLTextBox.Text = Convert.ToString(SL);
            RCTextBox.Text = Convert.ToString(RC);
            standardPanelWidthTextbox.Text = Convert.ToString(standardPanelWidth);
            PanelRC_CheckBox.Checked = flagOfPanelWithRC;
#if !WNPANEL //Added on 26/04/2023 by SDM
            ExtendPanelAtExternalCorners_CheckBox.Checked = flagOfExtendPanelAtExternalCornersOption;//Added by SDM 2022-08-10
            horzPanel_ForDoorWindowBeam_CheckBox.Checked = flagOfHorzPanel_ForDoorWindowBeam;
#endif
            SL_MaxLengthTextBox.Text = Convert.ToString(SL_MaxLength);
            bottomSplitHeight_TextBox.Text = Convert.ToString(bottomSplitHeight);

            tb_panel_height.Text = Convert.ToString(standardPanelHeight);//Added on 10/03/2023 by SDM
            tb_panel_thickness.Text = Convert.ToString(standardPanelThickness);//Added on 10/03/2023 by SDM

            txtbx_F_Dist.Text = Convert.ToString(f_dist);
            sEc_Dist_cmbbox.SelectedItem = sec_dist.ToString();
            txtbox_ver_max_pitch.Text = Convert.ToString(ver_max_pitch);
            txtbox_ver_min_pitch.Text = Convert.ToString(ver_min_pitch);

            wallPanelName_TextBox.Text = wallPanelName;
            wallTopPanelName_TextBox.Text = wallTopPanelName;
            windowPanelName_TextBox.Text = windowPanelName;
            windowTopPanelName_TextBox.Text = windowTopPanelName;
            doorTopPanelName_TextBox.Text = doorTopPanelName;
            beamPanelName_TextBox.Text = beamPanelName;
            deckPanelName_TextBox.Text = deckPanelName;
            slabJointPanelName_TextBox.Text = slabJointWallName;
            beamSideAngleName_TextBox.Text = beamSidePanelName;

        }

        public void getVariablesFromUI()
        {
            floorHeight = Convert.ToDouble(floorHeightTextBox.Text);
            kickerHt = Convert.ToDouble(kickerTextBox.Text);
            SL = Convert.ToDouble(SLTextBox.Text);
            RC = Convert.ToDouble(RCTextBox.Text);
            standardPanelWidth = Convert.ToDouble(standardPanelWidthTextbox.Text);
            standardPanelHeight = Convert.ToDouble(tb_panel_height.Text);//Added on 10/03/2023 by SDM
            standardPanelThickness = Convert.ToDouble(tb_panel_thickness.Text);//Added on 10/03/2023 by SDM

            flagOfPanelWithRC = PanelRC_CheckBox.Checked;
            flagOfExtendPanelAtExternalCornersOption = ExtendPanelAtExternalCorners_CheckBox.Checked;//Added by SDM 2022-08-10
            flagOfHorzPanel_ForDoorWindowBeam = horzPanel_ForDoorWindowBeam_CheckBox.Checked;

            SL_MaxLength = Convert.ToDouble(SL_MaxLengthTextBox.Text);
            bottomSplitHeight = Convert.ToDouble(bottomSplitHeight_TextBox.Text);
            maxHeightOfPanel = bottomSplitHeight + 300;//changes made on 08/02/2022 by RRR
            f_dist = Convert.ToDouble(txtbx_F_Dist.Text);
            sec_dist = Convert.ToDouble(sEc_Dist_cmbbox.SelectedItem.ToString());
            ver_max_pitch = Convert.ToDouble(txtbox_ver_max_pitch.Text);
            ver_min_pitch = Convert.ToDouble(txtbox_ver_min_pitch.Text);

            wallPanelName = wallPanelName_TextBox.Text;
            wallTopPanelName = wallTopPanelName_TextBox.Text;
            windowPanelName = windowPanelName_TextBox.Text;
            windowTopPanelName = windowTopPanelName_TextBox.Text;
            doorTopPanelName = doorTopPanelName_TextBox.Text;
            beamPanelName = beamPanelName_TextBox.Text;
            deckPanelName = deckPanelName_TextBox.Text;
            slabJointWallName = slabJointPanelName_TextBox.Text;
            beamSidePanelName = beamSideAngleName_TextBox.Text;

            window_UI_Helper.getDataFromWindowGridView(windowDataGridView);
            window_UI_Helper.getDataFromchajjaGridView(chajjaDataGridView);
            door_UI_Helper.getDataFromDoorGridView(doorDataGridView);
            beam_UI_Helper.getDataFromBeamGridView(beamDataGridView);//Added on 08/03/2023 by SDM
            beam_UI_Helper.getDataFromOffsetBeamGridView(offsetBeamDataGridView);
            internalSunkanSlab_UI_Helper.getDataFromSunkanSlabGridView(internalSunkanSlabDataGridView);
            internalWallSlab_UI_Helper.getDataFromInternalWallSlabGridView(internalWallSlabDataGridView);
            externalSunkanSlab_UI_Helper.getDataFromExternalSunkanSlabGridView(externalSunkanSlabDataGridView);
            parapet_UI_Helper.getDataFromParapetGridView(parapetDataGridView);
            stairCase_UI_Helper.getDataFromStairCaseGridView(stairCaseDataGridView);
            lift_UI_Helper.getDataFromLiftGridView(liftDataGridView);

            window_UI_Helper.previousDataInWindowGridView(windowDataGridView);
            window_UI_Helper.previousDataInChajjaGridView(chajjaDataGridView);
            door_UI_Helper.previousDataInDoorGridView(doorDataGridView);
            beam_UI_Helper.previousDataInBeamGridView(beamDataGridView);//Added on 08/03/2023 by SDM
            beam_UI_Helper.previousDataInOffsetBeamGridView(offsetBeamDataGridView);
            internalSunkanSlab_UI_Helper.previousDataInSunkanSlabGridView(internalSunkanSlabDataGridView);
            internalWallSlab_UI_Helper.previousDataInInternalWallSlabGridView(internalWallSlabDataGridView);
            externalSunkanSlab_UI_Helper.previousDataInExternalSunkanSlabGridView(externalSunkanSlabDataGridView);
            parapet_UI_Helper.previousDataInParapetGridView(parapetDataGridView);
            stairCase_UI_Helper.previousDataInStairCaseGridView(stairCaseDataGridView);
            lift_UI_Helper.previousDataInLiftGridView(liftDataGridView);
        }

        public void callMethodOfDefaultValue()
        {
            AEE_Utility.SetUCSInWorld();
            if (PanelLayout_UI.flagOfPanelLayoutOption == false)
            {
                setDefaultValue();
            }
        }

        private void createLayerOfCivilModel()
        {
            foreach (var windowLayer in Window_UI_Helper.listOfWindowLayerName)
            {
                AEE_Utility.CreateLayerFromUI(windowLayer, Window_UI_Helper.windowLayerColorIndex);
            }
            foreach (var chajjaLayer in Window_UI_Helper.listOfChajjaLayerName)
            {
                AEE_Utility.CreateLayerFromUI(chajjaLayer, Window_UI_Helper.windowLayerColorIndex);
            }

            foreach (var doorLayer in Door_UI_Helper.listOfDoorLayerName)
            {
                AEE_Utility.CreateLayerFromUI(doorLayer, Door_UI_Helper.doorLayerColor);
            }
            foreach (var beamLayer in Beam_UI_Helper.listOfBeamLayerName)//Added on 08/03/2023 by SDM
            {
                AEE_Utility.CreateLayerFromUI(beamLayer, Beam_UI_Helper.beamLayerColor);
            }
            foreach (var offsetbeamLayer in Beam_UI_Helper.listOfOffsetBeamLayerName)
            {
                AEE_Utility.CreateLayerFromUI(offsetbeamLayer, Beam_UI_Helper.offsetBeamLayerColorIndex);
            }
            foreach (var sunkanSlabLayer in InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLayerName)
            {
                AEE_Utility.CreateLayerFromUI(sunkanSlabLayer, InternalSunkanSlab_UI_Helper.internalSunkanSlabLayerColorIndex);
            }
            foreach (var sunkanSlabLayer in ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLayerName)
            {
                AEE_Utility.CreateLayerFromUI(sunkanSlabLayer, ExternalSunkanSlab_UI_Helper.externalSunkanSlabLayerColorIndex);
            }
            foreach (var sunkanSlabLayer in Parapet_UI_Helper.listOfParapetLayerName)
            {
                AEE_Utility.CreateLayerFromUI(sunkanSlabLayer, Parapet_UI_Helper.parapetLayerColorIndex);
            }
            foreach (var sunkanSlabLayer in StairCase_UI_Helper.listOfStairCaseLayerName)
            {
                AEE_Utility.CreateLayerFromUI(sunkanSlabLayer, 120);
            }
            foreach (var wallLayer in InternalWallSlab_UI_Helper.listOfInternalWallSlabLayerName)
            {
                AEE_Utility.CreateLayerFromUI(wallLayer, InternalWallSlab_UI_Helper.internalWallSlabLayerColorIndex);
            }

            

            //MODIFIED by N - OCT22 - START
            //1. fixing layer creation issue
            //2. Created Mandatory layers like AEE_EC_Wall
            foreach (var liftLayer in Lift_UI_Helper.listOfLiftLayerName)
            {
                AEE_Utility.CreateLayerFromUI(liftLayer, Lift_UI_Helper.liftLayerColorIndex);
            }

            AEE_Utility.CreateLayerFromUI("AEE_Duct", 2);//Added On 20/06/2023 by PRT

            List<string> ListMandatoryLayerName = new List<string>();
            var mandatoryLayerColorIndex = 9;
            ListMandatoryLayerName.Add("AEE_EC_Wall");

            foreach (var manLayer in ListMandatoryLayerName)
            {
                AEE_Utility.CreateLayerFromUI(manLayer, mandatoryLayerColorIndex);
            }
            //MODIFIED by N - OCT22 - END

        }

        public string textFilePath
        {
            get
            {
                var str = getTextFilePathFromDrawing();
                if (string.IsNullOrEmpty(str))
                {
                    return textOldFilePath;
                }
                return str;
            }
        }

        private string getTextFilePathFromDrawing()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                var ed = doc.Editor;
                var dir = Path.GetDirectoryName(doc.Name);
                var fileName = Path.GetFileNameWithoutExtension(doc.Name) + "_" + textFileName + textFileExtension;
                if (string.IsNullOrEmpty(dir))
                {
                    return "";
                }

                return Path.Combine(dir, fileName);
            }
            catch (Exception e)
            {

            }
            return "";
        }

        public void setUIValueFromTextFile()
        {

            PanelLayoutInputHelper panelLayoutInputHlp = new PanelLayoutInputHelper();
            // var flagOfInputFile = panelLayoutInputHlp.readInputTextFile(textFilePath);
            var flagOfInputFile = panelLayoutInputHlp.readinlutactivedoc(ref isnewdoc);
            if (flagOfInputFile == true)
            {
                flagOfPanelLayoutOption = true;
                createLayerOfCivilModel();
            }
            //Changes made on 10/06/2023 by SDM
            else
                flagOfPanelLayoutOption = false;
        }

        private void internalWallSlabDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[InternalWallSlab_UI_Helper.internalWallSlabLayerColumnIndex].Value = InternalWallSlab_UI_Helper.internalWallSlabStrtText + (Convert.ToString(InternalWallSlab_UI_Helper.internalWallSlabNameCount));
            InternalWallSlab_UI_Helper.internalWallSlabNameCount++;
            applyButton.Enabled = true;//Modified by RTJ on May 15, 2021
        }

        private void internalWallSlabDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (internalWallSlabDataGridView.CurrentCell.ColumnIndex == InternalWallSlab_UI_Helper.internalWallSlabBottomColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }

        public static double getSlabThickness(string layerName)
        {
            double slabThicknessByLayer = InternalWallSlab_UI_Helper.getThickness(layerName);
            if (slabThicknessByLayer > 0)
            {
                return slabThicknessByLayer;
            }
            slabThicknessByLayer = InternalSunkanSlab_UI_Helper.getThickness(layerName);
            if (slabThicknessByLayer > 0)
            {
                return slabThicknessByLayer;
            }
            slabThicknessByLayer = ExternalSunkanSlab_UI_Helper.getThickness(layerName);
            if (slabThicknessByLayer > 0)
            {
                return slabThicknessByLayer;
            }

            return PanelLayout_UI.slabThickness;
        }

        private void internalWallSlabDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            InternalWallSlab_UI_Helper.updateSlabThickness(internalWallSlabDataGridView, floorHeightTextBox);
        }

        private void floorHeightTextBox_TextChanged(object sender, EventArgs e)
        {
            InternalWallSlab_UI_Helper.updateSlabThickness(internalWallSlabDataGridView, floorHeightTextBox);
        }

        //Modified by RTJ on May 15, 2021
        private void floorHeightTextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = floorHeightTextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void floorHeightTextBox_Leave(object sender, EventArgs e)
        {
            if (!floorHeightTextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void standardPanelWidthTextbox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = standardPanelWidthTextbox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void standardPanelWidthTextbox_Leave(object sender, EventArgs e)
        {
            if (!standardPanelWidthTextbox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Added on 10/03/2023 by SDM
        private void tb_panel_height_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = tb_panel_height.Text;
        }
        //Added on 10/03/2023 by SDM
        private void tb_panel_height_Leave(object sender, EventArgs e)
        {
            if (!tb_panel_height.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Added on 10/03/2023 by SDM
        private void tb_panel_thickness_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = tb_panel_thickness.Text;
        }
        //Added on 10/03/2023 by SDM
        private void tb_panel_thickness_Leave(object sender, EventArgs e)
        {
            if (!tb_panel_thickness.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void bottomSplitHeight_TextBox_Leave(object sender, EventArgs e)
        {
            if (!bottomSplitHeight_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void bottomSplitHeight_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = bottomSplitHeight_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void SLTextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = SLTextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void SLTextBox_Leave(object sender, EventArgs e)
        {
            if (!SLTextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void kickerTextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = kickerTextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void kickerTextBox_Leave(object sender, EventArgs e)
        {
            if (!kickerTextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void RCTextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = RCTextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void RCTextBox_Leave(object sender, EventArgs e)
        {
            if (!RCTextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void SL_MaxLengthTextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = SL_MaxLengthTextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void SL_MaxLengthTextBox_Leave(object sender, EventArgs e)
        {
            if (!SL_MaxLengthTextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void horzPanel_ForDoorWindowBeam_CheckBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = horzPanel_ForDoorWindowBeam_CheckBox.Checked.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void horzPanel_ForDoorWindowBeam_CheckBox_Leave(object sender, EventArgs e)
        {
            if (!horzPanel_ForDoorWindowBeam_CheckBox.Checked.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void PanelRC_CheckBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = PanelRC_CheckBox.Checked.ToString();
        }
        //Added by SDM 2022-08-06
        private void ExtendPanelAtExternalCorners_CheckBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = ExtendPanelAtExternalCorners_CheckBox.Checked.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void PanelRC_CheckBox_Leave(object sender, EventArgs e)
        {
            if (!PanelRC_CheckBox.Checked.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Added by SDM 2022-08-06
        private void ExtendPanelAtExternalCorners_CheckBox_Leave(object sender, EventArgs e)
        {
            if (!ExtendPanelAtExternalCorners_CheckBox.Checked.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void standardPanelHghtTextbox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = standardPanelHghtTextbox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void standardPanelHghtTextbox_Leave(object sender, EventArgs e)
        {
            if (!standardPanelHghtTextbox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void wallPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = wallPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void wallPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!wallPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void wallTopPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = wallTopPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void wallTopPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!wallTopPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void windowPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = windowPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void windowPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!windowPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void windowTopPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = windowTopPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void windowTopPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!windowTopPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void doorTopPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = doorTopPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void doorTopPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!doorTopPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void beamPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = beamPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void beamPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!beamPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void deckPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = deckPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void deckPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!deckPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void slabJointPanelName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = slabJointPanelName_TextBox.Text;
        }
        //Modified by RTJ on May 15, 2021
        private void slabJointPanelName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!slabJointPanelName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }

        private void beamSideAngleName_TextBox_Enter(object sender, EventArgs e)
        {
            txtUIControlOldValue = beamSideAngleName_TextBox.Text;
        }
        private void beamSideAngleName_TextBox_Leave(object sender, EventArgs e)
        {
            if (!beamSideAngleName_TextBox.Text.Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }

        //Modified by RTJ on May 15, 2021
        private void windowDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (windowDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = windowDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void windowDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (windowDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!windowDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void doorDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (doorDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = doorDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void doorDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (doorDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!doorDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        private void beamDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)//Added on 08/03/2023 by SDM
        {
            if (beamDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = beamDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        private void beamDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)//Added on 08/03/2023 by SDM
        {
            if (beamDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!beamDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }

        //Modified by RTJ on May 15, 2021
        private void offsetBeamDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (offsetBeamDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = offsetBeamDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void offsetBeamDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (offsetBeamDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!offsetBeamDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void internalWallSlabDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (internalWallSlabDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = internalWallSlabDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void internalWallSlabDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (internalWallSlabDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!internalWallSlabDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void internalSunkanSlabDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (internalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = internalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void internalSunkanSlabDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (internalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!internalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void externalSunkanSlabDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (externalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = externalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void externalSunkanSlabDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (externalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!externalSunkanSlabDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void parapetDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (parapetDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = parapetDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void parapetDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (parapetDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!parapetDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void stairCaseDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (stairCaseDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = stairCaseDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void stairCaseDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (stairCaseDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!stairCaseDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }
        //Modified by RTJ on May 15, 2021
        private void liftDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (liftDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = liftDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }
        //Modified by RTJ on May 15, 2021
        private void liftDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (liftDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!liftDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }

        private void txtbx_F_Dist_TextChanged(object sender, EventArgs e)
        {
            f_dist = Convert.ToDouble(txtbx_F_Dist.Text);
            applyButton.Enabled = true;

        }

        private void sEc_Dist_cmbbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            sec_dist = Convert.ToDouble(sEc_Dist_cmbbox.SelectedItem.ToString());
            applyButton.Enabled = true;
        }

        private void txtbox_ver_max_pitch_TextChanged(object sender, EventArgs e)
        {
            ver_max_pitch = Convert.ToDouble(txtbox_ver_max_pitch.Text);
            applyButton.Enabled = true;
        }

        private void txtbox_ver_min_pitch_TextChanged(object sender, EventArgs e)
        {
            ver_min_pitch = Convert.ToDouble(txtbox_ver_min_pitch.Text);
            applyButton.Enabled = true;
        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void windowDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void chajjaDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[Window_UI_Helper.chajjaLayerColumnIndex].Value = Window_UI_Helper.chajjaStrtText + (Convert.ToString(Window_UI_Helper.chajjaLayerCount));
            Window_UI_Helper.chajjaLayerCount++;
            applyButton.Enabled = true;//Changes made on 21/06/2023 by PRT
        }

        private void chajjaDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (chajjaDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            txtUIControlOldValue = chajjaDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
        }

        private void chajjaDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (chajjaDataGridView[e.ColumnIndex, e.RowIndex].Value == null)
                return;
            if (!chajjaDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString().Equals(txtUIControlOldValue))
            {
                applyButton.Enabled = true;
            }
        }

        private void chajjaDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Column_KeyPress);
            if (chajjaDataGridView.CurrentCell.ColumnIndex == Window_UI_Helper.nearThickColumnIndex|| chajjaDataGridView.CurrentCell.ColumnIndex == Window_UI_Helper.farThickColumnIndex)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(Column_KeyPress);
                }
            }
        }

        private void externalSunkanSlabDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void externalSunkanSlabDataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[ExternalSunkanSlab_UI_Helper.externalSunkanSlabLayerColumnIndex].Value = ExternalSunkanSlab_UI_Helper.externalSunkanSlabStrtText + (Convert.ToString(ExternalSunkanSlab_UI_Helper.externalSunkanSlabNameCount));
            ExternalSunkanSlab_UI_Helper.externalSunkanSlabNameCount++;
            applyButton.Enabled = true;
        }

        private void doorDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //Modified by RTJ on May 15, 2021----End
    }


    public class PanelLayout_UI_Helper
    {
        public bool checkPanelLayout_UI(List<GroupBox> groupBoxList)
        {
            bool flag = true;

            foreach (var groupBox in groupBoxList)
            {
                foreach (Control c in groupBox.Controls)
                {
                    if (c is TextBox)
                    {
                        TextBox textBox = c as TextBox;

                        if (textBox.Text == string.Empty && textBox.Visible == true)
                        {
                            textBox.BackColor = Color.Red;

                            if (flag == true) // if any textBox, comboBox, NumericUpDown is empty, then flag value is false.
                            {
                                flag = false;
                            }
                        }
                    }
                    else if (c is ComboBox)
                    {
                        ComboBox comboBox = c as ComboBox;
                        if (comboBox.SelectedIndex == -1 && comboBox.Visible == true)
                        {
                            comboBox.BackColor = Color.Red;

                            if (flag == true) // if any textBox, comboBox, NumericUpDown is empty, then flag value is false.
                            {
                                flag = false;
                            }
                        }
                    }
                    else if (c is NumericUpDown)
                    {
                        NumericUpDown numericUpDown = c as NumericUpDown;
                        if (numericUpDown.Text == string.Empty && numericUpDown.Visible == true)
                        {
                            numericUpDown.BackColor = Color.Red;

                            if (flag == true) // if any textBox, comboBox, NumericUpDown is empty, then flag value is false.
                            {
                                flag = false;
                            }
                        }
                    }
                }
            }

            if (flag == false)
            {
                string message = "Feel in values for all the parameters and click OK";
                MessageBox.Show(message, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return flag;
        }

        public void keyPressValidate(object sender, KeyPressEventArgs e, TextBox textbox)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) /*&& e.KeyChar != '.'*/)
            {
                e.Handled = true;
            }

            //if (Regex.IsMatch(textbox.Text, "^\\d*\\.\\d{2}$") && e.KeyChar != (char)Keys.Back)
            //{
            //    // Allow 2 decimal places in a textbox
            //    e.Handled = true;
            //}

            //// only allow one decimal point
            //if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            //{
            //    e.Handled = true;
            //}
        }

        public void validateOfDeleteButton(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
            }
        }

        public bool textBox_Validating(object sender, CancelEventArgs e, TextBox textbox, string emptyText)
        {
            if (string.IsNullOrEmpty(textbox.Text) || string.IsNullOrWhiteSpace(textbox.Text))
            {
                textbox.BackColor = Color.Red;
                MessageBox.Show(emptyText, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;

                return false;
            }
            return true;
        }

        public bool textBox_Validating(object sender, CancelEventArgs e, TextBox textbox, double minimumValue, double maximumValue, string message)
        {
            string minValue = Convert.ToString(minimumValue);
            string maxValue = Convert.ToString(maximumValue);

            string errorMessage = "Valid range of " + message + " is " + minValue + " to " + maxValue;

            if (textbox.Text != "")
            {
                int value = Convert.ToInt32(textbox.Text);
                if (value >= minimumValue && value <= maximumValue)
                {

                }
                else
                {
                    MessageBox.Show(errorMessage, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                    return false;
                }
            }
            else
            {
                MessageBox.Show(errorMessage, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return false;
            }
            return true;
        }
    }

    public class PanelLayoutInputHelper
    {
        string floorHeightInputName = "FloorHeight=";
        string wallSlabBottomInputName = "WallSlabBottom=";
        string slabThickInputName = "SlabThick=";
        string panelWidthInputName = "PanelWidth=";
        //string maxCornerHeightInputName = "MaxCornerHeight=";
        string bottomSplitHeightInputName = "BottomSplitHeight=";
        string hrPanelForDoorWindowAndBeamInputName = "HrPanelForDoorWindowAndBeam=";
        string SLInputName = "SL=";
        string kickerInputName = "Kicker=";
        string RCInputName = "RC=";
        string maxSlLengthInputName = "Max.SLLength=";
        string panelWithoutRCInputName = "PanelWithoutRC=";

        string wallPanelInputName = "WallPanel=";
        string wallTopPanelInputName = "WallTopPanel=";
        string windowWallPanelInputName = "WindowWallPanel=";
        string windowTopPanelInputName = "WindowTopPanel=";
        string doorTopPanelInputName = "DoorTopPanel=";
        string beamPanelInputName = "BeamPanel=";
        string deckPanelInputName = "DeckPanel=";
        string slabJointPanelInputName = "SlabJointPanel=";
        string beamSideAngleInputName = "BeamSideAngle=";

        string windowLayerInputName = "Layer=";
        string windowSillLevelInputName = "SillLevel=";
        string windowLintelLevelInputName = "LintelLevel=";

        string chajjaLayerInputName = "Layer=";
        string chajjaNearThickInputName = "Near Thick=";
        string chajjaFarThickInputName = "Far Thick=";

        string doorLayerInputName = "Layer=";
        string doorLintelLevelInputName = "LintelLevel=";

        string beamLayerInputName = "Layer=";
        string beamHeightInputName = "Height=";

        string offsetBeamLayerInputName = "Layer=";
        string offsetBeamBottomInputName = "BeamBottom=";
        string offsetBeamTopInputName = "BeamTop=";

        string intrnlSnkanSlbLayerInputName = "Layer=";
        string intrnlSnkanSlbLevelDiffernceInputName = "LevelDiffernce=";
        string intrnlSnkanSlbThicknessInputName = "SlabThickness=";

        string extrnlSnkanSlbLayerInputName = "Layer=";
        string extrnlSnkanSlbLevelDiffernceInputName = "LevelDiffernce=";
        string extrnlSnkanSlbThicknessInputName = "SlabThickness=";

        string parapetLayerInputName = "Layer=";
        string parapetBottomInputName = "ParapetBottom=";
        string parapetTopInputName = "ParapetTop=";

        string staircaseLayerInputName = "Layer=";
        string liftcaseLayerInputName = "Layer=";

        string sizeInputName = "Size";
        string annotationInputName = "Annotation";
        string windowInputName = "Window";
        string chajjaInputName = "Chajja";
        string doorInputName = "Door";
        string beamInputName = "Beam";
        string offsetBeamInputName = "OffsetBeam";
        string internalWallSlabInputName = "InternalWallSlab";
        string internalSunkanSlabInputName = "InternalSunkanSlab";
        string externalSunkanSlabInputName = "ExternalSunkanSlab";
        string parapetInputName = "Parapet";
        string staircaseInputName = "Staircase";
        string liftInputName = "Lift";
        string CenterlineInputName = "centerline";

        string comma = ",";

        private static List<string> listOfWriteDataInTextFile = new List<string>();
        public static List<string> listOfAllSelectedEntity = new List<string>();
        public void setDataFromUI_ForTextFile()
        {
            listOfWriteDataInTextFile.Clear();

            setSizeDataForTextFile();
            setAnnotationDataForTextFile();
            setWindowDataForTextFile();
            setChajjaDataForTextFile();
            setDoorDataForTextFile();
            setBeamDataForTextFile();
            setOffsetBeamDataForTextFile();
            setInternalSunkanSlabDataForTextFile();
            setExternalSunkanSlabDataForTextFile();
            setParapetDataForTextFile();
            setStaircaseDataForTextFile();
            setLiftDataForTextFile();
            setInternalWallSlabDataForTextFile();
            centerline_details();
        }

        private void setSizeDataForTextFile()
        {
            string slabBottomText = floorHeightInputName + Convert.ToString(PanelLayout_UI.floorHeight) + comma;
            string slabThickText = slabThickInputName + Convert.ToString(PanelLayout_UI.slabThickness) + comma;
            string panelWidthText = panelWidthInputName + Convert.ToString(PanelLayout_UI.standardPanelWidth) + comma;
            string maxCornerHeightText = bottomSplitHeightInputName + Convert.ToString(PanelLayout_UI.bottomSplitHeight) + comma;
            string hrPanelForDoorWindowAndBeamText = hrPanelForDoorWindowAndBeamInputName + Convert.ToString(PanelLayout_UI.flagOfHorzPanel_ForDoorWindowBeam) + comma;
            string SLText = SLInputName + Convert.ToString(PanelLayout_UI.SL) + comma;
            string kickerText = kickerInputName + Convert.ToString(PanelLayout_UI.kickerHt) + comma;
            string RCText = RCInputName + Convert.ToString(PanelLayout_UI.RC) + comma;
            string maxSlLengthText = maxSlLengthInputName + Convert.ToString(PanelLayout_UI.SL_MaxLength) + comma;
            string panelWithoutRCText = panelWithoutRCInputName + Convert.ToString(PanelLayout_UI.flagOfPanelWithRC);

            string sizeData = slabBottomText + slabThickText + panelWidthText + maxCornerHeightText + hrPanelForDoorWindowAndBeamText +
               SLText + kickerText + RCText + maxSlLengthText + panelWithoutRCText;

            listOfWriteDataInTextFile.Add(sizeInputName);
            listOfWriteDataInTextFile.Add(sizeData);
        }
        private void setAnnotationDataForTextFile()
        {
            string wallPanelText = wallPanelInputName + Convert.ToString(PanelLayout_UI.wallPanelName) + comma;
            string wallTopPanelText = wallTopPanelInputName + Convert.ToString(PanelLayout_UI.wallTopPanelName) + comma;
            string windowWallPanelText = windowWallPanelInputName + Convert.ToString(PanelLayout_UI.windowPanelName) + comma;
            string windowTopPanelText = windowTopPanelInputName + Convert.ToString(PanelLayout_UI.windowTopPanelName) + comma;
            string doorTopPanelText = doorTopPanelInputName + Convert.ToString(PanelLayout_UI.doorTopPanelName) + comma;
            string beamPanelText = beamPanelInputName + Convert.ToString(PanelLayout_UI.beamPanelName) + comma;
            string deckPanelText = deckPanelInputName + Convert.ToString(PanelLayout_UI.deckPanelName) + comma;
            string slabJointPanelText = slabJointPanelInputName + Convert.ToString(PanelLayout_UI.slabJointWallName) + comma;
            string beamSideAngleText = beamSideAngleInputName + Convert.ToString(PanelLayout_UI.beamSidePanelName);


            string annotationData = wallPanelText + wallTopPanelText + windowWallPanelText + windowTopPanelText + doorTopPanelText +
                                    beamPanelText + deckPanelText + slabJointPanelText + beamSideAngleText;

            listOfWriteDataInTextFile.Add(annotationInputName);
            listOfWriteDataInTextFile.Add(annotationData);
        }

        private void setWindowDataForTextFile()
        {
            for (int index = 0; index < Window_UI_Helper.listOfWindowLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(windowInputName);
                }
                string layerName = windowLayerInputName + Window_UI_Helper.listOfWindowLayerName[index];
                string sillLevel = windowSillLevelInputName + Convert.ToString(Window_UI_Helper.listOfWindowSillLevel[index]);
                string lintelLevel = windowLintelLevelInputName + Convert.ToString(Window_UI_Helper.listOfWindowLintelLevel[index]);

                string windowData = layerName + comma + sillLevel + comma + lintelLevel;
                listOfWriteDataInTextFile.Add(windowData);
            }
        }
        private void setChajjaDataForTextFile()
        {
            for (int index = 0; index < Window_UI_Helper.listOfChajjaLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(chajjaInputName);
                }
                string layerName = windowLayerInputName + Window_UI_Helper.listOfChajjaLayerName[index];
                string nearThick = "Near Thick" + Convert.ToString(Window_UI_Helper.listOfNearSideThickness[index]);
                string farThick = "Far Thick" + Convert.ToString(Window_UI_Helper.listOfFarSideThickness[index]);

                string ChajjaData = layerName + comma + nearThick + comma + farThick;
                listOfWriteDataInTextFile.Add(ChajjaData);
            }
        }

        private void setDoorDataForTextFile()
        {
            for (int index = 0; index < Door_UI_Helper.listOfDoorLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(doorInputName);
                }
                string layerName = doorLayerInputName + Door_UI_Helper.listOfDoorLayerName[index];
                string lintelLevel = doorLintelLevelInputName + Convert.ToString(Door_UI_Helper.listOfDoorLintelLevel[index]);

                string doorData = layerName + comma + lintelLevel;
                listOfWriteDataInTextFile.Add(doorData);
            }
        }
        private void setBeamDataForTextFile()
        {
            for (int index = 0; index < Beam_UI_Helper.listOfBeamLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(beamInputName);
                }
                string layerName = beamLayerInputName + Beam_UI_Helper.listOfBeamLayerName[index];
                string height = beamHeightInputName + Convert.ToString(Beam_UI_Helper.listOfBeamHeight[index]);

                string beamData = layerName + comma + height;
                listOfWriteDataInTextFile.Add(beamData);
            }
        }

        private void setOffsetBeamDataForTextFile()
        {
            for (int index = 0; index < Beam_UI_Helper.listOfOffsetBeamLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(offsetBeamInputName);
                }
                string layerName = offsetBeamLayerInputName + Beam_UI_Helper.listOfOffsetBeamLayerName[index];
                string beamBottom = offsetBeamBottomInputName + Convert.ToString(Beam_UI_Helper.listOfOffsetBeamBottom[index]);
                string beamTop = offsetBeamTopInputName + Convert.ToString(Beam_UI_Helper.listOfOffsetBeamTop[index]);

                string beamData = layerName + comma + beamBottom + comma + beamTop;
                listOfWriteDataInTextFile.Add(beamData);
            }
        }

        private void setInternalSunkanSlabDataForTextFile()
        {
            for (int index = 0; index < InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(internalSunkanSlabInputName);
                }
                string layerName = intrnlSnkanSlbLayerInputName + InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLayerName[index];
                string levelDiffence = intrnlSnkanSlbLevelDiffernceInputName + Convert.ToString(InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLevelDifference[index]);
                string slabThick = intrnlSnkanSlbThicknessInputName + Convert.ToString(InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabThick[index]);

                string internalSunkanData = layerName + comma + levelDiffence + comma + slabThick;
                listOfWriteDataInTextFile.Add(internalSunkanData);
            }
        }

        private void centerline_details()
        {
            listOfWriteDataInTextFile.Add(CenterlineInputName);

            listOfWriteDataInTextFile.Add(PanelLayout_UI.f_dist.ToString() + comma + PanelLayout_UI.sec_dist.ToString() + comma + PanelLayout_UI.ver_max_pitch.ToString() + comma + PanelLayout_UI.ver_min_pitch.ToString());
        }
        private void setInternalWallSlabDataForTextFile()
        {
            listOfWriteDataInTextFile.Add(internalWallSlabInputName);
            for (int index = 0; index < InternalWallSlab_UI_Helper.listOfInternalWallSlabLayerName.Count; index++)
            {
                string layerName = intrnlSnkanSlbLayerInputName + InternalWallSlab_UI_Helper.listOfInternalWallSlabLayerName[index];
                string slabBottom = wallSlabBottomInputName + Convert.ToString(InternalWallSlab_UI_Helper.listOfInternalWallSlabBottom[index]);

                string internalSunkanData = layerName + comma + slabBottom;
                listOfWriteDataInTextFile.Add(internalSunkanData);
            }
        }

        private void setExternalSunkanSlabDataForTextFile()
        {
            for (int index = 0; index < ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(externalSunkanSlabInputName);
                }
                string layerName = extrnlSnkanSlbLayerInputName + ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLayerName[index];
                string levelDiffence = extrnlSnkanSlbLevelDiffernceInputName + Convert.ToString(ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLevelDifference[index]);
                string slabThick = extrnlSnkanSlbThicknessInputName + Convert.ToString(ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabThick[index]);

                string externalSunkanData = layerName + comma + levelDiffence + comma + slabThick;
                listOfWriteDataInTextFile.Add(externalSunkanData);
            }
        }

        private void setParapetDataForTextFile()
        {
            for (int index = 0; index < Parapet_UI_Helper.listOfParapetLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(parapetInputName);
                }
                string layerName = parapetLayerInputName + Parapet_UI_Helper.listOfParapetLayerName[index];
                string parapetBottom = parapetBottomInputName + Convert.ToString(Parapet_UI_Helper.listOfParapetBottom[index]);
                string parapetTop = parapetTopInputName + Convert.ToString(Parapet_UI_Helper.listOfParapetTop[index]);

                string parapetData = layerName + comma + parapetBottom + comma + parapetTop;
                listOfWriteDataInTextFile.Add(parapetData);
            }
        }

        private void setStaircaseDataForTextFile()
        {
            for (int index = 0; index < StairCase_UI_Helper.listOfStairCaseLayerName.Count; index++)
            {
                if (index == 0)
                {
                    listOfWriteDataInTextFile.Add(staircaseInputName);
                }
                string layerName = staircaseLayerInputName + StairCase_UI_Helper.listOfStairCaseLayerName[index];

                string staircaseData = layerName;
                listOfWriteDataInTextFile.Add(staircaseData);
            }
        }

        private void setLiftDataForTextFile()
        {
            listOfWriteDataInTextFile.Add(liftInputName);
            for (int index = 0; index < Lift_UI_Helper.listOfLiftLayerName.Count; index++)
            {
                string layerName = liftcaseLayerInputName + Lift_UI_Helper.listOfLiftLayerName[index];

                string liftData = layerName;
                listOfWriteDataInTextFile.Add(liftData);
            }
        }

        public void write_UI_ValueInTextFile(string textFilePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(textFilePath))
                {
                    for (int index = 0; index < listOfWriteDataInTextFile.Count; index++)
                    {
                        try
                        {
                            string text = listOfWriteDataInTextFile[index];
                            writer.WriteLine(text);
                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                            File.Delete(textFilePath);
                        }
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                File.Delete(textFilePath);
            }
        }

        public void selectPanelLayoutWithWindowSelection_Text(string promptSelMsg)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;



            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult acSSPrompt = default(PromptSelectionResult);
                PromptSelectionOptions acKeywordOpts = new PromptSelectionOptions();

                acKeywordOpts.MessageForAdding = (promptSelMsg);

                TypedValue[] filterlist = new TypedValue[1];
                filterlist.SetValue(new TypedValue((int)DxfCode.Start, "MTEXT"), 0);
                SelectionFilter filter = new SelectionFilter(filterlist);
                acSSPrompt = ed.GetSelection(acKeywordOpts, filter);

                //acSSPrompt = ed.GetSelection(acKeywordOpts);

                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    // Start a transaction to open the selected objects
                    SelectionSet acSSet = acSSPrompt.Value;
                    // Iterate through the selections set
                    foreach (SelectedObject so in acSSet)
                    {

                        Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;

                        if (ent.GetType() == typeof(MText))
                        {
                            string text = ((MText)ent).Contents.ToString();
                            //string d = "\p";
                            string[] data = text.Split(';');
                            string data1 = data[4];

                            string t = data1.Replace("\\P", ";");
                            string[] data2 = t.Split(';');
                            listOfAllSelectedEntity = data2.OfType<string>().ToList(); 
                        }

                    }
                }
                else if (acSSPrompt.Status == PromptStatus.Cancel)
                {
                    //escBtnCount++;
                }
                else if (acSSPrompt.Status == PromptStatus.Error)
                {
                    //enterBtnCount++;
                }



                ed.WriteMessage("\n\n");

                tr.Commit();
            }
        }


        public bool readinlutactivedoc(ref bool isnewdoc)
        {

            List<string> listOfReadDataFromTextFile = new List<string>();
            bool flag = false;
            try
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (acDoc == null)
                {
                    return flag;
                }


                Autodesk.AutoCAD.DatabaseServices.Database db = acDoc.Database;

                DocumentLock lok = acDoc.LockDocument();
                using (lok)
                {
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            // open the NOD for read
                            DBDictionary nod = trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                            try
                            {



                                // check to see if our entry is in there, excpetion will be thrown if not so process that
                                // condition in the catch
                                ObjectId entryId = nod.GetAt("MyData");

                                // if we are here, then all is ok
                                // print the data
                                //ed.WriteMessage("This entity already has data...");
                                // ok extract the xrecord
                                Xrecord myXrecord = default(Xrecord);
                                // read it from the NOD dictionary
                                myXrecord = trans.GetObject(entryId, OpenMode.ForRead) as Xrecord;

                                // now print out the values
                                // Get data from Xrecord
                                ResultBuffer resBuf = myXrecord.Data;
                                TypedValue[] resbufvalue = resBuf.AsArray();
                                string value = string.Empty;

                                value = resbufvalue[0].Value.ToString();

                                listOfReadDataFromTextFile = value.Split('|').ToList();

                                setDataFromTextFile_To_UI(listOfReadDataFromTextFile);

                                flag = true;

                            }
                            catch (Exception ex)
                            {
                                isnewdoc = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            isnewdoc = true;
                        }

                        trans.Commit();
                    }
                }
            }
            catch { }
            return flag;
        }

        public bool storeinlutactivedoc()
        {
            bool flag = false;
            try
            {

                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database db = acDoc.Database;

                DocumentLock lok = acDoc.LockDocument();
                using (lok)
                {
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {

                        // Find the NOD in the database
                        try
                        {
                            DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                            Xrecord myXrecord = new Xrecord();

                            // create the resbuf list
                            ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Text, string.Join("|", listOfWriteDataInTextFile.ToList())));

                            // now add it to the xrecord
                            myXrecord.Data = data;

                            // create the entry
                            nod.SetAt("MyData", myXrecord);
                            // tell the transaction about the newly created 

                            trans.AddNewlyCreatedDBObject(myXrecord, true);

                            flag = true;
                        }
                        catch (Exception ex)
                        {

                        }
                        trans.Commit();
                    }
                }
            }
            catch { }
            return flag;
        }
        public bool readInputTextFile(string textFilePath)
        {
            List<string> listOfReadDataFromTextFile = new List<string>();

            bool flag = false;
            try
            {
                if (File.Exists(textFilePath))
                {
                    FileStream fileStream = new FileStream(textFilePath, FileMode.Open, FileAccess.Read);
                    using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (line == "")
                            {

                            }
                            else
                            {
                                listOfReadDataFromTextFile.Add(line);
                            }
                        }

                        setDataFromTextFile_To_UI(listOfReadDataFromTextFile);
                    }

                    flag = true;
                }
                else
                {
                    //string messge = "Input text file does not exist.";
                    //MessageBox.Show(messge, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }

            return flag;
        }

        private void setDataFromTextFile_To_UI(List<string> listOfReadDataFromTextFile)
        {
            for (int index = 0; index < listOfReadDataFromTextFile.Count; index++)
            {
                string text = listOfReadDataFromTextFile[index];
                string[] textArray = text.Split(',');
                if (text == sizeInputName)
                {
                    index = setSizeDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == annotationInputName)
                {
                    index = setAnnotationDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == windowInputName)
                {
                    index = setWindowDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == chajjaInputName)
                {
                    index = setChajjaDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }

                else if (text == doorInputName)
                {
                    index = setDoorDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                //Added on 13/03/2023 by SDM
                else if (text == beamInputName)
                {
                    index = setBeamDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == offsetBeamInputName)
                {
                    index = setOffsetBeamDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == internalSunkanSlabInputName)
                {
                    index = setInternalSunkanSlabDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == internalWallSlabInputName)
                {
                    index = setInternalWallSlabDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == externalSunkanSlabInputName)
                {
                    index = setExternalSunkanSlabDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == parapetInputName)
                {
                    index = setParapetDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == staircaseInputName)
                {
                    index = setStaircaseDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == liftInputName)
                {
                    index = setLiftDataFromTextFile_To_UI(index, listOfReadDataFromTextFile);
                }
                else if (text == CenterlineInputName)
                {
                    index = centerline_details_To_UI(index, listOfReadDataFromTextFile);
                }
            }
            InternalWallSlab_UI_Helper.InitOnEmpty();
        }

        private int setSizeDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            int sizeDataIndex = index + 1;
            string sizeDataText = listOfReadDataFromTextFile[sizeDataIndex];
            string[] sizeDataTextArray = sizeDataText.Split(',');
            for (int k = 0; k < sizeDataTextArray.Length; k++)
            {
                string text = sizeDataTextArray[k];
                if (k == 0)
                {
                    string slabBottomText = text.Replace(floorHeightInputName, "");
                    int val = 0;
                    if (int.TryParse(slabBottomText, out val))
                    {
                        PanelLayout_UI.floorHeight = Convert.ToDouble(val);
                    }
                    else
                    {
                        PanelLayout_UI.floorHeight = PanelLayout_UI.defaultFloorHeight;
                    }
                }
                else if (k == 1)
                {
                    string slabThickText = text.Replace(slabThickInputName, "");
                }
                else if (k == 2)
                {
                    string panelWidthText = text.Replace(panelWidthInputName, "");
                    PanelLayout_UI.standardPanelWidth = Convert.ToDouble(panelWidthText);
                }
                else if (k == 3)
                {
                    string bottomSplitHeightText = "2700";
                    if (text.Contains(bottomSplitHeightInputName))
                    {
                        bottomSplitHeightText = text.Replace(bottomSplitHeightInputName, "");
                    }
                    PanelLayout_UI.bottomSplitHeight = Convert.ToDouble(bottomSplitHeightText);
                }
#if !WNPANEL //Added on 26/04/2023 by SDM
                else if (k == 4)
                {
                    string hrPanelForDoorWindowAndBeamText = text.Replace(hrPanelForDoorWindowAndBeamInputName, "");
                    PanelLayout_UI.flagOfHorzPanel_ForDoorWindowBeam = Convert.ToBoolean(hrPanelForDoorWindowAndBeamText);
                }
#endif
                else if (k == 5)
                {
                    string SLText = text.Replace(SLInputName, "");
                    PanelLayout_UI.SL = Convert.ToDouble(SLText);
                }
                else if (k == 6)
                {
                    string kickerText = text.Replace(kickerInputName, "");
                    PanelLayout_UI.kickerHt = Convert.ToDouble(kickerText);
                }
                else if (k == 7)
                {
                    string RCText = text.Replace(RCInputName, "");
                    PanelLayout_UI.RC = Convert.ToDouble(RCText);
                }
                else if (k == 8)
                {
                    string maxSlLengthText = text.Replace(maxSlLengthInputName, "");
                    PanelLayout_UI.SL_MaxLength = Convert.ToDouble(maxSlLengthText);
                }
                else if (k == 9)
                {
                    string panelWithoutRCText = text.Replace(panelWithoutRCInputName, "");
                    PanelLayout_UI.flagOfPanelWithRC = Convert.ToBoolean(panelWithoutRCText);
                }
            }

            return sizeDataIndex;
        }

        private int setAnnotationDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            int annotationDataIndex = index + 1;
            string annotationText = listOfReadDataFromTextFile[annotationDataIndex];
            string[] annotationTextArray = annotationText.Split(',');
            for (int k = 0; k < annotationTextArray.Length; k++)
            {
                string text = annotationTextArray[k];
                if (k == 0)
                {
                    string wallPanelText = text.Replace(wallPanelInputName, "");
                    PanelLayout_UI.wallPanelName = Convert.ToString(wallPanelText);
                }
                else if (k == 1)
                {
                    string wallTopPanelText = text.Replace(wallTopPanelInputName, "");
                    PanelLayout_UI.wallTopPanelName = Convert.ToString(wallTopPanelText);
                }
                else if (k == 2)
                {
                    string windowWallPanelText = text.Replace(windowWallPanelInputName, "");
                    PanelLayout_UI.windowPanelName = Convert.ToString(windowWallPanelText);
                }
                else if (k == 3)
                {
                    string windowTopPanelText = text.Replace(windowTopPanelInputName, "");
                    PanelLayout_UI.windowTopPanelName = Convert.ToString(windowTopPanelText);
                }
                else if (k == 4)
                {
                    string doorTopPanelText = text.Replace(doorTopPanelInputName, "");
                    PanelLayout_UI.doorTopPanelName = Convert.ToString(doorTopPanelText);
                }
                else if (k == 5)
                {
                    string beamPanelText = text.Replace(beamPanelInputName, "");
                    PanelLayout_UI.beamPanelName = Convert.ToString(beamPanelText);
                }
                else if (k == 6)
                {
                    string deckPanelText = text.Replace(deckPanelInputName, "");
                    PanelLayout_UI.deckPanelName = Convert.ToString(deckPanelText);
                }
                else if (k == 7)
                {
                    string slabJointPanelText = text.Replace(slabJointPanelInputName, "");
                    PanelLayout_UI.slabJointWallName = Convert.ToString(slabJointPanelText);
                }
                else if (k == 8)
                {
                    string beamSideAngleText = text.Replace(beamSideAngleInputName, "");
                    PanelLayout_UI.beamSidePanelName = Convert.ToString(beamSideAngleText);
                }
            }

            return annotationDataIndex;
        }

        private int setWindowDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            Window_UI_Helper.listOfWindowLayerName.Clear();
            Window_UI_Helper.listOfWindowSillLevel.Clear();
            Window_UI_Helper.listOfWindowLintelLevel.Clear();

            int windowDataIndex = index + 1;
            for (; windowDataIndex < listOfReadDataFromTextFile.Count; windowDataIndex++)
            {
                string windowText = listOfReadDataFromTextFile[windowDataIndex];
                string[] windowTextArray = windowText.Split(',');
                if (windowTextArray.Length <= 1)
                {
                    windowDataIndex = windowDataIndex - 1;
                    break;
                }

                for (int k = 0; k < windowTextArray.Length; k++)
                {
                    string text = windowTextArray[k];
                    if (k == 0)
                    {
                        string windowLyrName = text.Replace(windowLayerInputName, "");
                        string windowLayerName = Convert.ToString(windowLyrName);
                        Window_UI_Helper.listOfWindowLayerName.Add(windowLayerName);
                    }
                    else if (k == 1)
                    {
                        string windowSillLvl = text.Replace(windowSillLevelInputName, "");
                        double windowSillLevel = Convert.ToDouble(windowSillLvl);
                        Window_UI_Helper.listOfWindowSillLevel.Add(windowSillLevel);
                    }
                    else if (k == 2)
                    {
                        string windowLintlLvl = text.Replace(windowLintelLevelInputName, "");
                        double windowLintelLevel = Convert.ToDouble(windowLintlLvl);
                        Window_UI_Helper.listOfWindowLintelLevel.Add(windowLintelLevel);
                    }
                }
            }

            return windowDataIndex;
        }

        private int setChajjaDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            Window_UI_Helper.listOfChajjaLayerName.Clear();
            Window_UI_Helper.listOfNearSideThickness.Clear();
            Window_UI_Helper.listOfFarSideThickness.Clear();

            int chajjaDataIndex = index + 1;
            for (; chajjaDataIndex < listOfReadDataFromTextFile.Count; chajjaDataIndex++)
            {
                string chajjaText = listOfReadDataFromTextFile[chajjaDataIndex];
                string[]chajjaArray = chajjaText.Split(',');
                if (chajjaArray.Length <= 1)
                {
                    chajjaDataIndex = chajjaDataIndex - 1;
                    break;
                }

                for (int k = 0; k < chajjaArray.Length; k++)
                {
                    string text = chajjaArray[k];
                    if (k == 0)
                    {
                        string chajjaLyrName = text.Replace(chajjaLayerInputName, "");
                        string chajjaLayerName = Convert.ToString(chajjaLyrName);
                        Window_UI_Helper.listOfChajjaLayerName.Add(chajjaLayerName);
                    }
                    else if (k == 1)
                    {
                        string chajjaNearThick = text.Replace(chajjaNearThickInputName, "");
                        double chajjaThickLevel = Convert.ToDouble(chajjaNearThick);
                        Window_UI_Helper.listOfNearSideThickness.Add(chajjaThickLevel);
                    }
                    else if (k == 2)
                    {
                        string chajjaFarthick = text.Replace(chajjaFarThickInputName, "");
                        double chajjaFarThickLevel = Convert.ToDouble(chajjaFarthick);
                        Window_UI_Helper.listOfFarSideThickness.Add(chajjaFarThickLevel);
                    }
                }
            }

            return chajjaDataIndex;
        }


        private int setDoorDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            Door_UI_Helper.listOfDoorLayerName.Clear();
            Door_UI_Helper.listOfDoorLintelLevel.Clear();

            int doorDataIndex = index + 1;
            for (; doorDataIndex < listOfReadDataFromTextFile.Count; doorDataIndex++)
            {
                string doorText = listOfReadDataFromTextFile[doorDataIndex];
                string[] doorTextArray = doorText.Split(',');
                if (doorTextArray.Length <= 1)
                {
                    doorDataIndex = doorDataIndex - 1;
                    break;
                }

                for (int k = 0; k < doorTextArray.Length; k++)
                {
                    string text = doorTextArray[k];
                    if (k == 0)
                    {
                        string doorLyrName = text.Replace(doorLayerInputName, "");
                        string doorLayerName = Convert.ToString(doorLyrName);
                        Door_UI_Helper.listOfDoorLayerName.Add(doorLayerName);
                    }
                    else if (k == 1)
                    {
                        string doorLintlLvl = text.Replace(doorLintelLevelInputName, "");
                        double doorLintelLevel = Convert.ToDouble(doorLintlLvl);
                        Door_UI_Helper.listOfDoorLintelLevel.Add(doorLintelLevel);
                    }
                }
            }

            return doorDataIndex;
        }

        private int setOffsetBeamDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            Beam_UI_Helper.listOfOffsetBeamLayerName.Clear();
            Beam_UI_Helper.listOfOffsetBeamTop.Clear();
            Beam_UI_Helper.listOfOffsetBeamBottom.Clear();

            int beamDataIndex = index + 1;
            for (; beamDataIndex < listOfReadDataFromTextFile.Count; beamDataIndex++)
            {
                string beamText = listOfReadDataFromTextFile[beamDataIndex];
                string[] beamTextArray = beamText.Split(',');
                if (beamTextArray.Length <= 1)
                {
                    beamDataIndex = beamDataIndex - 1;
                    break;
                }

                for (int k = 0; k < beamTextArray.Length; k++)
                {
                    string text = beamTextArray[k];
                    if (k == 0)
                    {
                        string beamLyrName = text.Replace(offsetBeamLayerInputName, "");
                        string beamLayerName = Convert.ToString(beamLyrName);
                        Beam_UI_Helper.listOfOffsetBeamLayerName.Add(beamLayerName);
                    }
                    else if (k == 1)
                    {
                        string beamBttm = text.Replace(offsetBeamBottomInputName, "");
                        double beamBottom = Convert.ToDouble(beamBttm);
                        Beam_UI_Helper.listOfOffsetBeamBottom.Add(beamBottom);
                    }
                    else if (k == 2)
                    {
                        string beamTopText = text.Replace(offsetBeamTopInputName, "");
                        string beamTop = Convert.ToString(beamTopText);
                        Beam_UI_Helper.listOfOffsetBeamTop.Add(beamTop);
                    }
                }
            }

            return beamDataIndex;
        }

        private int setBeamDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            Beam_UI_Helper.listOfBeamLayerName.Clear();
            Beam_UI_Helper.listOfBeamHeight.Clear();

            int beamDataIndex = index + 1;
            for (; beamDataIndex < listOfReadDataFromTextFile.Count; beamDataIndex++)
            {
                string beamText = listOfReadDataFromTextFile[beamDataIndex];
                string[] beamTextArray = beamText.Split(',');
                if (beamTextArray.Length <= 1)
                {
                    beamDataIndex = beamDataIndex - 1;
                    break;
                }

                for (int k = 0; k < beamTextArray.Length; k++)
                {
                    string text = beamTextArray[k];
                    if (k == 0)
                    {
                        string beamLyrName = text.Replace(beamLayerInputName, "");
                        string beamLayerName = Convert.ToString(beamLyrName);
                        Beam_UI_Helper.listOfBeamLayerName.Add(beamLayerName);
                    }
                    else if (k == 1)
                    {
                        string beamBttm = text.Replace(beamHeightInputName, "");
                        double beamBottom = Convert.ToDouble(beamBttm);
                        Beam_UI_Helper.listOfBeamHeight.Add(beamBottom);
                    }

                }
            }

            return beamDataIndex;
        }

        private int setInternalSunkanSlabDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLayerName.Clear();
            InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLevelDifference.Clear();
            InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabThick.Clear();

            int intrnlSunkIndex = index + 1;
            for (; intrnlSunkIndex < listOfReadDataFromTextFile.Count; intrnlSunkIndex++)
            {
                string intrnlSunkText = listOfReadDataFromTextFile[intrnlSunkIndex];
                string[] intrnlSunkTextArray = intrnlSunkText.Split(',');
                if (intrnlSunkTextArray.Length <= 1)
                {
                    intrnlSunkIndex = intrnlSunkIndex - 1;
                    break;
                }

                for (int k = 0; k < intrnlSunkTextArray.Length; k++)
                {
                    string text = intrnlSunkTextArray[k];
                    if (k == 0)
                    {
                        string intrnlSunkLyrName = text.Replace(intrnlSnkanSlbLayerInputName, "");
                        string intrnlSunkLayerName = Convert.ToString(intrnlSunkLyrName);
                        InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLayerName.Add(intrnlSunkLayerName);
                    }
                    else if (k == 1)
                    {
                        string levelDiff = text.Replace(intrnlSnkanSlbLevelDiffernceInputName, "");
                        double levelDifference = Convert.ToDouble(levelDiff);
                        InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabLevelDifference.Add(levelDifference);
                    }
                    else if (k == 2)
                    {
                        string slabThk = text.Replace(intrnlSnkanSlbThicknessInputName, "");
                        double slabThick = Convert.ToDouble(slabThk);
                        InternalSunkanSlab_UI_Helper.listOfInternalSunkanSlabThick.Add(slabThick);
                    }
                }
            }

            return intrnlSunkIndex;
        }


        private int setInternalWallSlabDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            InternalWallSlab_UI_Helper.listOfInternalWallSlabLayerName.Clear();
            InternalWallSlab_UI_Helper.listOfInternalWallSlabBottom.Clear();

            int intrnlSunkIndex = index + 1;
            for (; intrnlSunkIndex < listOfReadDataFromTextFile.Count; intrnlSunkIndex++)
            {
                string intrnlSunkText = listOfReadDataFromTextFile[intrnlSunkIndex];
                string[] intrnlSunkTextArray = intrnlSunkText.Split(',');
                if (intrnlSunkTextArray.Length <= 1)
                {
                    intrnlSunkIndex = intrnlSunkIndex - 1;
                    break;
                }

                for (int k = 0; k < intrnlSunkTextArray.Length; k++)
                {
                    string text = intrnlSunkTextArray[k];
                    if (k == 0)
                    {
                        string intrnlSunkLyrName = text.Replace(intrnlSnkanSlbLayerInputName, "");
                        string intrnlSunkLayerName = Convert.ToString(intrnlSunkLyrName);
                        InternalWallSlab_UI_Helper.listOfInternalWallSlabLayerName.Add(intrnlSunkLayerName);
                    }
                    else if (k == 1)
                    {
                        string levelDiff = text.Replace(wallSlabBottomInputName, "");
                        double levelDifference = Convert.ToDouble(levelDiff);
                        InternalWallSlab_UI_Helper.listOfInternalWallSlabBottom.Add(levelDifference);
                    }
                }
            }

            return intrnlSunkIndex;
        }

        private int setExternalSunkanSlabDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLayerName.Clear();
            ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLevelDifference.Clear();
            ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabThick.Clear();

            int extrnlSunkIndex = index + 1;
            for (; extrnlSunkIndex < listOfReadDataFromTextFile.Count; extrnlSunkIndex++)
            {
                string extrnlSunkText = listOfReadDataFromTextFile[extrnlSunkIndex];
                string[] extrnlSunkTextArray = extrnlSunkText.Split(',');
                if (extrnlSunkTextArray.Length <= 1)
                {
                    extrnlSunkIndex = extrnlSunkIndex - 1;
                    break;
                }

                for (int k = 0; k < extrnlSunkTextArray.Length; k++)
                {
                    string text = extrnlSunkTextArray[k];
                    if (k == 0)
                    {
                        string extrnlSunkLyrName = text.Replace(extrnlSnkanSlbLayerInputName, "");
                        string extrnlSunkLayerName = Convert.ToString(extrnlSunkLyrName);
                        ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLayerName.Add(extrnlSunkLayerName);
                    }
                    else if (k == 1)
                    {
                        string levelDiff = text.Replace(extrnlSnkanSlbLevelDiffernceInputName, "");
                        double levelDifference = Convert.ToDouble(levelDiff);
                        ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLevelDifference.Add(levelDifference);
                    }
                    else if (k == 2)
                    {
                        string slabThk = text.Replace(extrnlSnkanSlbThicknessInputName, "");
                        double slabThick = Convert.ToDouble(slabThk);
                        ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabThick.Add(slabThick);
                    }
                }
            }
            return extrnlSunkIndex;
        }
        private int setParapetDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            Parapet_UI_Helper.listOfParapetLayerName.Clear();
            Parapet_UI_Helper.listOfParapetTop.Clear();
            Parapet_UI_Helper.listOfParapetBottom.Clear();

            int parapetDataIndex = index + 1;
            for (; parapetDataIndex < listOfReadDataFromTextFile.Count; parapetDataIndex++)
            {
                string parapetText = listOfReadDataFromTextFile[parapetDataIndex];
                string[] parapetTextArray = parapetText.Split(',');
                if (parapetTextArray.Length <= 1)
                {
                    parapetDataIndex = parapetDataIndex - 1;
                    break;
                }

                for (int k = 0; k < parapetTextArray.Length; k++)
                {
                    string text = parapetTextArray[k];
                    if (k == 0)
                    {
                        string parapetLyrName = text.Replace(parapetLayerInputName, "");
                        string parapetLayerName = Convert.ToString(parapetLyrName);
                        Parapet_UI_Helper.listOfParapetLayerName.Add(parapetLayerName);
                    }
                    else if (k == 1)
                    {
                        string parapetBttm = text.Replace(parapetBottomInputName, "");
                        string parapetBottom = Convert.ToString(parapetBttm);
                        Parapet_UI_Helper.listOfParapetBottom.Add(parapetBottom);
                    }
                    else if (k == 2)
                    {
                        string parapetTopText = text.Replace(parapetTopInputName, "");
                        double parapetTop = Convert.ToDouble(parapetTopText);
                        Parapet_UI_Helper.listOfParapetTop.Add(parapetTop);
                    }
                }
            }
            return parapetDataIndex;
        }
        private int setStaircaseDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            StairCase_UI_Helper.listOfStairCaseLayerName.Clear();

            int staircaseDataIndex = index + 1;
            for (; staircaseDataIndex < listOfReadDataFromTextFile.Count; staircaseDataIndex++)
            {
                string staircaseText = listOfReadDataFromTextFile[staircaseDataIndex];
                string[] staircaseTextArray = staircaseText.Split(',');
                if (staircaseText == liftInputName || staircaseText == internalWallSlabInputName)
                {
                    staircaseDataIndex = staircaseDataIndex - 1;
                    break;
                }

                for (int k = 0; k < staircaseTextArray.Length; k++)
                {
                    string text = staircaseTextArray[k];
                    if (k == 0)
                    {
                        string staircaseLyrName = text.Replace(staircaseLayerInputName, "");
                        string staircaseLayerName = Convert.ToString(staircaseLyrName);
                        StairCase_UI_Helper.listOfStairCaseLayerName.Add(staircaseLayerName);
                    }
                }
            }
            return staircaseDataIndex;
        }
        private int setLiftDataFromTextFile_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {
            Lift_UI_Helper.listOfLiftLayerName.Clear();

            int liftDataIndex = index + 1;
            for (; liftDataIndex < listOfReadDataFromTextFile.Count; liftDataIndex++)
            {
                string liftText = listOfReadDataFromTextFile[liftDataIndex];
                string[] liftTextArray = liftText.Split(',');
                //if (liftTextArray.Length <= 1)
                //{
                //    liftDataIndex = liftDataIndex - 1;
                //    break;
                //}
                if (liftText == internalWallSlabInputName)
                {
                    liftDataIndex = liftDataIndex - 1;
                    break;
                }

                for (int k = 0; k < liftTextArray.Length; k++)
                {
                    string text = liftTextArray[k];
                    if (k == 0)
                    {
                        string liftLyrName = text.Replace(liftcaseLayerInputName, "");
                        string liftLayerName = Convert.ToString(liftLyrName);
                        Lift_UI_Helper.listOfLiftLayerName.Add(liftLayerName);
                    }
                }
            }
            return liftDataIndex;
        }

        private int centerline_details_To_UI(int index, List<string> listOfReadDataFromTextFile)
        {

            int liftDataIndex = index + 1;

            string liftText = listOfReadDataFromTextFile[liftDataIndex];
            string[] liftTextArray = liftText.Split(',');


            PanelLayout_UI.f_dist = Convert.ToDouble(liftTextArray[0]);
            PanelLayout_UI.sec_dist = Convert.ToDouble(liftTextArray[1]);
            PanelLayout_UI.ver_max_pitch = Convert.ToDouble(liftTextArray[2]);
            PanelLayout_UI.ver_min_pitch = Convert.ToDouble(liftTextArray[3]);

            return liftDataIndex;
        }
    }
}
