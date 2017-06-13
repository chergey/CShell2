using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using CShell.Util;

namespace CShell.Modules.Workspace.ViewModels
{
    public class AssemblyGacItemViewModel : PropertyChangedBase
    {
        private readonly AssemblyName _assemblyName;
        public AssemblyGacItemViewModel(AssemblyName assemblyName)
        {
            this._assemblyName = assemblyName;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyOfPropertyChange(()=>IsSelected);}
        }

        public string Name => AssemblyName.Name;

        public string Version => AssemblyName.Version.ToString();

        private string _filePath = null;
        public string FilePath => _filePath ?? (_filePath = GlobalAssemblyCache.FindAssemblyInNetGac(AssemblyName));

        public AssemblyName AssemblyName => _assemblyName;
    }

    public class AssemblyGacViewModel : Screen
    {
        public AssemblyGacViewModel()
        {
            DisplayName = "Add Reference from GAC";
            MaxSelectedAssemblyCount = 20;
            LatestVersionsOnly = true;
        }

        private List<AssemblyGacItemViewModel> _gacItems;
        private List<AssemblyGacItemViewModel> GacItems
        {
            get
            {
                if(_gacItems == null)
                {
                    _gacItems = new List<AssemblyGacItemViewModel>();
                    foreach (var assemblyName in GlobalAssemblyCache.GetAssemblyList())
                    {
                        var vm = new AssemblyGacItemViewModel(assemblyName);
                        vm.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == "IsSelected")
                            {
                                NotifyOfPropertyChange(() => SelectedAssemblies);
                                NotifyOfPropertyChange(() => SelectedAssemblyCount);
                                NotifyOfPropertyChange(() => CanOk);
                            }
                        };
                        _gacItems.Add(vm);
                    }
                }
                return _gacItems;
            }
        }

        public IEnumerable<AssemblyGacItemViewModel> Assemblies
        {
            get
            {
                IEnumerable<AssemblyGacItemViewModel> items = GacItems;
                if (LatestVersionsOnly)
                    items = items.GroupBy(item => item.Name).Select(group => group.OrderByDescending(item => item.AssemblyName.Version).First());

                if (string.IsNullOrEmpty(_searchText))
                    return items;
                else
                    return items.Where(item => item.Name.ToLower().Contains(_searchText.ToLower()));
            }
        }

        public IEnumerable<AssemblyGacItemViewModel> SelectedAssemblies
        {
            get { return GacItems.Where(item => item.IsSelected); }
        }

        public int SelectedAssemblyCount => SelectedAssemblies.Count();

        /// <summary>
        /// Gets or sets the max count of allowed assemblies to be selected at once.
        /// </summary>
        public int MaxSelectedAssemblyCount { get; set; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                NotifyOfPropertyChange(() => Assemblies);
            }
        }

        private bool _latestVersionsOnly;
        public bool LatestVersionsOnly
        {
            get { return _latestVersionsOnly; }
            set
            {
                _latestVersionsOnly = value;
                NotifyOfPropertyChange(() => LatestVersionsOnly);
                NotifyOfPropertyChange(() => Assemblies);
            }
        }

        public bool CanOk => SelectedAssemblies.Any() && SelectedAssemblyCount <= MaxSelectedAssemblyCount;

        public void Ok() => TryClose(true);

        public void Cancel() => TryClose(false);
    }
}
