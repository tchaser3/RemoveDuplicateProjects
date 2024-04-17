/* Title:           Main Menu
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

namespace RemoveDuplicateProjects
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();

        public MainMenu()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
        }

        private void btnFindProjectsByDID_Click(object sender, RoutedEventArgs e)
        {
            FindDuplicateProjects FindDuplicateProjects = new FindDuplicateProjects();
            FindDuplicateProjects.Show();
            Close();
        }

        private void btnFindProjectsByName_Click(object sender, RoutedEventArgs e)
        {
            FindProjectByName FindProjectByName = new FindProjectByName();
            FindProjectByName.Show();
            Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnFindProjectByNotEnterd_Click(object sender, RoutedEventArgs e)
        {
            FindProjectsByNotEntered FindProjectsByNotEntered = new FindProjectsByNotEntered();
            FindProjectsByNotEntered.Show();
            Close();
        }
    }
}
