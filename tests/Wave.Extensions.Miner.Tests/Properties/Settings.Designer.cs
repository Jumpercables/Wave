﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Wave.Extensions.Miner.Tests.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\ad-kbaesler\\Documents\\GitHub\\Wave\\tests\\Sample Data\\Databases\\SessionMan" +
            "ager.mdb")]
        public string SessionManager {
            get {
                return ((string)(this["SessionManager"]));
            }
            set {
                this["SessionManager"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\ad-kbaesler\\Documents\\GitHub\\Wave\\tests\\Sample Data\\Databases\\WorkflowMa" +
            "nager.mdb")]
        public string WorkflowManager {
            get {
                return ((string)(this["WorkflowManager"]));
            }
            set {
                this["WorkflowManager"] = value;
            }
        }
    }
}
