/* Title:           Find Project By Name
 * Date:            6-15-17
 * Author:          Terry Holmes */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProjectsDLL;
using NewEventLogDLL;
using ReceivePartsDLL;
using IssuedPartsDLL;
using BOMPartsDLL;

namespace RemoveDuplicateProjects
{
    /// <summary>
    /// Interaction logic for FindProjectByName.xaml
    /// </summary>
    public partial class FindProjectByName : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessageClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        ProjectClass TheProjectsClass = new ProjectClass();
        ReceivePartsClass TheReceivePartsClass = new ReceivePartsClass();
        IssuedPartsClass TheIssuedPartsClass = new IssuedPartsClass();
        BOMPartsClass TheBOMPartsClass = new BOMPartsClass();

        //setting up the data
        FindProjectByProjectNameDataSet TheFindProjectByProjectNameDataSet = new FindProjectByProjectNameDataSet();
        FindReceivedPartsByProjectIDDataSet TheFindReceivedPartsByProjectIDDataset = new FindReceivedPartsByProjectIDDataSet();
        FindIssuedPartsByProjectIDDataSet TheFindIssuedPartsByProjectIDDataSet = new FindIssuedPartsByProjectIDDataSet();
        FindBOMPartsByProjectIDDataSet TheFindBOMPartsByProjectIDDataSet = new FindBOMPartsByProjectIDDataSet();
        DuplicateProjects TheDuplicateProjectsDataSet = new DuplicateProjects();

