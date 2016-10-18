﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NXOpen;
using CaxGlobaltek;
using System.Text.RegularExpressions;

namespace TEUpload
{
    public class PartExcelNC
    {
        public static bool GetPartData(Part displayPart, out TEUploadDlg.PartInfo sPartInfo)
        {
            sPartInfo = new TEUploadDlg.PartInfo();
            try
            {
                string[] SplitPath = displayPart.FullPath.Split('\\');
                PartInfo.CusName = SplitPath[3];
                PartInfo.PartNo = SplitPath[4];
                PartInfo.CusRev = SplitPath[5];
                PartInfo.OpNum = Regex.Replace(Path.GetFileNameWithoutExtension(displayPart.FullPath).Split('_')[1], "[^0-9]", "");
            }
            catch (System.Exception ex)
            {
                return false;
            }
            return true;
        }
        public static bool PartFilePath(Part displayPart, out Dictionary<string, TEUpload.TEUploadDlg.PartDirData> DicPartDirData)
        {
            DicPartDirData = new Dictionary<string, TEUpload.TEUploadDlg.PartDirData>();
            try
            {
                //總組立
                TEUpload.TEUploadDlg.PartDirData sPartDirData = new TEUpload.TEUploadDlg.PartDirData();
                sPartDirData.PartLocalDir = displayPart.FullPath;
                sPartDirData.PartServer1Dir = string.Format(@"{0}\{1}", TEUploadDlg.cMETE_Download_Upload_Path.Server_ShareStr, Path.GetFileName(displayPart.FullPath));
                DicPartDirData.Add(Path.GetFileNameWithoutExtension(displayPart.FullPath), sPartDirData);
                TEUploadDlg.listView1.Items.Add(Path.GetFileName(displayPart.FullPath));

                //子Comp
                NXOpen.Assemblies.ComponentAssembly casm = displayPart.ComponentAssembly;
                List<NXOpen.Assemblies.Component> ListChildrenComp = new List<NXOpen.Assemblies.Component>();
                CaxAsm.GetCompChildren(casm.RootComponent, ref ListChildrenComp);
                foreach (NXOpen.Assemblies.Component i in ListChildrenComp)
                {
                    sPartDirData = new TEUpload.TEUploadDlg.PartDirData();
                    //判斷Server是否已存在此檔案，如果有就不上傳
                    string ServerPartPath = string.Format(@"{0}\{1}", TEUploadDlg.cMETE_Download_Upload_Path.Server_ShareStr, Path.GetFileName(((Part)i.Prototype).FullPath));
                    if (File.Exists(ServerPartPath))
                    {
                        if (!((Part)i.Prototype).FullPath.Contains(PartInfo.OpNum))
                        {
                            continue;
                        }
                    }
                    sPartDirData.PartLocalDir = ((Part)i.Prototype).FullPath;
                    sPartDirData.PartServer1Dir = ServerPartPath;
                    DicPartDirData.Add(i.Name, sPartDirData);
                    TEUploadDlg.listView1.Items.Add(Path.GetFileName(((Part)i.Prototype).FullPath));
                    //sPartDirData.PartServer2Dir = string.Format(@"{0}\{1}", cMETE_Download_Upload_Path.Server_ShareStr, "OP" + PartInfo.OIS);
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
