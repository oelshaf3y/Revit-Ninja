using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Revit_Ninja
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ParamFromExcel : IExternalCommand
    {
        public UIDocument uidoc { get; set; }
        public Document doc { get; set; }
        public UIApplication uiapp { get; set; }
        public Application application { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            application = uiapp.Application;



            #region get parameters from excel

            Excel.Application xlap = new Excel.Application();
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            string excelFile = dialog.FileName;
            Excel.Workbook wb = xlap.Workbooks.Open(excelFile);
            Excel.Worksheet parSheet = wb.Worksheets[1];
            Excel.Range range = parSheet.UsedRange;
            List<string> mergedRanges = new List<string>();
            foreach (Excel.Range cell in range)
            {
                if (cell.MergeCells)
                {
                    Excel.Range merged = cell.MergeArea;
                    mergedRanges.Add(merged.Address);
                }
            }
            //6789

            mergedRanges = mergedRanges.Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            foreach (string r in mergedRanges) { sb.AppendLine(r); }
            List<SharedParameter> sharedParameters = new List<SharedParameter>();
            for (int i = 6; i < 10; i++)
            {
                try
                {

                    string ran = mergedRanges[i].Replace("$", "");
                    int start = Int32.Parse(ran.Split(':')[0].Replace("A", "").ToString());
                    int end = Int32.Parse(ran.Split(':')[1].Replace("A", "").ToString());
                    for (int j = start; j <= end; j++)
                    {
                        string parName = ((Excel.Range)parSheet.Cells[j, 2]).Value;
                        string parGroup = ((Excel.Range)parSheet.Cells[start, 1]).Value;
                        string parApplicable = ((Excel.Range)parSheet.Cells[j, 3]).Value;
                        sharedParameters.Add(new SharedParameter(parName, parGroup, parApplicable));
                    }
                }
                catch (Exception ex)
                {
                    doc.print(ex.StackTrace);
                }
            }
            parSheet = wb.Worksheets[3];
            List<IFCEntity> entities = new List<IFCEntity>();
            List<string> Fields = new List<string>();
            string EntName = "";
            for (int row = 2; row < parSheet.Rows.Count; row++)
            {
                Excel.Range entityCell = (Excel.Range)parSheet.Cells[row, 1];
                Excel.Range field = (Excel.Range)parSheet.Cells[row, 2];
                if (entityCell != null && entityCell.Value != null)
                {
                    if (Fields.Count > 0)
                    {
                        entities.Add(new IFCEntity(EntName, Fields));
                    }
                    EntName = reverseCamelCase(entityCell.Value.ToString().Replace("Ifc", ""));
                    Fields = new List<string>();
                }
                else if (field != null && field.Value != null)
                {
                    Fields.Add(field.Value.ToString().Replace("Ifc", ""));
                }
                else
                {
                    if (Fields.Count > 0)
                    {
                        entities.Add(new IFCEntity(EntName, Fields));
                    }
                    break;
                }
            }


            List<IFCEntity> Dynamics = new List<IFCEntity>();
            List<IFCEntity> Maintainables = new List<IFCEntity>();
            List<IFCEntity> OpSegnificants = new List<IFCEntity>();
            List<IFCEntity> nOpSegnificants = new List<IFCEntity>();
            parSheet = wb.Worksheets[4];
            for (int row = 2; row < 61; row++)
            {
                try
                {

                    Excel.Range dyn = (((Excel.Range)parSheet.Cells[row, 1]));
                    if (dyn.Value != null && dyn.Value.ToString().Length > 0)
                    {
                        string s = dyn.Value.ToString().Replace(" ", "").Replace("Ifc", "");
                        s = reverseCamelCase(s);
                        IFCEntity dynEnt = entities.Where(x => x.name == s).FirstOrDefault();
                        if (dynEnt != null)
                            Dynamics.Add(dynEnt);
                    }
                    Excel.Range maintainable = (((Excel.Range)parSheet.Cells[row, 2]));
                    if (maintainable.Value != null && maintainable.Value.ToString().Length > 0)
                    {
                        string ss = maintainable.Value.ToString().Replace(" ", "").Replace("Ifc", "");
                        ss = reverseCamelCase(ss);
                        IFCEntity maintainableEnt = entities.Where(x => x.name == ss).FirstOrDefault();
                        if (maintainableEnt != null)
                            Maintainables.Add(maintainableEnt);
                    }

                    Excel.Range opSeg = (((Excel.Range)parSheet.Cells[row, 3]));
                    if (opSeg.Value != null && opSeg.Value.ToString().Length > 0)
                    {
                        string sss = opSeg.Value.ToString().Replace(" ", "").Replace("Ifc", "");
                        sss = reverseCamelCase(sss);
                        IFCEntity opSegEnt = entities.Where(x => x.name == sss).FirstOrDefault();
                        if (opSegEnt != null)
                            OpSegnificants.Add(opSegEnt);
                    }
                    Excel.Range nOpSeg = (((Excel.Range)parSheet.Cells[row, 4]));
                    if (nOpSeg.Value != null && nOpSeg.Value.ToString().Length > 0)
                    {
                        string ssss = nOpSeg.Value.ToString().Replace(" ", "").Replace("Ifc", "");
                        ssss = reverseCamelCase(ssss);
                        IFCEntity nOpSegEnt = entities.Where(x => x.name == ssss).FirstOrDefault();
                        if (nOpSegEnt != null)
                            nOpSegnificants.Add(nOpSegEnt);
                    }
                }
                catch (Exception e)
                {
                    sb.AppendLine(e.Message);

                }
            }

            DB db = new DB(sharedParameters, Dynamics, Maintainables, OpSegnificants, nOpSegnificants);
            string jstring = JsonSerializer.Serialize(db);
            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "sp.json");
            File.WriteAllText(savePath, jstring);
            wb.Close(false);
            Marshal.ReleaseComObject(wb);
            xlap.Quit();
            Marshal.ReleaseComObject(xlap);
            doc.print(sb.ToString());
            #endregion

            return Result.Succeeded;
        }

        private string reverseCamelCase(string s)
        {
            //return Regex.Replace(s, "(?<!^)([A-Z])", " s");
            return Regex.Replace(s, "(?<=[a-z])([A-Z])", " $1");
        }

    }

    internal class SharedParameter
    {
        public string name { get; set; }
        public string parameterGroup { get; set; }
        public string applicableTo { get; set; }
        [JsonConstructor]
        public SharedParameter(string name, string group, string applicable)
        {
            this.name = name;
            parameterGroup = group;
            applicableTo = applicable;
        }
    }
    internal class IFCEntity
    {
        public string name { get; set; }
        public List<string> fields { get; set; }
        [JsonConstructor]
        public IFCEntity(string name, List<string> fields)
        {
            this.name = name;
            this.fields = fields;
        }
    }
    internal class DB
    {
        public List<SharedParameter> sharedParameters { get; set; }
        public List<IFCEntity> Dynamics { get; set; }
        public List<IFCEntity> Maintainables { get; set; }
        public List<IFCEntity> OpSegnificants { get; set; }
        public List<IFCEntity> nOpSegnificants { get; set; }
        [JsonConstructor]
        public DB(List<SharedParameter> sharedParameters, List<IFCEntity> Dynamics, List<IFCEntity> Maintainables,
            List<IFCEntity> OpSegnificants, List<IFCEntity> nOpSegnificants)
        {
            this.sharedParameters = sharedParameters;
            this.Dynamics = Dynamics;
            this.Maintainables = Maintainables;
            this.OpSegnificants = OpSegnificants;
            this.nOpSegnificants = nOpSegnificants;
        }
    }
}