        public FindProjectByName()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessageClass.CloseTheProgram();
        }

        private void btnFindProjects_Click(object sender, RoutedEventArgs e)
        {
            //this will find the projects
            //setting local variables
            string strProjectName;
            int intCounter;
            int intNumberOfRecords;

            try
            {
                TheDuplicateProjectsDataSet.duplicateprojects.Rows.Clear();

                strProjectName = txtEnterProjectName.Text;
                if(strProjectName == "")
                {
                    TheMessageClass.ErrorMessage("Project Name Was Not Entered");
                    return;
                }

                TheFindProjectByProjectNameDataSet = TheProjectsClass.FindProjectByProjectName(strProjectName);

                intNumberOfRecords = TheFindProjectByProjectNameDataSet.FindProjectByProjectName.Rows.Count - 1;

                if(intNumberOfRecords > -1)
                {
                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        DuplicateProjects.duplicateprojectsRow NewProjectRow = TheDuplicateProjectsDataSet.duplicateprojects.NewduplicateprojectsRow();

                        NewProjectRow.AssignedProjectID = TheFindProjectByProjectNameDataSet.FindProjectByProjectName[intCounter].AssignedProjectID;
                        NewProjectRow.ProjectID = TheFindProjectByProjectNameDataSet.FindProjectByProjectName[intCounter].ProjectID;
                        NewProjectRow.ProjectName = TheFindProjectByProjectNameDataSet.FindProjectByProjectName[intCounter].ProjectName;
                        NewProjectRow.Remove = false;

                        TheDuplicateProjectsDataSet.duplicateprojects.Rows.Add(NewProjectRow);
                    }
                }

                dgrProjects.ItemsSource = TheDuplicateProjectsDataSet.duplicateprojects;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Project By Name // Find Projects Button " + Ex.Message);

                TheMessageClass.ErrorMessage(Ex.ToString());
            }
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //setting variables
            int intCounter;
            int intNumberOfRecords;
            int intProjectIDKept = 0;
            int intProjectIDRemoved;
            bool blnFatalError;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                intNumberOfRecords = TheDuplicateProjectsDataSet.duplicateprojects.Rows.Count - 1;

                //loop to find project id kept
                for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    if (TheDuplicateProjectsDataSet.duplicateprojects[intCounter].Remove == false)
                    {
                        intProjectIDKept = TheDuplicateProjectsDataSet.duplicateprojects[intCounter].ProjectID;
                        break;
                    }
                }

                //loop to find items to remove
                for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    if (TheDuplicateProjectsDataSet.duplicateprojects[intCounter].Remove == true)
                    {
                        intProjectIDRemoved = TheDuplicateProjectsDataSet.duplicateprojects[intCounter].ProjectID;

                        blnFatalError = ChangeReceivedTransactions(intProjectIDKept, intProjectIDRemoved);
                        if (blnFatalError == false)
                            blnFatalError = ChangeIssuedTransacations(intProjectIDKept, intProjectIDRemoved);
                        if (blnFatalError == false)
                            blnFatalError = ChangeBOMTransactions(intProjectIDKept, intProjectIDRemoved);
                        if (blnFatalError == false)
                            blnFatalError = TheProjectsClass.RemoveProjectEntry(intProjectIDRemoved);

                        if (blnFatalError == true)
                        {
                            TheMessageClass.ErrorMessage("The Is A Problem, Contact IT");
                            return;
                        }
                    }
                }

                TheMessageClass.InformationMessage("The Project Has Been Removed");
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Duplicate Projects // Remove Duplicates Button " + Ex.Message);

                TheMessageClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }
        private bool ChangeBOMTransactions(int intProjectIDKept, int intProjectIDRemoved)
        {
            bool blnFatalError = false;
            int intCounter;
            int intNumberOfRecords;
            int intTransactionID;

            try
            {
                TheFindBOMPartsByProjectIDDataSet = TheBOMPartsClass.FindBOMPartsByProjectID(intProjectIDRemoved);

                intNumberOfRecords = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID.Rows.Count - 1;

                if (intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheFindBOMPartsByProjectIDDataSet.FindBOMPartsByProjectID[intCounter].TransactionID;

                        blnFatalError = TheBOMPartsClass.UpdateBOMPartsProjectID(intTransactionID, intProjectIDKept);

                        if (blnFatalError == true)
                        {
                            TheMessageClass.ErrorMessage("There Is A Problem, Contact IT");
                            return blnFatalError;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Duplicate Projects // Change BOM Transactions " + Ex.Message);

                TheMessageClass.ErrorMessage(Ex.ToString());

                blnFatalError = true;
            }

            return blnFatalError = false;
        }
        private bool ChangeIssuedTransacations(int intProjectIDKept, int intProjectIDRemoved)
        {
            bool blnFatalError = false;
            int intCounter;
            int intNumberOfRecords;
            int intTransactionID;

            try
            {
                TheFindIssuedPartsByProjectIDDataSet = TheIssuedPartsClass.FindIssuedPartsByProjectID(intProjectIDRemoved);

                intNumberOfRecords = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID.Rows.Count - 1;

                if (intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheFindIssuedPartsByProjectIDDataSet.FindIssuedPartsByProjectID[intCounter].TransactionID;

                        blnFatalError = TheIssuedPartsClass.UpdateIssuedPartsProjectID(intTransactionID, intProjectIDKept);

                        if (blnFatalError == true)
                        {
                            TheMessageClass.ErrorMessage("There Is A Problem, Contact IT");
                            return blnFatalError;
                        }
                    }
                }

            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Duplicate Projects // Change Issued Transactions " + Ex.Message);
            }

            return blnFatalError;
        }
        private bool ChangeReceivedTransactions(int intProjectIDKept, int intProjectIDRemoved)
        {
            bool blnFatalError = false;
            int intNumberOfRecords;
            int intCounter;
            int intTransactionID;

            try
            {
                TheFindReceivedPartsByProjectIDDataset = TheReceivePartsClass.FindReceivedPartsByProjectID(intProjectIDRemoved);

                //getting the number of records
                intNumberOfRecords = TheFindReceivedPartsByProjectIDDataset.FindReceivedPartsByProjectID.Rows.Count - 1;

                if (intNumberOfRecords > -1)
                {
                    for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheFindReceivedPartsByProjectIDDataset.FindReceivedPartsByProjectID[intCounter].TransactionID;

                        blnFatalError = TheReceivePartsClass.UpdateReceivePartsProjectID(intTransactionID, intProjectIDKept);

                        if (blnFatalError == true)
                        {
                            TheMessageClass.ErrorMessage("There is a Problem, Contact IT");
                            return blnFatalError;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Duplicate Projects // Change Receive Transactions " + Ex.Message);

                TheMessageClass.ErrorMessage(Ex.ToString());

                blnFatalError = true;
            }

            return blnFatalError;
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }
    }
}
