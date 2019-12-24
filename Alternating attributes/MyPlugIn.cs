using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows.Threading;
using Alternating_attributes.Model;
using Alternating_attributes.ViewModel;
using Aucotec.EngineeringBase.Client.Runtime;


namespace Alternating_attributes
{
    /// <summary>
    /// Implements Wizard Alternate Attribute
    /// </summary>
    [AddIn("Alternate Attribute", Description = "This alternates a types attribute in a bulky format. ", Publisher = "Lamev")]
    public class MyPlugIn : PlugInWizard
    {

        /// <summary>
        /// Runs the wizard.
        /// </summary>
        /// <param name="myApplication">Application object instance</param>	
        public override void Run(Application myApplication)
        {
            MainWindow frm = new MainWindow();
            //    frm.DataContext = new MainWindowViewModel(new List<ObjectItemViewModel>() { new ObjectItemViewModel(myApplication.Selection[0]) });

           var ListOfDefinations = myApplication.TypeDefinitions.ToList();
            List<ObjectItemTypeDefinationModel> model = new List<ObjectItemTypeDefinationModel>();
            //foreach (var item in ListOfDefinations)
            //{
            //    if(!item.Name.Equals("Project System Templates"))
            //    model.Add(new ObjectItemTypeDefinationModel(item));       
            //}

            ListOfDefinations.ForEach(x => { if (x.Name != "Project System Templates") { model.Add(new ObjectItemTypeDefinationModel(x)); }});
            frm.DataContext = new MainWindowViewModel(model,myApplication);

            WindowInteropHelper wih = new WindowInteropHelper(frm);
            wih.Owner = myApplication.ActiveWindow.Handle;
            frm.ShowDialog();

            

            // Make a synchronously shutdown
            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
                Dispatcher.CurrentDispatcher.InvokeShutdown();
        }


    }
}

