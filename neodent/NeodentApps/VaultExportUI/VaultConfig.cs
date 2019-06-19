using System.Configuration;

namespace VaultExportUI
{
    public class VaultConfig : ApplicationSettingsBase
    {
        private const string VaultuserProperty = "vaultuser";
        private const string VaultpassProperty = "vaultpass";
        private const string VaultserveraddrProperty = "vaultserveraddr";
        private const string VaultserverProperty = "vaultserver";
        private const string BaseRepositoriesProperty = "baseRepositories";
        private const string ExportfileProperty = "exportfile";

        [UserScopedSetting()]
        [DefaultSettingValue("integracao")]
        public string Vaultuser
        {
            get
            {
                return ((string)this[VaultuserProperty]);
            }
            set
            {
                this[VaultuserProperty] = (string)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("brasil2010")]
        public string Vaultpass
        {
            get
            {
                return ((string)this[VaultpassProperty]);
            }
            set
            {
                this[VaultpassProperty] = (string)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("br03s059.straumann.com")]
        public string Vaultserveraddr
        {
            get
            {
                return ((string)this[VaultserveraddrProperty]);
            }
            set
            {
                this[VaultserveraddrProperty] = (string)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("neodent")]
        public string Vaultserver
        {
            get
            {
                return ((string)this[VaultserverProperty]);
            }
            set
            {
                this[VaultserverProperty] = (string)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("$/Neodent/Produção")]
        public string BaseRepositories
        {
            get
            {
                return ((string)this[BaseRepositoriesProperty]);
            }
            set
            {
                this[BaseRepositoriesProperty] = (string)value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("desenhos.xlsx")]
        public string Exportfile
        {
            get
            {
                return ((string)this[ExportfileProperty]);
            }
            set
            {
                this[ExportfileProperty] = (string)value;
            }
        }
    }
}
