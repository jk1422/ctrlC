using Colossal;
using Colossal.IO.AssetDatabase;
using ctrlC.Constants;
using Game;
using Game.Input;
using Game.Modding;
using Game.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace ctrlC
{
    [FileLocation(nameof(ctrlC))]
    [SettingsUIGroupOrder(kSupportGroup, kCategoryGroup, kSliderGroup, kDropdownGroup, kKeybindingGroup)]
    [SettingsUIShowGroupName(kSupportGroup, kCategoryGroup, kSliderGroup, kDropdownGroup, kKeybindingGroup)]
    [SettingsUIKeyboardAction(Mod.kOpenModActionName, ActionType.Button, usages: new string[] { Usages.kMenuUsage, "TestUsage" }, interactions: new string[] { "UIButton" })]
    [SettingsUIGamepadAction(Mod.kOpenModActionName, ActionType.Button, usages: new string[] { Usages.kMenuUsage, "TestUsage" }, interactions: new string[] { "UIButton" })]
    [SettingsUIMouseAction(Mod.kOpenModActionName, ActionType.Button, usages: new string[] { Usages.kMenuUsage, "TestUsage" }, interactions: new string[] { "UIButton" })]
    public class Setting : ModSetting
    {
        public const string kSection = "Main";

        public const string kDevGroup = "Development tools";
        public const string kDevSection = "Dev Tools";

        public const string kSupportGroup = "Button";
        public const string kCategoryGroup = "Toggle";
        public const string kSliderGroup = "Slider";
        public const string kDropdownGroup = "Dropdown";
        public const string kKeybindingGroup = "KeyBinding";
        public Setting(IMod mod) : base(mod)
        {
           
        }

        [SettingsUIButton]
        [SettingsUIButtonGroup("LinkGroup")]
        [SettingsUISection(kSection, kSupportGroup)]
        public bool NavPatreon
        {
            set
            {
                Application.OpenURL(PathConstants.PatreonLink);
            }
        }
        [SettingsUIButton]
        [SettingsUIButtonGroup("LinkGroup")]
        [SettingsUISection(kSection, kSupportGroup)]
        public bool NavX
        {
            set
            {
                Application.OpenURL(PathConstants.XLink);
            }
        }


        [SettingsUIButton]
        [SettingsUISection(kSection, kCategoryGroup)]
        public bool OpenPrefabFolder
        {
            set
            {
                Application.OpenURL(PathConstants.PrefabStoragePath);
            }
        }


        [SettingsUITextInput]
        [SettingsUISection(kSection, kCategoryGroup)]
        public string Category1Name { get; set; } = "Featured";
        [SettingsUITextInput]
        [SettingsUISection(kSection, kCategoryGroup)]
        public string Category2Name { get; set; } = "Category 2";
        [SettingsUITextInput]
        [SettingsUISection(kSection, kCategoryGroup)]
        public string Category3Name { get; set; } = "Category 3";
        [SettingsUITextInput]
        [SettingsUISection(kSection, kCategoryGroup)]
        public string Category4Name { get; set; } = "Category 4";

        [SettingsUISection(kSection, kCategoryGroup)]
        public bool AutoOpenPrefabMenu
        {
            get => _autoOpenPrefabMenu;
            set
            {
                if (_autoOpenPrefabMenu != value)
                {
                    _autoOpenPrefabMenu = value;
                    Mod.AutoOpenPrefabMenu = value;  // Kalla på metoden som uppdaterar den statiska variabeln
                }
            }
        }

        private bool _autoOpenPrefabMenu = true;

        [SettingsUIButton]
        [SettingsUISection(kSection, kCategoryGroup)]
        public bool SaveCatNames
        {
            set
            {
                Mod.ReadCategoryNames(Category1Name, Category2Name, Category3Name, Category4Name);
            }
        }

        

        [SettingsUIKeyboardBinding(BindingKeyboard.C, Mod.kOpenModActionName, shift: true)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding OpenModBinding { get; set; }

        [SettingsUIKeyboardBinding(BindingKeyboard.C, Mod.kCopyActionName, ctrl: true)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding CopyBinding { get; set; }

        [SettingsUIKeyboardBinding(BindingKeyboard.X, Mod.kMirrorActionName, ctrl: true)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding MirrorBinding { get; set; }

        [SettingsUIKeyboardBinding(BindingKeyboard.Space, Mod.kPhotoActionName, ctrl: true)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding PhotoBinding { get; set; }

        [SettingsUISection(kSection, kKeybindingGroup)]
        public bool ResetBindings
        {
            set
            {
                Mod.log.Info("Reset key bindingss");
                ResetKeyBindings();
            }
        }



        [SettingsUISection(kDevSection, kDevGroup)]
        public bool DevMode { get; set; }
        [SettingsUIAdvanced]
        [SettingsUISection(kDevSection, kDevGroup)]
        public SystemUpdatePhase PlacementToolUpdatePhase { get; set; } = SystemUpdatePhase.ToolUpdate;
        [SettingsUIAdvanced]
        [SettingsUISection(kDevSection, kDevGroup)]
        public SystemUpdatePhase SelectionToolUpdatePhase { get; set; } = SystemUpdatePhase.ToolUpdate;
        [SettingsUIAdvanced]
        [SettingsUISection(kDevSection, kDevGroup)]
        public SystemUpdatePhase OverlayRendererUpdatePhase { get; set; } = SystemUpdatePhase.ToolUpdate;

        public override void SetDefaults()
        {
            throw new System.NotImplementedException();
        }

    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetOptionTabLocaleID(Setting.kDevSection), "Dev" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DevMode)), "Dev Mode" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DevMode)), $"This will override the version saftey control and let the mod load even if outdated" },


                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.PlacementToolUpdatePhase)), "Placement Tool Update Phase" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SelectionToolUpdatePhase)), "Selection Tool Update Phase" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OverlayRendererUpdatePhase)), "Overlay renderer Update Phase" },


                { m_Setting.GetSettingsLocaleID(), "ctrlC" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { m_Setting.GetOptionGroupLocaleID(Setting.kSupportGroup), "Support the Developer" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kCategoryGroup), "Categories" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kSliderGroup), "Sliders" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kDropdownGroup), "Dropdowns" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NavPatreon)), "Patreon"},
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.NavPatreon)), "Open the developers Patreon page."},

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NavX)), "Twitter"},
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.NavX)), "Open the developers Twitter page."},

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SaveCatNames)), "Save Names" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.SaveCatNames)), $"Press this magic button to refresh the category names without the need to restart" },
                
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenPrefabFolder)), "Open Prefab Folder" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenPrefabFolder)), $"Open the folder where prefabs is stored on your machine." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Category1Name)), "Category 1 name:" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Category1Name)), $"Here you can adjust names of your categories" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Category2Name)), "Category 2 name:" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Category2Name)), $"Here you can adjust names of your categories" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Category3Name)), "Category 3 name:" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Category3Name)), $"Here you can adjust names of your categories" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Category4Name)), "Category 4 name:" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Category4Name)), $"Here you can adjust names of your categories" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPrefabMenu)), $"Auto open prefab menu"},
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPrefabMenu)), $"Automatically open prefab menu when opening the mod."},

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenModBinding)), "Mod Key" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenModBinding)), $"Keyboard binding for opening the mod" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CopyBinding)), "Copy Key" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.CopyBinding)), $"Keyboard binding for copying stuff" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MirrorBinding)), "Mirror Key" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.MirrorBinding)), $"Keyboard binding for mirroring the copied stuff" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetBindings)), "Reset key bindings" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetBindings)), $"Reset all key bindings of the mod" },

                { m_Setting.GetBindingKeyLocaleID(Mod.kOpenModActionName), "Open Mod Binding" },
                { m_Setting.GetBindingKeyLocaleID(Mod.kCopyActionName), "Copy Binding" },
                { m_Setting.GetBindingKeyLocaleID(Mod.kMirrorActionName), "Mirror Binding" },


                { m_Setting.GetBindingMapLocaleID(), "Mod settings sample" },
            };
        }

        public void Unload()
        {

        }
    }
}
