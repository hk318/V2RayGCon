﻿using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static V2RayGCon.Lib.StringResource;

namespace V2RayGCon.Controller
{
    class Configer : Model.BaseClass.FormController
    {
        Service.Servers servers;

        public JObject config;
        string originalConfig, originalFile;
        ConfigerComponet.Editor editor;

        public Configer(string originalConfig)
        {
            servers = Service.Servers.Instance;

            this.originalFile = string.Empty;
            this.originalConfig = string.Empty;

            LoadConfig(originalConfig);
        }

        #region public method

        public void Prepare()
        {
            editor = GetComponent<ConfigerComponet.Editor>();
            editor.ShowSection();
            Update();
        }

        public bool IsConfigSaved()
        {
            if (editor.IsChanged())
            {
                return false;
            }

            if (string.IsNullOrEmpty(originalConfig)
                && string.IsNullOrEmpty(originalFile))
            {
                return false;
            }

            if (string.IsNullOrEmpty(originalFile))
            {
                JObject orgConfig = JObject.Parse(originalConfig);
                return JObject.DeepEquals(orgConfig, config);
            }

            JObject orgFile = JObject.Parse(originalFile);
            return JObject.DeepEquals(orgFile, config);
        }

        public string GetAlias()
        {
            return Lib.Utils.GetValue<string>(config, "v2raygcon.alias");
        }

        public void Update()
        {
            foreach (var component in this.GetAllComponents())
            {
                (component.Value as Controller.ConfigerComponet.ConfigerComponentController)
                    .Update(config);
            }
        }

        public bool SaveServer()
        {
            return ReplaceServer(originalConfig);
        }

        public bool ReplaceServer(string originalConfig)
        {
            if (!editor.Flush())
            {
                return false;
            }
            Update();

            var newConfig = Lib.Utils.Config2String(config);
            if (originalConfig == newConfig
                || servers.IsServerItemExist(newConfig))
            {
                MessageBox.Show(I18N("DuplicateServer"));
                return false;
            }

            if (servers.ReplaceServerConfig(originalConfig, newConfig))
            {
                MarkOriginalConfig();
            }
            else
            {
                MessageBox.Show(I18N("OrgServNotFound"));
                return false;
            }

            return true;
        }

        public void AddNewServer()
        {
            if (!editor.Flush())
            {
                return;
            }

            Update();

            if (servers.AddServer(Lib.Utils.Config2String(config), ""))
            {
                MarkOriginalConfig();
            }
            else
            {
                MessageBox.Show(I18N("DuplicateServer"));
            }
        }

        public void MarkOriginalConfig()
        {
            originalFile = string.Empty;
            originalConfig = Lib.Utils.Config2String(config);
        }

        public void MarkOriginalFile()
        {
            originalConfig = string.Empty;
            originalFile = GetConfigFormated();
        }

        public string GetConfigFormated()
        {
            return config.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        public bool LoadJsonFromFile(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            try
            {
                var o = JObject.Parse(content);
                if (o == null)
                {
                    return false;
                }
                config = o;
                Update();
                editor.ShowSection();
                MarkOriginalFile();
                return true;
            }
            catch { }
            return false;
        }

        public void LoadServer(string configString)
        {
            editor.DiscardChanges();
            editor.SelectSection(0);
            LoadConfig(configString);
            Update();
            editor.ShowSection();
        }

        public void InjectConfigHelper(Action lambda)
        {
            if (!editor.Flush())
            {
                return;
            }

            lambda?.Invoke();

            Update();
            editor.ShowSection();
        }

        #endregion

        #region private method
        void LoadConfig(string originalConfig)
        {
            JObject o = null;

            if (!string.IsNullOrEmpty(originalConfig))
            {
                o = JObject.Parse(originalConfig);
            }

            if (o == null)
            {
                o = Service.Cache.Instance.tpl.LoadMinConfig();
                Task.Factory.StartNew(
                    () => MessageBox.Show(
                        I18N("EditorCannotLoadServerConfig")));
            }

            config = o;
            MarkOriginalConfig();
        }
        #endregion
    }
}
