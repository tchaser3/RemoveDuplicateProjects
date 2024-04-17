/* Title:           Find Project By Not Entered
 * Date:            6-16-17
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
using NewEventLogDLL;
using ProjectsDLL;

namespace RemoveDuplicateProjects
{
    /// <summary>
    /// Interaction logic for FindProjectsByNotEntered.xaml
    /// </summary>
    public partial class FindProjectsByNotEntered : Window
    {
        WPFMessagesClass TheMessagesClasses = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        ProjectClass TheProjectClass = new ProjectClass();
        
        //setting up the data
        FindProjectByProjectNameDataSet TheFindProjectsByNameDataSet = new FindProjectByProjectNameDataSet();
        
        public FindProjectsByNotEntered()
        {
            InitializeComponent();
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //getting ready for the loop
            int intCounter;
            int intNumberOfRecords;
            int intProjectID;
            string strAssignedProjectID;
            string strProjectName;
            bool blnFatalError = false;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                intNumberOfRecords = TheFindProjectsByNameDataSet.FindProjectByProjectName.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    intProjectID = TheFindProjectsByNameDataSet.FindProjectByProjectName[intCounter].ProjectID;
                    strAssignedProjectID = TheFindProjectsByNameDataSet.FindProjectByProjectName[intCounter].AssignedProjectID.ToUpper();
                    strProjectName = TheFindProjectsByNameDataSet.FindProjectByProjectName[intCounter].ProjectName.ToUpper();

                    blnFatalError = TheProjectClass.UpdateProjectProject(intProjectID, strAssignedProjectID, strProjectName);
                }

                UpdateProjectGrid();
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Projects By Not Entered // Process Button " + Ex.Message);

                TheMessagesClasses.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClasses.CloseTheProgram();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProjectGrid();
        }
        private void UpdateProjectGrid()
        {
            try
            {
                TheFindProjectsByNameDataSet = TheProjectClass.FindProjectByProjectName("NOT ENTERED");

                dgrProjects.ItemsSource = TheFindProjectsByNameDataSet.FindProjectByProjectName;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Remove Duplicate Projects // Find Projects By Not Entered // Update Project Grid " + Ex.Message);

                TheMessagesClasses.ErrorMessage(Ex.ToString());
            }
        }
    }
}
