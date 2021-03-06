﻿using System.Windows;

namespace Playground.WpfApp.Forms.ReactiveEx.Crud
{
    public partial class EmpEditorView
    {
        private readonly EmpEditorViewModel _viewModel;

        public EmpEditorView(int id, string transactionType)
        {
            InitializeComponent();

            _viewModel = new EmpEditorViewModel(id, transactionType);
            DataContext = _viewModel;
            Closing += EmpEditorView_Closing;
        }

        private void EmpEditorView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel.HasUnsavedChanges() == false)
            {
                e.Cancel = false;
                return;
            }

            var result = MessageBox.Show("Unsaved changes found.\nDiscard changes and close?", "Confirm close",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; //cancel closing!
                return;
            }

            e.Cancel = false; //go ahead and close!
        }
    }
}
