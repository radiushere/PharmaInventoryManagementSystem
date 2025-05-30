﻿#pragma checksum "..\..\..\UserProfile.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "322289DF87EF6F9D5429A2E6348BC3372A56BF87"
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
    /// UserProfile
    /// </summary>
    public partial class UserProfile : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 123 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image ProfileImage;
        
        #line default
        #line hidden
        
        
        #line 128 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock UserNameText;
        
        #line default
        #line hidden
        
        
        #line 130 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock UserRoleText;
        
        #line default
        #line hidden
        
        
        #line 132 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock UserEmailText;
        
        #line default
        #line hidden
        
        
        #line 151 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock UserIdText;
        
        #line default
        #line hidden
        
        
        #line 154 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock DepartmentText;
        
        #line default
        #line hidden
        
        
        #line 157 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock PhoneText;
        
        #line default
        #line hidden
        
        
        #line 160 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock JoinDateText;
        
        #line default
        #line hidden
        
        
        #line 169 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel NotificationsPanel;
        
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
            System.Uri resourceLocater = new System.Uri("/SimpleLoginWPF;component/userprofile.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UserProfile.xaml"
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
            this.ProfileImage = ((System.Windows.Controls.Image)(target));
            return;
            case 2:
            this.UserNameText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.UserRoleText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.UserEmailText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.UserIdText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.DepartmentText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.PhoneText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.JoinDateText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.NotificationsPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 10:
            
            #line 214 "..\..\..\UserProfile.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.EditProfile_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 215 "..\..\..\UserProfile.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Close_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

