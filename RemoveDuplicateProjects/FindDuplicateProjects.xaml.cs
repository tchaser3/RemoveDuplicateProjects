/* Title:           Find Duplicate Projects
 * Date:            6-14-17
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
using DataValidationDLL;
using KeyWordDLL;
using ReceivePartsDLL;
using IssuedPartsDLL;
using BOMPartsDLL;

namespace RemoveDuplicateProjects
{
    /// <summary>
    /// Interaction logic for FindDuplicateProjects.xaml
    /// </summary>
    public partial class FindDuplicateProjects : Window
    {
        //setting up the class
        WPFMessagesClass TheMessageClass = new WPFMessagesClass();
        ProjectClass TheProjectClass = new ProjectClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();
        KeyWordClass TheKeyWordClass = new KeyWordClass();
        BOMPartsClass TheBOMPartsClass = new BOMPartsClass();
        IssuedPartsClass TheIssuedPartsClass = new IssuedPartsClass();
        ReceivePartsClass TheReceivePartsClass = new ReceivePartsClass();

        //setting up the data
        ProjectsDataSet TheProjectsDataSet = new ProjectsDataSet();
        DuplicateProjects TheDuplicateProjectsDataSet = new DuplicateProjects();
        DuplicateProjects TheSelectedDuplicateProjects = new DuplicateProjects();
        FindProjectByAssignedProjectIDDataSet TheFindProjectsByAssignedProjectIDDataSet = new FindProjectByAssignedProjectIDDataSet();
        FindIssuedPartsByProjectIDDataSet TheFindIssuedPartsByProjectID = new FindIssuedPartsByProjectIDDataSet();
        FindReceivedPartsByProjectIDDataSet TheFindReceivedPartsByProjectIDDataSet = new FindReceivedPartsByProjectIDDataSet();
        FindBOMPartsByProjectIDDataSet TheFindBOMPartsByProjectIDDataSet = new FindBOMPartsByProjectIDDataSet();

        //setting up the global variables
        int gintDuplicateCounter;
        int gintDuplicateUpperLimit;

        public FindDuplicateProjects()
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TheProjectsDataSet = TheProjectClass.GetProjectsInfo();

            dgrProjects.ItemsSource = TheProjectsDataSet.projects;
        }

        private void btnFindDuplicates_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intFirstCounter;
            int intSecondCounter;
            int intDuplicateCounter;
            int intNumberOfRecords;
            int intProjectID;
            int intSecondProjectID;
            string strProjectName;
            string strAssignedProjectID;
            bool blnAddToComboBox;
            bool blnItemNotFound = true;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                TheProjectsDataSet = TheProjectClass.GetProjectsInfo();

                TheDuplicateProjectsDataSet.duplicateprojects.Rows.Clear();

                intNumberOfRecords = TheProjectsDataSet.projects.Rows.Count - 1;
                cboAssignedProjectID.Items.Clear();
                cboAssignedProjectID.Items.Add("Select Project");
                gintDuplicateCounter = 0;
                gintDuplicateUpperLimit = 0;

                for(intFirstCounter = 0; intFirstCounter <= intNumberOfRecords; intFirstCounter++)
                {
                    strProjectName = TheProjectsDataSet.projects[intFirstCounter].ProjectName;
                    strAssignedProjectID = TheProjectsDataSet.projects[intFirstCounter].AssignedProjectID;
                    intProjectID = TheProjectsDataSet.projects[intFirstCounter].ProjectID;
                    blnAddToComboBox = false;

                    for(intSecondCounter = 0; intSecondCounter <= intNumberOfRecords; intSecondCounter++)
                    {
                        if(strAssignedProjectID == TheProjectsDataSet.projects[intSecondCounter].AssignedProjectID)
                        {
                            intSecondProjectID = TheProjectsDataSet.projects[intSecondCounter].ProjectID;

                            if(intProjectID != intSecondProjectID)
                            {
                                blnItemNotFound = true;

                                if (gintDuplicateCounter > 0)
                                {
                                    for (intDuplicateCounter = 0; intDuplicateCounter <= gintDuplicateUpperLimit; intDuplicateCounter++)
                                    {
                                        if (intSecondProjectID == TheDuplicateProjectsDataSet.duplicateprojects[intDuplicateCounter].ProjectID)
                                        {
                                            blnItemNotFound = false;
                                        }
                                    }
                                }

                                if(blnItemNotFound == true)
                                {
                                    DuplicateProjects.duplicateprojectsRow NewProjectRow = TheDuplicateProjectsDataSet.duplicateprojects.NewduplicateprojectsRow();

                                    NewProjectRow.AssignedProjectID = TheProjectsDataSet.projects[intSecondCounter].AssignedProjectID;
                                    NewProjectRow.ProjectID = intSecondProjectID;
                                    NewProjectRow.ProjectName = TheProjectsDataSet.projects[intSecondCounter].ProjectName;
                                    NewProjectRow.Remove = false;

                                    TheDuplicateProjectsDataSet.duplicateprojects.Rows.Add(NewProjectRow);
                                    gintDuplicateUpperLimit = gintDuplicateCounter;
                                    gintDuplicateCounter++;
                                    blnAddToComboBox = true;
                                }
                            }
                        }
                    }

                    if(blnAddToComboBox == true)
                    {
                        cboAssignedProjectID.Items.Add(strAssignedProjectID);
                    }
                }

                dgrProjects.ItemsSource = TheDuplicateProjectsDataSet.duplicateprojects;
                cboAssignedProjectID.SelectedIndex = 0;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Duplicate Projects // Find Duplicate Button " + Ex.Message);

                TheMessageClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }

        private void cboAssignedProjectID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int intSelectedIndex;
            int intCounter;
            int intNumberOfRecords;
            string strAssignedProjectID;
            
            try
            {
                intSelectedIndex = cboAssignedProjectID.SelectedIndex;

                if(intSelectedIndex > -1)
                {
                    strAssignedProjectID = cboAssignedProjectID.SelectedItem.ToString();

                    if(strAssignedProjectID != "Select Project")
                    {
                        TheSelectedDuplicateProjects.duplicateprojects.Rows.Clear();

                        TheFindProjectsByAssignedProjectIDDataSet = TheProjectClass.FindProjectByAssignedProjectID(strAssignedProjectID);

                        intNumberOfRecords = TheFindProjectsByAssignedProjectIDDataSet.FindProjectByAssignedProjectID.Rows.Count - 1;

                        for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                        {
                            DuplicateProjects.duplicateprojectsRow NewDuplicateRow = TheSelectedDuplicateProjects.duplicateprojects.NewduplicateprojectsRow();

                            NewDuplicateRow.AssignedProjectID = strAssignedProjectID;
                            NewDuplicateRow.ProjectID = TheFindProjectsByAssignedProjectIDDataSet.FindProjectByAssignedProjectID[intCounter].ProjectID;
                            NewDuplicateRow.ProjectName = TheFindProjectsByAssignedProjectIDDataSet.FindProjectByAssignedProjectID[intCounter].ProjectName;
                            NewDuplicateRow.Remove = false;

                            TheSelectedDuplicateProjects.duplicateprojects.Rows.Add(NewDuplicateRow);
                        }

                        dgrProjects.ItemsSource = TheSelectedDuplicateProjects.duplicateprojects;
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Duplicate Projects // Combo Box Selected Change Event " + Ex.Message);

                TheMessageClass.ErrorMessage(Ex.ToString());
            }
        }

        private void btnRemoveDuplicates_Click(object sender, RoutedEventArgs e)
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
                intNumberOfRecords = TheSelectedDuplicateProjects.duplicateprojects.Rows.Count - 1;
                
                //loop to find project id kept
                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    if(TheSelectedDuplicateProjects.duplicateprojects[intCounter].Remove == false)
                    {
                        intProjectIDKept = TheSelectedDuplicateProjects.duplicateprojects[intCounter].ProjectID;
                        break;
                    }
                }

                //loop to find items to remove
                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    if(TheSelectedDuplicateProjects.duplicateprojects[intCounter].Remove == true)
                    {
                        intProjectIDRemoved = TheSelectedDuplicateProjects.duplicateprojects[intCounter].ProjectID;

                        blnFatalError = ChangeReceivedTransactions(intProjectIDKept, intProjectIDRemoved);
                        if (blnFatalError == false)
                            blnFatalError = ChangeIssuedTransacations(intProjectIDKept, intProjectIDRemoved);
                        if (blnFatalError == false)
                            blnFatalError = ChangeBOMTransactions(intProjectIDKept, intProjectIDRemoved);
                        if (blnFatalError == false)
                            blnFatalError = TheProjectClass.RemoveProjectEntry(intProjectIDRemoved);

                        if(blnFatalError == true)
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

                if(intNumberOfRecords > -1)
                {
                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
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
                TheFindIssuedPartsByProjectID = TheIssuedPartsClass.FindIssuedPartsByProjectID(intProjectIDRemoved);

                intNumberOfRecords = TheFindIssuedPartsByProjectID.FindIssuedPartsByProjectID.Rows.Count - 1;

                if(intNumberOfRecords > -1)
                {
                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheFindIssuedPartsByProjectID.FindIssuedPartsByProjectID[intCounter].TransactionID;

                        blnFatalError = TheIssuedPartsClass.UpdateIssuedPartsProjectID(intTransactionID, intProjectIDKept);

                        if(blnFatalError == true)
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
                TheFindReceivedPartsByProjectIDDataSet = TheReceivePartsClass.FindReceivedPartsByProjectID(intProjectIDRemoved);

                //getting the number of records
                intNumberOfRecords = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID.Rows.Count - 1;

                if(intNumberOfRecords > -1)
                {
                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        intTransactionID = TheFindReceivedPartsByProjectIDDataSet.FindReceivedPartsByProjectID[intCounter].TransactionID;

                        blnFatalError = TheReceivePartsClass.UpdateReceivePartsProjectID(intTransactionID, intProjectIDKept);

                        if(blnFatalError == true)
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
