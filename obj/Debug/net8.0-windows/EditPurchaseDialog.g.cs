﻿#pragma checksum "..\..\..\EditPurchaseDialog.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3E80EBE3A047E38F137F14A2D1843AF0BA084C3B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SimpleLoginWPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SimpleLoginWPF {
    
    
    /// <summary>
    /// EditPurchaseDialog
    /// </summary>
    public partial class EditPurchaseDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 159 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox PurchaseIdTextBox;
        
        #line default
        #line hidden
        
        
        #line 162 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker PurchaseDatePicker;
        
        #line default
        #line hidden
        
        
        #line 165 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox SupplierComboBox;
        
        #line default
        #line hidden
        
        
        #line 173 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox StatusComboBox;
        
        #line default
        #line hidden
        
        
        #line 182 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox PaymentMethodComboBox;
        
        #line default
        #line hidden
        
        
        #line 195 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DatePicker DeliveryDatePicker;
        
        #line default
        #line hidden
        
        
        #line 198 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TotalAmountTextBox;
        
        #line default
        #line hidden
        
        
        #line 201 "..\..\..\EditPurchaseDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox NotesTextBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.2.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SimpleLoginWPF;component/editpurchasedialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\EditPurchaseDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.2.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.PurchaseIdTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.PurchaseDatePicker = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 3:
            this.SupplierComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.StatusComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.PaymentMethodComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 6:
            this.DeliveryDatePicker = ((System.Windows.Controls.DatePicker)(target));
            return;
            case 7:
            this.TotalAmountTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.NotesTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            
            #line 216 "..\..\..\EditPurchaseDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CancelButton_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 217 "..\..\..\EditPurchaseDialog.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

