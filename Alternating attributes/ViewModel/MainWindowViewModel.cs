using Alternating_attributes.Model;
using Alternating_attributes.View;
using Aucotec.EngineeringBase.Client.Runtime;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Alternating_attributes.ViewModel
{
    public class MainWindowViewModel : Helpers.VMBase
    {
        public List<ObjectItemTypeDefinationModel> Tree { get; }
        public ObjectItem SelFun;
        public ObjectItem _SelEquip;
        private string _sheetname;
        private Aucotec.EngineeringBase.Client.Runtime.Application myApplication;

        public ICommand CmdImportPage { get; set; }

        public MainWindowViewModel(List<ObjectItemTypeDefinationModel> tree, Application myApplication)
        {
            Tree = tree;
            CmdAdd = new Helpers.RelayCommand(exeAdd);
            CmdImportPage = new Helpers.RelayCommand(PushImportPage);
            this.myApplication = myApplication;
        }

        public ICommand CmdAdd { get; set; }
        public object source { get; private set; }

        private void exeAdd(object obj)
        {
            #region integer
            //to calculate integers
            //int amount = 0;
            //foreach (var parent in Tree)
            //    amount += CountChecks(parent);
            //MessageBox.Show(amount.ToString());
            #endregion

      
                    List<ObjectItemTypeDefinationModel> selectedData = GetAllSelectedChildrens(Tree);
                    WriteDataToExcel(selectedData);
                    //_SelEquip.TargetAssociations.Add(AssociationRole.DeviceToFunction, SelFun);
               
        }

        private void PushImportPage(object obj)
        {
            List<ObjectItemTypeDefinationModel> selectedData = GetAllSelectedChildrens(Tree);
            MaskDescriptionModel mappedDesc = new MaskDescriptionModel();
            List<ObjectitemTabTree> Tabmodel = new List<ObjectitemTabTree>();
            List<ObjectitemToDragToTree> attModel = new List<ObjectitemToDragToTree>();
            var definations = GetAllAttribute();
            definations.ForEach(x => { if (x.Name != "Project System Templates") { attModel.Add(new ObjectitemToDragToTree(x)); } });
            //attModel.Add(new ObjectitemToDragToTree(definations));
            foreach (var item in selectedData)
            {
                mappedDesc.FolderName = item.Name;
                List<MaskDescriptionModel> ListmappedDesc = new List<MaskDescriptionModel>();
                foreach (var types in item.FolderChildren)
                {
                    var maskDesc = GetMaskDescription(types.Source.Id);
                    MaskDescriptionModel childMapping = new MaskDescriptionModel();
                    childMapping.TreeView = MapMaskDescriptionModel(maskDesc);
                    childMapping.Typename = types.Name;
                    ListmappedDesc.Add(childMapping);

                }
                mappedDesc.TreeView = ListmappedDesc;
                Tabmodel.Add(new ObjectitemTabTree(mappedDesc));
            }
            if (selectedData.Count == 0)
            {
                System.Windows.MessageBox.Show("Please Select Some Data!!!");
            }
            else
            {
                ImportWindow importPage = new ImportWindow();
                importPage.DataContext = new ImportWindowViewModel(this.myApplication, Tabmodel, attModel);
                importPage.ShowDialog();
            }
        }

        private List<ObjectItemTypeDefinationModel> GetAllSelectedChildrens(List<ObjectItemTypeDefinationModel> Tree)
        {
            var selectedData = Tree.Where(x => x.FolderChildren.Any(f => f.Checkbox == true)).ToList();
            selectedData.ForEach(u => { u.FolderChildren.RemoveAll(a => a.Checkbox != true); });
            selectedData.ForEach(x => { x.FolderChildren.ForEach(l => l.FolderChildren.RemoveAll(f => f.Checkbox != true)); });
            return selectedData;
        }

        private int _i = 0;
        private void WriteDataToExcel(List<ObjectItemTypeDefinationModel> selectedData)
        {
            var _filePath = string.Empty;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                _filePath = dlg.FileName;
            }
            if (!String.IsNullOrEmpty(_filePath))
            {
                try {
                    using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(_filePath, SpreadsheetDocumentType.Workbook))
                    {

                        // Add a WorkbookPart to the document.
                        WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                        workbookpart.Workbook = new Workbook();
                        //workbookpart.Workbook.Save();
                        using (WaitDialog wait = myApplication.Dialogs.CreateWaitDialog())
                        {
                            wait.ShowDialogAsync(500, "Please wait...", AnimationType.FileMove);


                            foreach (var obj in selectedData)
                            {
                                foreach (var child in obj.FolderChildren)
                                {
                                    if (child.FolderChildren.Count == 0)
                                    {
                                        _sheetname = child.Name;
                                        var newSheet = CreateNewWorkSheet(spreadsheetDocument, _sheetname);
                                        AddrowsToNewWorkSheet(child, spreadsheetDocument, _sheetname);
                                    }
                                    else
                                    {
                                        foreach (var StackChild in child.FolderChildren)
                                        {
                                            _sheetname = StackChild.Name;
                                            var newSheet = CreateNewWorkSheet(spreadsheetDocument, _sheetname);
                                            AddrowsToNewWorkSheet(StackChild, spreadsheetDocument, _sheetname);
                                            wait.UpdateDialog(_i, _sheetname + " " + ": Exported");
                                        }

                                    }

                                    wait.UpdateDialog(_i, _sheetname + " " + "Exported");

                                }
                                wait.UpdateDialog(_i, "Next Type Defination Folder");
                            }
                            wait.UpdateDialog(500, "Almost there");
                            wait.CloseDialog();
                            System.Windows.MessageBox.Show("Successfully Exported All Selected Data");
                        }

                        // Close the document.
                        spreadsheetDocument.Close();
                    }
                }
                catch { System.Windows.MessageBox.Show("Close all excel files"); }
                }
            else
                System.Windows.MessageBox.Show("File Path Invalid");
        }

        private DocumentFormat.OpenXml.Spreadsheet.Sheet CreateNewWorkSheet(SpreadsheetDocument spreadSheet, string NewSheetName)
        {
            // Add a blank WorksheetPart.
            WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new SheetData());
            //gets sheet 
            Sheets sheets = spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
            //add a blank sheet
            if(sheets == null)
                sheets = spreadSheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            string relationshipId = spreadSheet.WorkbookPart.GetIdOfPart(newWorksheetPart);

            // Get a unique ID for the new worksheet.
            uint sheetId = 1;
            if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            // Append the new worksheet and associate it with the workbook.
            DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = NewSheetName };
            sheets.Append(sheet);
            newWorksheetPart.Worksheet.Save();
            spreadSheet.WorkbookPart.Workbook.Save();
            return sheet;
        }

        private void AddrowsToNewWorkSheet(ObjectItemTypeDefinationModel sheetObject, SpreadsheetDocument spreadsheetDocument, string CurrentSheetName)
        {

            IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Sheet> Sheets = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Where(s => s.Name == CurrentSheetName);
            string relationshipId = Sheets.First().Id.Value;
            WorksheetPart worksheetPart = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(relationshipId);
            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            //First Time Row Creation for Header
            int rowCount = 1;
            Row HeaderRow = new Row();


            HeaderRow.Append(CreateNewStringCell("A", rowCount, "Designation"));
            HeaderRow.Append(CreateNewStringCell("B", rowCount, "AttributeID"));
            HeaderRow.Append(CreateNewStringCell("C", rowCount, "Function"));
            sheetData.Append(HeaderRow);

            ObjectItem currentType = searchTypeFromDefination(sheetObject.Name);
            var maskDescription = GetMaskDescription(currentType.Id);

            List<ObjectMaskDescription> AttributeAndTabName = new List<ObjectMaskDescription>();
            List<AttributeId> attributesOnTab = new List<AttributeId>();
          

           var tabsAndAtt = MapMaskDescriptionModel(maskDescription);

            foreach (var obj in tabsAndAtt)
            {
                Row row = new Row();
                rowCount++;
                
                row.Append(CreateNewStringCell("A", rowCount, obj.TabName.Name));
                row.Append(CreateNewStringCell("B", rowCount, "Tab Name"));
               
                sheetData.Append(row);

                var AttributeObject = searchAttribute(obj.TabAttributes);

                if (AttributeObject != null)
                {
                    foreach (var att in AttributeObject)
                    {
                        Row Attrow = new Row();
                        rowCount++;
                        _i++;
                        if (att != null)
                        {
                            Attrow.Append(CreateNewStringCell("A", rowCount, att.Name));
                            var AttID = att.Attributes.Where(x => x.Name.Equals("AttributeID")).FirstOrDefault();

                            if (AttID != null)
                                Attrow.Append(CreateNewCell("B", rowCount, AttID.Value.ToString()));
                            else
                                Attrow.Append(CreateNewStringCell("B", rowCount, "ID Not Found"));
                        }

                        sheetData.Append(Attrow);
                    }
                }
            }

        }

        private List<MaskDescriptionModel> MapMaskDescriptionModel(IList<ObjectMaskDescription> maskDescription)
        {
            List<MaskDescriptionModel> tabsAndAtt = new List<MaskDescriptionModel>();
            MaskDescriptionModel singletab = new MaskDescriptionModel();
            foreach (var obj in maskDescription)
            {
                if (obj.ID.Equals(AttributeId.Unspecified))
                {
                    if (singletab.TabAttributes != null)
                        tabsAndAtt.Add(singletab);

                    singletab = new MaskDescriptionModel();
                    singletab.TabName = obj;
                }

                if (singletab.TabAttributes == null)
                    singletab.TabAttributes = new List<ObjectMaskDescription>();

                if (!obj.ID.Equals(AttributeId.Unspecified))
                    singletab.TabAttributes.Add(obj);

            }

            if(singletab != null)
            tabsAndAtt.Add(singletab);

            return tabsAndAtt;

        }

        private IList<ObjectMaskDescription> GetMaskDescription(string Typeid)
        {
            IList<ObjectMaskDescription> maskData = new List<ObjectMaskDescription>();
            ExApplication exApp = this.myApplication as ExApplication;
            exApp.ExtendedUtils.GetObjectsMaskDescription(0, Typeid, ref maskData);
  
            return maskData;
        }

        private ObjectItem searchTypeFromDefination(string typeName)
        {
          
            FilterExpression fe = myApplication.CreateFilter();
            fe.Add(AttributeId.Designation, BinaryOperator.Equal, typeName);
            var typeObj = myApplication.Folders.TypeDefinitions.FindObjects(fe, SearchBehavior.Hierarchical, true).FirstOrDefault();

            return typeObj;
        }

        private ObjectCollection searchAttribute(List<ObjectMaskDescription> Attributes)
        {
            ObjectCollection AttObj = null;
            MultiFilterExpression fa = myApplication.CreateMultiFilter();
            int i = 0;
            foreach (var item in Attributes)
            {
                i++;
                FilterExpression fe = myApplication.CreateFilter();
                fe.Add(AttributeId.Designation, BinaryOperator.Equal, item.Name);
                fa.AddFilter(fe);
            }
            return AttObj;
        }

        private List<ObjectItem> GetAllAttribute()
        {
            List<ObjectItem> AttObj = null;
            return AttObj = myApplication.Folders.Attributes.Children.ToList(); ;

        }
        

        private Cell CreateNewCell(string columnName, int rowNumber, string cellValue)
        {
            Cell cell = new Cell()
            {
                DataType = CellValues.Number,
                CellReference = columnName + rowNumber,
                CellValue = new CellValue(cellValue),
            };


            return cell;
        }

        private Cell CreateNewStringCell(string columnName, int rowNumber, string cellValue)
        {
            Cell cell = new Cell()
            {
                DataType = CellValues.InlineString,
                CellReference = columnName + rowNumber,
                InlineString = new InlineString() { Text = new Text(cellValue) },
            };


            return cell;
        }
    }
}
//foreach (var item in tabNames)
//{
//    string formula = "R20;R-3;{El;A5; {=\""+item+"\" [+ R10\"#\";A3;]OGE;}};";
//    currentType.ExecuteFormula(formula, out string result);

//    if (!string.IsNullOrEmpty(result))
//    {
//        foreach (string stringId in result.Split('#'))
//        {
//            if (int.TryParse(stringId, out int id))
//                attributesOnTab.Add((AttributeId)id);
//        }
//    }
//}
