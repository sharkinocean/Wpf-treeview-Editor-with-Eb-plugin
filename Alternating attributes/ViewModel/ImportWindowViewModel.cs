using Alternating_attributes.Model;
using Aucotec.EngineeringBase.Client.Runtime;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Alternating_attributes.ViewModel
{
    public class ImportWindowViewModel : Helpers.VMBase
    {

        private Application myApplication;
        public ICommand CmdImport { get; set; }
        public ICommand CmdConfirm { get; set; }
        public ICommand cmdSearch { get; set; }
        public List<ObjectitemTabTree> TabTree { get; }

        private List<ObjectitemToDragToTree> _toDragTree;
        public List<ObjectitemToDragToTree> ToDragTree
        {
            get
            {
                return _toDragTree;
            }
            set
            {
                _toDragTree = value;
                OnPropertyChanged(nameof(ToDragTree));
            }
        }
        public List<ObjectitemToDragToTree> ToDragTreeCache { get; set; }

        private string _searchValue;
        public string SearchValue
        {
            get
            {
                //if(String.IsNullOrEmpty)
                //OnPropertyChanged(nameof(TabTreeName));
                return _searchValue;
            }
            set
            {
                _searchValue = value;
                OnPropertyChanged(nameof(SearchValue));
            }
        }
        public ImportWindowViewModel(Application myApplication, List<ObjectitemTabTree> Tablist, List<ObjectitemToDragToTree> AttributeList )
        {
            CmdImport = new Helpers.RelayCommand(exeImport);
            CmdConfirm = new Helpers.RelayCommand(exeAddNewAtt);
            cmdSearch = new Helpers.RelayCommand(exeSearch);
            this.myApplication = myApplication;
            ToDragTree = AttributeList;
            TabTree = Tablist;
        }

        private void exeSearch(object obj)
        {
            if (ToDragTreeCache == null && ToDragTree != null) {
                ToDragTreeCache = ToDragTree;
            }
            if (!String.IsNullOrEmpty(SearchValue))
            {
                if (ToDragTreeCache != null)
                 ToDragTree = ToDragTreeCache;
                 //ToDragTree.ForEach(x => x.ToDragTreeChildren.RemoveAll(c => !c.ToDragTreeName.Contains(SearchValue)));
                 ToDragTree = ToDragTree.Where(c => (c.ObjectKind != ObjectKind.FolderForUserAttributes)? c.ToDragTreeName.Contains(SearchValue) : true).ToList();
            }
            else
            {
                ToDragTree = ToDragTreeCache;
            }
           
        }

        private void exeImport(object obj)
        {
            ReadExcel();
        }

        private void exeAddNewAtt(object obj)
        {
            var selected = GetAllSelectedChildrens(TabTree);

            foreach (var types in selected)
            {
                AttAndRemoveAttributeForExcel(types.TabTreeChildren, types.TabTreeName);
            }

        }

        private void AttAndRemoveAttributeForExcel(List<ObjectitemTabTree> Items, string TypeName)
        {
            FilterExpression fe = myApplication.CreateFilter();
            fe.Add(AttributeId.Designation, BinaryOperator.Equal, TypeName);
            var oc = myApplication.Folders.TypeDefinitions.FindObjects(fe, SearchBehavior.Hierarchical, true).FirstOrDefault();

            foreach (var Tab in Items)
            {
                if (!Tab.TabTreeName.Contains("System Attributes"))
                {
                    foreach (var att in Tab.TabTreeChildren)
                    {
                        int ssid = int.Parse(att.AddedId);

                        try
                        {
                            oc.Attributes.AddAttributeToMask((AttributeId)ssid, Tab.TabTreeName);
                        }
                        catch
                        {
                            System.Windows.MessageBox.Show(" Atribute " + ssid + " Already Exist in " + Tab.TabTreeName);
                        }
                    }
                }
                else { System.Windows.MessageBox.Show(" Cant Add attributes to " + Tab.TabTreeName + " yet !!");}
            }

        }

    
    private List<ObjectitemTabTree> GetAllSelectedChildrens(List<ObjectitemTabTree> Tree)
        {
            var selectedData = Tree.SelectMany(x => x.TabTreeChildren).ToList();

            selectedData.ForEach(u => { u.TabTreeChildren.ForEach(k => k.TabTreeChildren.RemoveAll(j => String.IsNullOrEmpty(j.AddedId))); });
            selectedData.ForEach(u => { u.TabTreeChildren.RemoveAll(k => k.TabTreeChildren.Count == 0); });

            return selectedData;
        }

        private void ReadExcel()
        {

            var _selectedFilePath = openSelectionDialog();
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(_selectedFilePath, true))
            {
                SpreadSheetHelper SpreadSheetHelper = new SpreadSheetHelper();
                SpreadSheetHelper = GetAllWorksheets(spreadsheetDocument);

                int validationCount = 0;
                if (SpreadSheetHelper != null && SpreadSheetHelper.MultipleSheet != null) {
                    foreach (var obj in SpreadSheetHelper.MultipleSheet)
                    {
                        SpreadSheetHelper = GetOneSheet(SpreadSheetHelper, spreadsheetDocument, obj as DocumentFormat.OpenXml.Spreadsheet.Sheet);
                        string TypeName = SpreadSheetHelper.SheetName.Split('>').Last();

                        if (!String.IsNullOrEmpty(TypeName))
                        {
                           
                            SpreadSheetHelper = GetRowsAndStringTableFromSheet(SpreadSheetHelper);
                            if (SpreadSheetHelper.Rows != null)
                            {
                                var tabs = MapSheetAsObjTypeDefination(SpreadSheetHelper);
                                var attWithFunctions = GetAttToAddAndRemove(tabs);
                         
                            if (attWithFunctions.Count > 0)
                            {
                                    AttAndRemoveAttributeForExcel(attWithFunctions, TypeName);

                                validationCount++;
                            }
                            }

                        }
                        
                    }
                }
                if (validationCount == 0)
                {
                    System.Windows.MessageBox.Show("Nothing to update or Wrong file or format." +
                        " /n Check your Sheet name and make sure to include ID in the right TAB." +
                        " /n Also Dont forget to include the ADD OR REMOVE word in column C");
                }
                else
                    System.Windows.MessageBox.Show("Successfully updated data");
            }

            }

        private string openSelectionDialog()
        {
            var _filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
                _filePath = openFileDialog.FileName;

            return _filePath;

        }

        private List<MaskDescriptionModel> GetAttToAddAndRemove(List<MaskDescriptionModel> tabs)
        {
            List<MaskDescriptionModel> attToAdd = new List<MaskDescriptionModel>();
            if (tabs != null && tabs.Count > 0)
            {
                 attToAdd = tabs.Where(x => x.AttibuteNameAndID.Any(s => s.Function != null)).ToList();
                if (attToAdd != null && attToAdd.Count > 0)
                {
                    attToAdd.ForEach(u => { u.AttibuteNameAndID.RemoveAll(a => a.Function == null); });
                    
                }
            }

            return attToAdd;

        }

        private void AttAndRemoveAttributeForExcel(List<MaskDescriptionModel> data, string TypeName)
        {
            FilterExpression fe = myApplication.CreateFilter();
            fe.Add(AttributeId.Designation, BinaryOperator.Equal, TypeName);
            var oc = myApplication.Folders.TypeDefinitions.FindObjects(fe, SearchBehavior.Hierarchical, true).FirstOrDefault();
           
            foreach (var obj in data)
            {
                foreach (var item in obj.AttibuteNameAndID) {
                    int ssid = int.Parse(item.AttributesIdString);

                    if (String.Equals(item.Function, "ADD", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            oc.Attributes.AddAttributeToMask((AttributeId)ssid, obj.TabNameString);
                        }
                        catch
                        {
                            System.Windows.MessageBox.Show(" Atribute " + ssid + " Already Exist in " + obj.TabNameString);
                        }
                    }
                    else if (String.Equals(item.Function, "REMOVE", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                             oc.Attributes.Remove((AttributeId)ssid);
                        }
                        catch(Exception ex)
                        {
                            //System.Windows.MessageBox.Show(" Atribute " + ssid + " Does Not Exist To Remove " + obj.TabNameString);
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }
                    else { System.Windows.MessageBox.Show(" Nothing to Remove Or Add Please Check If ADD OR REMOVE Word Has Been Added To Coloum C "); }
                }

            }

        }

        private List<MaskDescriptionModel> MapSheetAsObjTypeDefination(SpreadSheetHelper spreadSheetHelper)
        {
            int rowIndex = 1;
            List<MaskDescriptionModel> TotalTabAndAtt = new List<MaskDescriptionModel>();
            MaskDescriptionModel TabsAndAtt = new MaskDescriptionModel();
            List<ExcelAttributeMapping> totalAtt = new List<ExcelAttributeMapping>();
            foreach (Row row in spreadSheetHelper.Rows)
            {
                ExcelAttributeMapping AttNameID = new ExcelAttributeMapping();

                var RowA = row.Descendants<Cell>().Where(x => x.CellReference == "A" + rowIndex).FirstOrDefault();
                var RowB = row.Descendants<Cell>().Where(x => x.CellReference == "B" + rowIndex).FirstOrDefault();
                var RowC = row.Descendants<Cell>().Where(x => x.CellReference == "C" + rowIndex).FirstOrDefault();

                var ValueA = CellValueFromCell(RowA, spreadSheetHelper);
                var ValueB = CellValueFromCell(RowB, spreadSheetHelper);
                var ValueC = CellValueFromCell(RowC, spreadSheetHelper);

                if (String.IsNullOrEmpty(ValueB) && RowB != null)
                    ValueB = RowB.CellValue.InnerText;
                if (String.IsNullOrEmpty(ValueC) && RowC != null)
                    ValueC = RowC.CellValue.InnerText;

                if (ValueB != null && ValueB.Equals("Tab Name"))
                {
                    if (rowIndex > 2)
                    {
                        TabsAndAtt.AttibuteNameAndID = totalAtt;
                        TotalTabAndAtt.Add(TabsAndAtt);
                        TabsAndAtt = new MaskDescriptionModel();
                        totalAtt = new List<ExcelAttributeMapping>();
                    }

                    if (ValueB != null)
                        TabsAndAtt.TabNameString = ValueA;
                    
                }
                else if (ValueB != null && !ValueB.Equals("Tab Name") && !ValueB.Equals("AttributeID"))
                {
                    AttNameID.AttributesNameString = ValueA;
                    AttNameID.AttributesIdString = ValueB;
                    if (!String.IsNullOrEmpty(ValueC))
                        AttNameID.Function = ValueC;
                }
                if (AttNameID.AttributesIdString != null )
                    totalAtt.Add(AttNameID);

                rowIndex++;
            }

            if(TabsAndAtt != null && TotalTabAndAtt != null && totalAtt !=null)
            TabsAndAtt.AttibuteNameAndID = totalAtt;
            TotalTabAndAtt.Add(TabsAndAtt);

            return TotalTabAndAtt;
            
        }

        private string CellValueFromCell(Cell cell, SpreadSheetHelper spreadSheetHelper)
        {
            string cellValue = "";
            if (cell != null && (cell.DataType != null) && (cell.DataType == CellValues.SharedString))
            {
                int ssid = int.Parse(cell.CellValue.Text);
                cellValue = spreadSheetHelper.sharedStringTable.ChildElements[ssid].InnerText;
            }
            return cellValue;
        }

        private SpreadSheetHelper GetOneSheet(SpreadSheetHelper helper, SpreadsheetDocument document, DocumentFormat.OpenXml.Spreadsheet.Sheet obj)
        {
            var sheetname =  obj.Name;
            IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Sheet> Sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Where(s => s.Name == sheetname);
            helper.SheetId = Sheets.First().Id.Value;
            helper.workbookPart = document.WorkbookPart;

            helper.WorkSheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(helper.SheetId);
            helper.workSheet = helper.WorkSheetPart.Worksheet;

            helper.SheetName = sheetname.ToString();
            
            return helper;
        }

        private SpreadSheetHelper GetRowsAndStringTableFromSheet(SpreadSheetHelper spreadSheet)
        {
            try
            {
                spreadSheet.SharedStringTablePart = spreadSheet.workbookPart.GetPartsOfType<SharedStringTablePart>().First();
           
            spreadSheet.sharedStringTable = spreadSheet.SharedStringTablePart.SharedStringTable;
            //List<Cell> cells = sheetData.workSheet.Descendants<Cell>().ToList();
            spreadSheet.Rows = spreadSheet.workSheet.Descendants<Row>().ToList();
            }
            catch
            {
                System.Windows.MessageBox.Show("No Changes Or Invalid File");
            }
            
            return spreadSheet;

        }

        public static SpreadSheetHelper GetAllWorksheets(SpreadsheetDocument Document)
        {
                SpreadSheetHelper theSheets = new SpreadSheetHelper();
            
                WorkbookPart wbPart = Document.WorkbookPart;
                theSheets.MultipleSheet = wbPart.Workbook.Sheets;
            
            return theSheets;
        }

    }
}
