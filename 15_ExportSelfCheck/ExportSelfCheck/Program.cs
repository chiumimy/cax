using System;
using NXOpen;
using NXOpen.UF;
using ExportSelfCheck;
using System.Collections.Generic;
using NXOpen.Utilities;
using CaxGlobaltek;
using NXOpen.Annotations;
using System.IO;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;

public class Program
{
    // class members
    private static Session theSession;
    private static UI theUI;
    private static UFSession theUfSession;
    public static Program theProgram;
    public static bool isDisposeCalled;

    //------------------------------------------------------------------------------
    // Constructor
    //------------------------------------------------------------------------------
    public Program()
    {
        try
        {
            theSession = Session.GetSession();
            theUI = UI.GetUI();
            theUfSession = UFSession.GetUFSession();
            isDisposeCalled = false;
        }
        catch (NXOpen.NXException ex)
        {
            // ---- Enter your exception handling code here -----
            // UI.GetUI().NXMessageBox.Show("Message", NXMessageBox.DialogType.Error, ex.Message);
        }
    }

    //------------------------------------------------------------------------------
    //  Explicit Activation
    //      This entry point is used to activate the application explicitly
    //------------------------------------------------------------------------------
    public static int Main(string[] args)
    {
        int retValue = 0;
        try
        {
            theProgram = new Program();
            Part workPart = theSession.Parts.Work;
            Part displayPart = theSession.Parts.Display;

            bool status;

            DefineVariable.Is_Local = Environment.GetEnvironmentVariable("UGII_ENV_FILE");
            if (DefineVariable.Is_Local != "")
            {
                //取得本機ShopDoc.xls路徑
                DefineVariable.SelfCheckPath = string.Format(@"{0}\{1}\{2}", Path.GetDirectoryName(displayPart.FullPath), "MODEL", "SelfCheck.xls");
            }

            //取得料號
            string PartNo = Path.GetFileNameWithoutExtension(displayPart.FullPath);

            //取得日期
            string CurrentDate = DateTime.Now.ToShortDateString();

            int SheetCount = 0;
            NXOpen.Tag[] SheetTagAry = null;
            theUfSession.Draw.AskDrawings(out SheetCount, out SheetTagAry);

            DefineVariable.DicDimenData = new Dictionary<string, TextData>();
            for (int i = 0; i < SheetCount; i++)
            {
                //打開Sheet並記錄所有OBJ
                NXOpen.Drawings.DrawingSheet CurrentSheet = (NXOpen.Drawings.DrawingSheet)NXObjectManager.Get(SheetTagAry[i]);
                CurrentSheet.Open();
                if (i == 0)
                {
                    //記錄第一張Sheet
                    DefineVariable.FirstDrawingSheet = CurrentSheet;
                }
                DisplayableObject[] SheetObj = CurrentSheet.View.AskVisibleObjects();
                foreach (DisplayableObject singleObj in SheetObj)
                {
                    TextData cTextData = new TextData();
                    string singleObjType = singleObj.GetType().ToString();
                    string SelfCheck_Gauge = "", BallonNum = "", Frequency = "", Location = "";
                    string[] mainText;
                    string[] dualText;

                    #region 取SelfCheck共用屬性(泡泡值、檢具名稱、檢驗頻率、泡泡所在區域)，如果都沒屬性就找下一個
                    try
                    {
                        SelfCheck_Gauge = singleObj.GetStringAttribute(CaxME.DimenAttr.IPQC_Gauge);
                        BallonNum = singleObj.GetStringAttribute(CaxME.DimenAttr.BallonNum);
                        Frequency = singleObj.GetStringAttribute(CaxME.DimenAttr.IPQC_Freq);
                        Location = singleObj.GetStringAttribute(CaxME.DimenAttr.BallonLocation);
                    }
                    catch (System.Exception ex)
                    {
                        SelfCheck_Gauge = "";
                    }
                    if (SelfCheck_Gauge == "")
                    {
                        continue;
                    }
                    #endregion

                    #region 紀錄共用屬性(泡泡值、檢具名稱、檢驗頻率、泡泡所在區域)

                    //取得泡泡值
                    cTextData.BallonNum = BallonNum;

                    //取得檢具名稱
                    cTextData.Gauge = SelfCheck_Gauge;

                    //取得檢驗頻率
                    cTextData.Frequency = Frequency;

                    //取得泡泡所在區域
                    cTextData.Location = Location;

                    #endregion

                    if (singleObjType == "NXOpen.Annotations.VerticalDimension")
                    {
                        #region VerticalDimension取Text
                        cTextData.Type = "NXOpen.Annotations.VerticalDimension";
                        NXOpen.Annotations.VerticalDimension temp = (NXOpen.Annotations.VerticalDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.PerpendicularDimension")
                    {
                        #region PerpendicularDimension取Text
                        cTextData.Type = "NXOpen.Annotations.PerpendicularDimension";
                        NXOpen.Annotations.PerpendicularDimension temp = (NXOpen.Annotations.PerpendicularDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.MinorAngularDimension")
                    {
                        #region MinorAngularDimension取Text
                        cTextData.Type = "NXOpen.Annotations.MinorAngularDimension";
                        NXOpen.Annotations.MinorAngularDimension temp = (NXOpen.Annotations.MinorAngularDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.RadiusDimension")
                    {
                        #region MinorAngularDimension取Text
                        cTextData.Type = "NXOpen.Annotations.RadiusDimension";
                        NXOpen.Annotations.RadiusDimension temp = (NXOpen.Annotations.RadiusDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.HorizontalDimension")
                    {
                        #region HorizontalDimension取Text
                        cTextData.Type = "NXOpen.Annotations.HorizontalDimension";
                        NXOpen.Annotations.HorizontalDimension temp = (NXOpen.Annotations.HorizontalDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.IdSymbol")
                    {
                        #region IdSymbol取Text
                        cTextData.Type = "NXOpen.Annotations.IdSymbol";
                        NXOpen.Annotations.IdSymbol temp = (NXOpen.Annotations.IdSymbol)singleObj;

                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.Note")
                    {
                        #region Note取Text
                        cTextData.Type = "NXOpen.Annotations.Note";
                        NXOpen.Annotations.Note temp = (NXOpen.Annotations.Note)singleObj;

                        cTextData.MainText = temp.GetText()[0];
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.DraftingFcf")
                    {
                        #region DraftingFcf取Text
                        NXOpen.Annotations.DraftingFcf temp = (NXOpen.Annotations.DraftingFcf)singleObj;
                        CaxME.FcfData sFcfData = new CaxME.FcfData();
                        CaxME.GetFcfData(temp, out sFcfData);
                        cTextData.Type = "NXOpen.Annotations.DraftingFcf";
                        cTextData.Characteristic = sFcfData.Characteristic;
                        cTextData.ZoneShape = sFcfData.ZoneShape;
                        cTextData.ToleranceValue = sFcfData.ToleranceValue;
                        cTextData.MaterialModifier = sFcfData.MaterialModifier;
                        cTextData.PrimaryDatum = sFcfData.PrimaryDatum;
                        cTextData.PrimaryMaterialModifier = sFcfData.PrimaryMaterialModifier;
                        cTextData.SecondaryDatum = sFcfData.SecondaryDatum;
                        cTextData.SecondaryMaterialModifier = sFcfData.SecondaryMaterialModifier;
                        cTextData.TertiaryDatum = sFcfData.TertiaryDatum;
                        cTextData.TertiaryMaterialModifier = sFcfData.TertiaryMaterialModifier;
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.Label")
                    {
                        #region Label取Text
                        cTextData.Type = "NXOpen.Annotations.Label";
                        NXOpen.Annotations.Label temp = (NXOpen.Annotations.Label)singleObj;
                        cTextData.MainText = temp.GetText()[0];
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.DraftingDatum")
                    {
                        #region DraftingDatum取Text
                        cTextData.Type = "NXOpen.Annotations.DraftingDatum";
                        NXOpen.Annotations.DraftingDatum temp = (NXOpen.Annotations.DraftingDatum)singleObj;
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.DiameterDimension")
                    {
                        #region DiameterDimension取Text
                        cTextData.Type = "NXOpen.Annotations.DiameterDimension";
                        NXOpen.Annotations.DiameterDimension temp = (NXOpen.Annotations.DiameterDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.AngularDimension")
                    {
                        #region AngularDimension取Text
                        cTextData.Type = "NXOpen.Annotations.AngularDimension";
                        NXOpen.Annotations.AngularDimension temp = (NXOpen.Annotations.AngularDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.CylindricalDimension")
                    {
                        #region CylindricalDimension取Text
                        cTextData.Type = "NXOpen.Annotations.CylindricalDimension";
                        NXOpen.Annotations.CylindricalDimension temp = (NXOpen.Annotations.CylindricalDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0];
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralTwoLines")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = temp.LowerMetricToleranceValue.ToString();
                        }
                        if (temp.ToleranceType.ToString() == "UnilateralAbove")
                        {
                            cTextData.TolType = "UnilateralAbove";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = "0";
                        }
                        if (temp.ToleranceType.ToString() == "UnilateralBelow")
                        {
                            cTextData.TolType = "UnilateralBelow";
                            cTextData.UpperTol = "0";
                            cTextData.LowerTol = temp.LowerMetricToleranceValue.ToString();
                        }
                        #endregion
                    }
                    else if (singleObjType == "NXOpen.Annotations.ChamferDimension")
                    {
                        #region ChamferDimension取Text
                        cTextData.Type = "NXOpen.Annotations.ChamferDimension";
                        NXOpen.Annotations.ChamferDimension temp = (NXOpen.Annotations.ChamferDimension)singleObj;

                        temp.GetDimensionText(out mainText, out dualText);

                        if (mainText.Length > 0)
                        {
                            cTextData.MainText = mainText[0] + "X" + "45" + "<$s>";
                        }
                        if (temp.GetAppendedText().GetBeforeText().Length > 0)
                        {
                            cTextData.BeforeText = temp.GetAppendedText().GetBeforeText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAfterText().Length > 0)
                        {
                            cTextData.AfterText = temp.GetAppendedText().GetAfterText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetAboveText().Length > 0)
                        {
                            cTextData.AboveText = temp.GetAppendedText().GetAboveText()[0].ToString();
                        }
                        if (temp.GetAppendedText().GetBelowText().Length > 0)
                        {
                            cTextData.BelowText = temp.GetAppendedText().GetBelowText()[0].ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralOneLine")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = (-1 * temp.UpperMetricToleranceValue).ToString();
                        }
                        if (temp.ToleranceType.ToString() == "BilateralTwoLines")
                        {
                            cTextData.TolType = "BilateralOneLine";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = temp.LowerMetricToleranceValue.ToString();
                        }
                        if (temp.ToleranceType.ToString() == "UnilateralAbove")
                        {
                            cTextData.TolType = "UnilateralAbove";
                            cTextData.UpperTol = temp.UpperMetricToleranceValue.ToString();
                            cTextData.LowerTol = "0";
                        }
                        if (temp.ToleranceType.ToString() == "UnilateralBelow")
                        {
                            cTextData.TolType = "UnilateralBelow";
                            cTextData.UpperTol = "0";
                            cTextData.LowerTol = temp.LowerMetricToleranceValue.ToString();
                        }
                        #endregion
                    }
                    //計算泡泡總數
                    DefineVariable.BallonCount++;

                    DefineVariable.DicDimenData[BallonNum] = cTextData;
                }
            }

            //設定輸出路徑--Local
            //string[] FolderFile = System.IO.Directory.GetFileSystemEntries(Path.GetDirectoryName(displayPart.FullPath), "*.xls");
            //string OutputPath = string.Format(@"{0}\{1}", Path.GetDirectoryName(displayPart.FullPath),
            //                                                   Path.GetFileNameWithoutExtension(displayPart.FullPath) + "_" + "SelfCheck" + "_" + (FolderFile.Length + 1) + ".xls");

            //設定輸出路徑--Server
            string OperNum = Regex.Replace(Path.GetFileNameWithoutExtension(displayPart.FullPath).Split('_')[1], "[^0-9]", "");
            string Local_Folder_OIS = string.Format(@"{0}\{1}\{2}", Path.GetDirectoryName(displayPart.FullPath), "OP" + OperNum, "OIS");
            if (!File.Exists(Local_Folder_OIS))
            {
                System.IO.Directory.CreateDirectory(Local_Folder_OIS);
            }
            string[] FolderFile = System.IO.Directory.GetFileSystemEntries(Local_Folder_OIS, "*.xls");
            List<string> ListFolderFileWithIPQC = new List<string>();
            foreach (string i in FolderFile)
            {
                if (i.Contains("SelfCheck"))
                {
                    ListFolderFileWithIPQC.Add(i);
                }
            }
            string OutputPath = string.Format(@"{0}\{1}", Local_Folder_OIS,
                   Path.GetFileNameWithoutExtension(displayPart.FullPath) + "_" + "SelfCheck" + "_" + (ListFolderFileWithIPQC.Count + 1) + ".xls");



            //檢查PC有無Excel在執行
            foreach (var item in Process.GetProcesses())
            {
                if (item.ProcessName == "EXCEL")
                {
                    CaxLog.ShowListingWindow("請先關閉所有Excel再重新執行輸出，如沒有EXCEL在執行，請開啟工作管理員關閉背景EXCEL");
                    return retValue;
                }
                else
                {
                    continue;
                }
            }

            
            Excel.ApplicationClass x = new Excel.ApplicationClass();
            Excel.Workbook book = null;
            Excel.Worksheet sheet = null;
            Excel.Range oRng = null;

            try
            {
                x.Visible = false;
                if (DefineVariable.Is_Local != "")
                {
                    if (File.Exists(DefineVariable.SelfCheckPath))
                    {
                        book = x.Workbooks.Open(DefineVariable.SelfCheckPath);
                    }
                    else
                    {
                        book = x.Workbooks.Open(@"D:\SelfCheck.xls");
                    }
                }
                else
                {
                    book = x.Workbooks.Open(@"D:\SelfCheck.xls");
                }
                sheet = (Excel.Worksheet)book.Sheets[1];

                //由檢驗總數開啟符合總數的頁數
                int needSheetNo = (DefineVariable.BallonCount / 11);
                int needSheetNo_Reserve = (DefineVariable.BallonCount % 11);
                if (needSheetNo_Reserve != 0)
                {
                    needSheetNo++;
                }
                for (int i = 1; i < needSheetNo; i++)
                {
                    sheet.Copy(System.Type.Missing, x.Workbooks[1].Worksheets[1]);
                }

                //更改每一個Sheet的名稱與頁數
                for (int i = 0; i < book.Worksheets.Count; i++)
                {
                    sheet = (Excel.Worksheet)book.Sheets[i + 1];
                    if (i == 0 && book.Worksheets.Count > 1)
                    {
                        sheet.Name = PartNo;
                        oRng = (Excel.Range)sheet.Cells[4, 5];
                        oRng.Value = oRng.Value.ToString().Replace("1/1", "1/" + (book.Worksheets.Count).ToString());
                    }
                    else
                    {
                        sheet.Name = PartNo + "(" + (i + 1) + ")";
                        oRng = (Excel.Range)sheet.Cells[4, 5];
                        string temp = (i + 1).ToString();
                        oRng.Value = oRng.Value.ToString().Replace("1/1", temp + "/" + (book.Worksheets.Count).ToString());
                    }
                }

                //填表
                int ExcelSequenceNo = -1;
                for (int i = 1; i < 1000; i++)
                {
                    ExcelSequenceNo++;

                    TextData cTextData;
                    DefineVariable.DicDimenData.TryGetValue(i.ToString(), out cTextData);
                    if (cTextData == null)
                    {
                        ExcelSequenceNo--;
                        continue;
                    }

                    RowColumn sRowColumn;
                    DefineVariable.GetExcelRowColumn(ExcelSequenceNo, out sRowColumn);
                    int currentSheet_Value = (ExcelSequenceNo / 11);
                    int currentSheet_Reserve = (ExcelSequenceNo % 11);
                    if (currentSheet_Value == 0)
                    {
                        sheet = (Excel.Worksheet)book.Sheets[1];
                    }
                    else
                    {
                        sheet = (Excel.Worksheet)book.Sheets[currentSheet_Value + 1];
                    }

                    oRng = (Excel.Range)sheet.Cells;


                    if (cTextData.Type == "NXOpen.Annotations.DraftingFcf")
                    {
                        #region DraftingFcf填資料
                        if (cTextData.Characteristic != "")
                        {
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = DefineVariable.GetCharacteristicSymbol(cTextData.Characteristic);
                            //oRng[sRowColumn.CharacteristicRow, sRowColumn.CharacteristicColumn] = DefineVariable.GetCharacteristicSymbol(cTextData.Characteristic);
                        }
                        if (cTextData.ZoneShape != "")
                        {
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + DefineVariable.GetZoneShapeSymbol(cTextData.ZoneShape);
                            //oRng[sRowColumn.ZoneShapeRow, sRowColumn.ZoneShapeColumn] = DefineVariable.GetZoneShapeSymbol(cTextData.ZoneShape);
                        }
                        oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.ToleranceValue;
                        //oRng[sRowColumn.ToleranceValueRow, sRowColumn.ToleranceValueColumn] = cTextData.ToleranceValue;
                        if (cTextData.MaterialModifier != "" & cTextData.MaterialModifier != "None")
                        {
                            string ValueStr = cTextData.MaterialModifier;
                            if (ValueStr == "LeastMaterialCondition")
                            {
                                ValueStr = "l";
                            }
                            else if (ValueStr == "MaximumMaterialCondition")
                            {
                                ValueStr = "m";
                            }
                            else if (ValueStr == "RegardlessOfFeatureSize")
                            {
                                ValueStr = "s";
                            }
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + ValueStr;
                            //oRng[sRowColumn.MaterialModifierRow, sRowColumn.MaterialModifierColumn] = ValueStr;
                        }
                        //Primary
                        oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.PrimaryDatum;
                        //oRng[sRowColumn.PrimaryDatumRow, sRowColumn.PrimaryDatumColumn] = cTextData.PrimaryDatum;
                        if (cTextData.PrimaryMaterialModifier != "" & cTextData.PrimaryMaterialModifier != "None")
                        {
                            string ValueStr = cTextData.PrimaryMaterialModifier;
                            if (ValueStr == "LeastMaterialCondition")
                            {
                                ValueStr = "l";
                            }
                            else if (ValueStr == "MaximumMaterialCondition")
                            {
                                ValueStr = "m";
                            }
                            else if (ValueStr == "RegardlessOfFeatureSize")
                            {
                                ValueStr = "s";
                            }
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + ValueStr;
                            //oRng[sRowColumn.PrimaryMaterialModifierRow, sRowColumn.PrimaryMaterialModifierColumn] = ValueStr;
                        }
                        //Secondary
                        oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.SecondaryDatum;
                        //oRng[sRowColumn.SecondaryDatumRow, sRowColumn.SecondaryDatumColumn] = cTextData.SecondaryDatum;
                        if (cTextData.SecondaryMaterialModifier != "" & cTextData.SecondaryMaterialModifier != "None")
                        {
                            string ValueStr = cTextData.SecondaryMaterialModifier;
                            if (ValueStr == "LeastMaterialCondition")
                            {
                                ValueStr = "l";
                            }
                            else if (ValueStr == "MaximumMaterialCondition")
                            {
                                ValueStr = "m";
                            }
                            else if (ValueStr == "RegardlessOfFeatureSize")
                            {
                                ValueStr = "s";
                            }
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + ValueStr;
                            //oRng[sRowColumn.SecondaryMaterialModifierRow, sRowColumn.SecondaryMaterialModifierColumn] = ValueStr;
                        }
                        //Tertiary
                        oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.TertiaryDatum;
                        //oRng[sRowColumn.TertiaryDatumRow, sRowColumn.TertiaryDatumColumn] = cTextData.TertiaryDatum;
                        if (cTextData.TertiaryMaterialModifier != "" & cTextData.TertiaryMaterialModifier != "None")
                        {
                            string ValueStr = cTextData.TertiaryMaterialModifier;
                            if (ValueStr == "LeastMaterialCondition")
                            {
                                ValueStr = "l";
                            }
                            else if (ValueStr == "MaximumMaterialCondition")
                            {
                                ValueStr = "m";
                            }
                            else if (ValueStr == "RegardlessOfFeatureSize")
                            {
                                ValueStr = "s";
                            }
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + ValueStr;
                            //oRng[sRowColumn.TertiaryMaterialModifierRow, sRowColumn.TertiaryMaterialModifierColumn] = ValueStr;
                        }
                        #endregion
                    }
                    else if (cTextData.Type == "NXOpen.Annotations.Label")
                    {
                        oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = cTextData.MainText;
                        //((Range)oRng[sRowColumn.MainTextRow, sRowColumn.MainTextColumn]).Interior.ColorIndex = 50;
                    }
                    else
                    {
                        #region Dimension填資料
                        if (cTextData.BeforeText != null)
                        {
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + DefineVariable.GetGDTWord(cTextData.BeforeText);
                            //oRng[sRowColumn.BeforeTextRow, sRowColumn.BeforeTextColumn] = DefineVariable.GetGDTWord(cTextData.BeforeText);
                        }
                        oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + DefineVariable.GetGDTWord(cTextData.MainText);
                        //oRng[sRowColumn.MainTextRow, sRowColumn.MainTextColumn] = DefineVariable.GetGDTWord(cTextData.MainText);
                        //Range ab = (Range)oRng[sRowColumn.MainTextRow, sRowColumn.MainTextColumn];
                        //ab.Interior.ColorIndex = 39;
                        if (cTextData.UpperTol != "" & cTextData.TolType == "BilateralOneLine")
                        {
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + "±";
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.UpperTol;
                            string MaxMinStr = "(" + (Convert.ToDouble(cTextData.MainText) + Convert.ToDouble(cTextData.UpperTol)).ToString() + "-" + (Convert.ToDouble(cTextData.MainText) - Convert.ToDouble(cTextData.UpperTol)).ToString() + ")";
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + MaxMinStr;
                            //oRng[sRowColumn.ToleranceSymbolRow, sRowColumn.ToleranceSymbolColumn] = "±";
                            //oRng[sRowColumn.UpperTolRow, sRowColumn.UpperTolColumn] = cTextData.UpperTol;
                        }
                        else if (cTextData.UpperTol != "" & cTextData.TolType == "BilateralTwoLines")
                        {
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + "+";
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.UpperTol;
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + "/";
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.LowerTol;
                            string MaxMinStr = "(" + (Convert.ToDouble(cTextData.MainText) + Convert.ToDouble(cTextData.UpperTol)).ToString() + "-" + (Convert.ToDouble(cTextData.MainText) + Convert.ToDouble(cTextData.LowerTol)).ToString() + ")";
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + MaxMinStr;
                        }
                        else if (cTextData.UpperTol != "" & cTextData.TolType == "UnilateralAbove")
                        {
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + "+";
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.UpperTol;

                        }
                        else if (cTextData.UpperTol != "" & cTextData.TolType == "UnilateralBelow")
                        {
                            //oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + "-";
                            oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn] = ((Excel.Range)oRng[sRowColumn.DimensionRow, sRowColumn.DimensionColumn]).Value + cTextData.LowerTol;
                        }
                        #endregion
                    }

                    #region 檢具、頻率、Max、Min、泡泡、泡泡位置、料號、日期
                    oRng[sRowColumn.GaugeRow, sRowColumn.GaugeColumn] = cTextData.Gauge;
                    oRng[sRowColumn.FrequencyRow, sRowColumn.FrequencyColumn] = cTextData.Frequency;
                    oRng[sRowColumn.BallonRow, sRowColumn.BallonColumn] = cTextData.BallonNum;
                    oRng[sRowColumn.LocationRow, sRowColumn.LocationColumn] = cTextData.Location;
                    oRng[sRowColumn.PartNoRow, sRowColumn.PartNoColumn] = PartNo;
                    oRng[sRowColumn.DateRow, sRowColumn.DateColumn] = CurrentDate;
                    #endregion
                }

                book.SaveAs(OutputPath, Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, 
                    Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                book.Close(Type.Missing, Type.Missing, Type.Missing);
                x.Quit();

                //切回第一張Sheet
                DefineVariable.FirstDrawingSheet.Open();
                UI.GetUI().NXMessageBox.Show("SelfCheck", NXMessageBox.DialogType.Information, "輸出完成");
                theProgram.Dispose();
            }
            catch (System.Exception ex)
            {
                book.SaveAs(OutputPath, Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                book.Close(Type.Missing, Type.Missing, Type.Missing);
                x.Quit();
            }
        }
        catch (NXOpen.NXException ex)
        {
            // ---- Enter your exception handling code here -----

        }
        return retValue;
    }

    //------------------------------------------------------------------------------
    // Following method disposes all the class members
    //------------------------------------------------------------------------------
    public void Dispose()
    {
        try
        {
            if (isDisposeCalled == false)
            {
                //TODO: Add your application code here 
            }
            isDisposeCalled = true;
        }
        catch (NXOpen.NXException ex)
        {
            // ---- Enter your exception handling code here -----

        }
    }

    public static int GetUnloadOption(string arg)
    {
        //Unloads the image explicitly, via an unload dialog
        //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);

        //Unloads the image immediately after execution within NX
        return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);

        //Unloads the image when the NX session terminates
        // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
    }

}

