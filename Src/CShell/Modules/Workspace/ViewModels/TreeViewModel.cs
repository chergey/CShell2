using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace CShell.Modules.Workspace.ViewModels
{
    public class TreeViewModel : PropertyChangedBase, IHaveDisplayName, IDisposable
    {
        private string _displayName;
        public virtual string DisplayName
        {
            get { return _displayName; }
            set
            {
                //if while a rename ESC is pressed the DisplayName will be set to null 
                if(value == null)
                    return;
                _displayName = value;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public virtual Uri IconSource => new Uri("pack://application:,,,/CShell;component/Resources/Icons/page_white.png");

        private readonly IObservableCollection<TreeViewModel> _children = new BindableCollection<TreeViewModel>();
        public virtual IObservableCollection<TreeViewModel> Children => _children;

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; ; NotifyOfPropertyChange(() => IsSelected); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                NotifyOfPropertyChange(() => IsExpanded);
                NotifyOfPropertyChange(() => IconSource);
            }
        }

        private bool _isEditable = true;
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;
                NotifyOfPropertyChange(() => IsEditable);
            }
        }

        private bool _isInEditMode = false;
        public virtual bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                var previousMode = _isInEditMode;
                _isInEditMode = value;
                if(previousMode == true && _isInEditMode == false)
                    EditModeFinished();
                NotifyOfPropertyChange(() => IsInEditMode);
            }
        }

        protected virtual void EditModeFinished()
        {
            
        }

        /// <summary>
        /// Recursively returns all children in the tree.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeViewModel> GetAllChildren()
        {
            foreach (var treeViewModel in Children)
            {
                yield return treeViewModel;
                foreach (var subTreeViewModel in treeViewModel.GetAllChildren())
                {
                    yield return subTreeViewModel;
                }
            }
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_children != null)
            {
                foreach (var child in Children)
                {
                    child.Dispose(disposing);
                }
            }
        }
    }
}
