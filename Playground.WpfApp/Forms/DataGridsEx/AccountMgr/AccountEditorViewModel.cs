﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Playground.WpfApp.Behaviors;
using Playground.WpfApp.Mvvm;
using Playground.WpfApp.Repositories;
// ReSharper disable PossibleNullReferenceException
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Playground.WpfApp.Forms.DataGridsEx.AccountMgr
{
    public class AccountEditorViewModel : ValidationPropertyChangedBase, ICloseWindow
    {
        public override string Title => _title;
        private readonly string _title;
        private readonly IAccountRepository _repository;
        private readonly AccountModel _backupAccountModel;
        private readonly AccountActionType _actionType;
        private bool _promptToSave = true;
        public bool SaveSuccessful { get; set; }
        public AccountModel Model { get; private set; }

        public List<CategoryModel> AllCategories { get; private set; }

        private CategoryModel _selectedCategory;

        [Required(ErrorMessage = "Category Name is required!")]
        public CategoryModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetPropertyValue(ref _selectedCategory, value);
                if(value != null)
                {
                    Model.CategoryId = value.CategoryId;
                }
                ValidateProperty(value);
            }
        }

        private int _accountId;

        public int AccountId
        {
            get => _accountId;
            set
            {
                SetPropertyValue(ref _accountId, value);
                Model.AccountId = value;
            }
        }

        private string _accountName;

        [Required(ErrorMessage = "Account Name is required!")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Account Name Should be minimum 3 characters and a maximum of 50 characters")]
        [DataType(DataType.Text)]
        public string AccountName
        {
            get => _accountName;
            set
            {
                SetPropertyValue(ref _accountName, value);
                Model.AccountName = value;
                ValidateProperty(value);
            }
        }

        private string _loginId;

        [Required(ErrorMessage = "Login Id is required!")]
        public string LoginId
        {
            get => _loginId;
            set
            {
                SetPropertyValue(ref _loginId, value);
                Model.AccountLoginId = value;
                ValidateProperty(value);
            }
        }

        private string _password;

        [Required(ErrorMessage = "Password is required!")]
        public string Password
        {
            get => _password;
            set
            {
                SetPropertyValue(ref _password, value);
                Model.AccountPassword = value;
                ValidateProperty(value);
            }
        }

        private string _notes;

        public string Notes
        {
            get => _notes;
            set
            {
                SetPropertyValue(ref _notes, value);
                Model.Notes = value;
            }
        }

        public AccountEditorViewModel(IAccountRepository repository, AccountModel model, List<CategoryModel> allCategories, AccountActionType actionType)
        {
            _repository = repository;
            _backupAccountModel = model;
            _actionType = actionType;
            AllCategories = allCategories;
            Model = new AccountModel();

            if (actionType == AccountActionType.New)
            {
                _title = "Create new Account";
                _selectedCategory = null;
                Model.CategoryId = 0;
            }
            else if (actionType == AccountActionType.Edit)
            {
                _title = $"Editing Account: {model.AccountId}/{model.AccountName}";
                _selectedCategory = AllCategories.FirstOrDefault(c => c.CategoryId == model.CategoryId);
                Model.CategoryId = _selectedCategory.CategoryId;
            }

            NotifyPropertyChanged("Title");
            NotifyPropertyChanged("SelectedCategory");

            AccountId = model.AccountId;
            AccountName = model.AccountName;
            LoginId = model.AccountLoginId;
            Password = model.AccountPassword;
            Notes = model.Notes;

            //Validate properties
            ValidateProperty(_selectedCategory, $"SelectedCategory");
            ValidateProperty(_accountName, $"AccountName");
            ValidateProperty(_loginId, $"LoginId");
            ValidateProperty(_password, $"Password");

            //Commands
            CancelCommand = new DelegateCommand(() => OnClose());
            _saveCommand = new DelegateCommand(() => SaveAndClose(), () => CanSave);

            PropertyChanged += AccountEditorViewModel_PropertyChanged;
        }

        private void AccountEditorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsDirty")
            {
                _saveCommand.RaiseCanExecuteChanged();
            }
        }

        private DelegateCommand _saveCommand;

        public DelegateCommand SaveCommand
        {
            get => _saveCommand;
            set => SetPropertyValue(ref _saveCommand, value);
        }

        private bool CanSave =>

            !HasErrors && ErrorCount == 0 &&
            !string.IsNullOrEmpty(AccountName) &&
            !string.IsNullOrEmpty(LoginId) &&
            !string.IsNullOrEmpty(Password) &&
            _selectedCategory != null &&
            HasUnsavedChanges();

        private void SaveAndClose()
        {
            if (_actionType == AccountActionType.New)
            {
                var id = _repository.InsertNewAccount(Model);
                Model.AccountId = id;
                Model.DateCreated = DateTime.Today;
                Model.DateCreated = null;
            }
            else if (_actionType == AccountActionType.Edit)
            {
                _repository.UpdateAccount(Model);
                Model.DateCreated = _backupAccountModel.DateCreated;
                Model.DateModified = DateTime.Today;
            }

            SaveSuccessful = true;
            OnClose();
        }

        public ICommand CancelCommand { get; }

        private void OnClose()
        {
            _promptToSave = false;
            Close?.Invoke();
        }

        #region Closing
        public Action Close { get; set; }

        public bool HasUnsavedChanges()
        {
            if (!_promptToSave) return false;

            if (_actionType == AccountActionType.New) return true;

            if (_backupAccountModel.CategoryId != _selectedCategory.CategoryId ||
                _backupAccountModel.AccountName != AccountName ||
                _backupAccountModel.AccountLoginId != LoginId ||
                _backupAccountModel.AccountPassword != Password ||
                _backupAccountModel.Notes != Notes)
            {
                return true;

            }

            return false;
        }
        public bool CanClose()
        {
            if (!_promptToSave) return true;

            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("Unsaved changes found.\nDiscard changes and close?", "Confirm Close",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return false;
                }
            }

            return true;
        }

        public void DisposeResources()
        {
            _selectedCategory = null;
        }
        #endregion
    }
}
