﻿using System;
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

namespace TiMP_Project_OnlineChat
{
    /// <summary>
    /// Логика взаимодействия для NewConnectionWindow.xaml
    /// </summary>
    public partial class NewConnectionWindow : Window
    {
        public NewConnectionWindow(NewConnectionWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
