﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl, IDisposable {
        private readonly IRToolsSettings _settings;
        private readonly IDataObjectEvaluator _evaluator;
        private readonly IREnvironmentProvider _environmentProvider;
        private readonly IObjectDetailsViewerAggregator _aggregator;

        private ObservableTreeNode _rootNode;

        private static List<REnvironment> _defaultEnvironments = new List<REnvironment>() { new REnvironment(Package.Resources.VariableExplorer_EnvironmentName) };

        public VariableView() : this(null) { }

        public VariableView(IRToolsSettings settings) {
            _settings = settings;

            InitializeComponent();

            _evaluator = VsAppShell.Current.ExportProvider.GetExportedValue<IDataObjectEvaluator>();
            _aggregator = VsAppShell.Current.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>();

            SetRootNode(VariableViewModel.Ellipsis);
            EnvironmentComboBox.ItemsSource = _defaultEnvironments;
            EnvironmentComboBox.SelectedIndex = 0;

            SortDirection = ListSortDirection.Ascending;
            RootTreeGrid.Sorting += RootTreeGrid_Sorting;

            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var rSession = sessionProvider.GetInteractiveWindowRSession();

            _environmentProvider = new REnvironmentProvider(rSession);
            _environmentProvider.EnvironmentChanged += EnvironmentProvider_EnvironmentChanged;
        }

        public void Dispose() {
            RootTreeGrid.Sorting -= RootTreeGrid_Sorting;

            if (_environmentProvider != null) {
                _environmentProvider.EnvironmentChanged -= EnvironmentProvider_EnvironmentChanged;
            }
        }

        private void RootTreeGrid_Sorting(object sender, DataGridSortingEventArgs e) {
            // SortDirection
            if (SortDirection == ListSortDirection.Ascending) {
                SortDirection = ListSortDirection.Descending;
            } else {
                SortDirection = ListSortDirection.Ascending;
            }

            _rootNode.Sort();
            e.Handled = true;
        }

        private void EnvironmentProvider_EnvironmentChanged(object sender, REnvironmentChangedEventArgs e) {
            int selectedIndex = 0;

            var currentItem = EnvironmentComboBox.SelectedItem as REnvironment;
            if (currentItem != null && e.Environments.Count > 0 && !e.Environments[0].FrameIndex.HasValue) {
                for (int i = 1; i < e.Environments.Count; i++) {
                    if (e.Environments[i].Name == currentItem.Name) {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            EnvironmentComboBox.ItemsSource = e.Environments;
            EnvironmentComboBox.SelectedIndex = selectedIndex;
        }

        private void EnvironmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if ((EnvironmentComboBox.ItemsSource != _defaultEnvironments) && (e.AddedItems.Count > 0)) {
                var env = e.AddedItems[0] as REnvironment;
                if (env != null) {
                    SetRootModelAsync(env).DoNotWait();
                }
            }
        }

        private async Task SetRootModelAsync(REnvironment env) {
            await TaskUtilities.SwitchToBackgroundThread();
            const REvaluationResultProperties properties = ClassesProperty | ExpressionProperty | TypeNameProperty | DimProperty | LengthProperty;

            var result = await _evaluator.EvaluateAsync(GetExpression(env), properties, RValueRepresentations.Str());
            if (result != null) {
                var wrapper = new VariableViewModel(result, _aggregator);
                var rootNodeModel = new VariableNode(_settings, wrapper);
                VsAppShell.Current.DispatchOnUIThread(() => _rootNode.Model = rootNodeModel);
            }
        }

        private string GetExpression(REnvironment env) {
            if (env.FrameIndex.HasValue) {
                return Invariant($"sys.frame({env.FrameIndex.Value})");
            } else {
                return Invariant($"as.environment({env.Name.ToRStringLiteral()})");
            }
        }

        private void SetRootNode(VariableViewModel evaluation) {
            _rootNode = new ObservableTreeNode(
                new VariableNode(_settings, evaluation),
                Comparer<ITreeNode>.Create(Comparison));

            RootTreeGrid.ItemsSource = new TreeNodeCollection(_rootNode).ItemList;
        }

        private ListSortDirection SortDirection { get; set; }

        private int Comparison(ITreeNode left, ITreeNode right) {
            return VariableNode.Comparison((VariableNode)left, (VariableNode)right, SortDirection);
        }

        private void GridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            HandleDefaultAction();
        }

        private void RootTreeGrid_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                HandleDefaultAction();
            } else if(e.Key == Key.Delete || e.Key == Key.Back) {
                DeleteCurrentVariableAsync().DoNotWait();
            }
        }

        private void HandleDefaultAction() {
            var node = RootTreeGrid.SelectedItem as ObservableTreeNode;
            var ew = node?.Model?.Content as VariableViewModel;
            if (ew != null && ew.CanShowDetail) {
                ew.ShowDetailCommand.Execute(ew);
            }
        }

        private Task DeleteCurrentVariableAsync() {
            var node = RootTreeGrid.SelectedItem as ObservableTreeNode;
            var ew = node?.Model?.Content as VariableViewModel;
            return ew != null ? ew.DeleteAsync() : Task.CompletedTask;
        }
    }
}