﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.UI;
using KeePass.Util;
using KeeTheme.Editor;

namespace KeeTheme.Options
{
	public partial class OptionsPanel : UserControl
	{
		private readonly KeeThemeOptions _options;

		public OptionsPanel()
		{
			InitializeComponent();

			if (!WinUtil.IsAtLeastWindows10)
				autoSyncWin10ThemeCheckBox.Enabled = false;
		}

		private OptionsPanel(KeeThemeOptions options) : this()
		{
			_options = options;

			LoadOptions();
		}

		public static void Create(OptionsForm optionsForm, KeeThemeOptions options)
		{
			var controls = optionsForm.Controls.Find("m_tabMain", false);
			if (controls.Length != 1)
				return;

			var tabControl = controls[0] as TabControl;
			if (tabControl == null)
				return;

			if (tabControl.ImageList == null)
			{
				tabControl.ImageList = new ImageList();
				tabControl.ImageList.ImageSize = new Size(DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
			}
			var imageIndex = tabControl.ImageList.Images.Add(Properties.Resource.PluginIcon, Color.Transparent);
			
			var optionsPanel = new OptionsPanel(options);
			var tabPage = new TabPage("KeeTheme");
			tabPage.ImageIndex = imageIndex;
			tabPage.Controls.Add(optionsPanel);
			tabPage.UseVisualStyleBackColor = true;
			tabControl.TabPages.Add(tabPage);
			optionsPanel.Dock = DockStyle.Fill;

			if (TemplateEditorForm.Instance != null)
			{
				TemplateEditorForm.Instance.PreviewButtonClick += optionsPanel.HandleTemplateEditorPreviewButtonClick;
				TemplateEditorForm.Instance.Closed += optionsPanel.HandleTemplateEditorPreviewButtonClick;
			}

			optionsForm.FormClosed += (sender, args) =>
			{
				if (optionsForm.DialogResult == DialogResult.OK)
					optionsPanel.SaveOptions();
				
				if (TemplateEditorForm.Instance != null)
				{
					TemplateEditorForm.Instance.PreviewButtonClick -= optionsPanel.HandleTemplateEditorPreviewButtonClick;
					TemplateEditorForm.Instance.Closed -= optionsPanel.HandleTemplateEditorPreviewButtonClick;
				}
			};
		}

		private void LoadOptions()
		{
			hotKeyTextBox.HotKey = _options.HotKey;
			autoSyncWin10ThemeCheckBox.Checked = _options.AutoSyncWithWin10Theme;
			LoadKeeThemeTemplates();
		}

		private void LoadKeeThemeTemplates()
		{
			var templates = TemplateReader.GetTemplatesFromResources();
			var fileTemplates = TemplateReader.GetTemplatesFromPluginsDir();
			templates.AddRange(fileTemplates);

			var selectedTemplate = templates.Find(x => x.Path == _options.Template);
			if (selectedTemplate == null)
			{
				var templateName = TemplateReader.GetTemplateName(_options.Template);
				if (templateName != null)
				{
					var templateFile = new TemplateFile();
					templateFile.Name = templateName;
					templateFile.Path = _options.Template;
					templates.Add(templateFile);
					selectedTemplate = templateFile;
				}
			}

			themeTemplateComboBox.Items.Clear();
			foreach (var template in templates)
			{
				themeTemplateComboBox.Items.Add(template);
			}

			themeTemplateComboBox.SelectedItem = selectedTemplate ?? templates
				.First(x => x.Path == TemplateReader.DefaultTemplatePath);
		}

		private void SaveOptions()
		{
			_options.HotKey = hotKeyTextBox.HotKey;
			_options.AutoSyncWithWin10Theme = autoSyncWin10ThemeCheckBox.Checked;

			var template = (TemplateFile) themeTemplateComboBox.SelectedItem;
			_options.Template = template.Path;
		}

		private void HandleEditTemplateButtonClick(object sender, EventArgs e)
		{
			if (TemplateEditorForm.Instance == null)
			{
				var templateEditorForm = TemplateEditorForm.Create(_options);
				templateEditorForm.PreviewButtonClick += HandleTemplateEditorPreviewButtonClick;
				templateEditorForm.Closed += HandleTemplateEditorFormClosed;
				templateEditorForm.Show();
				templateEditorForm.Activate();
			}
		}

		private void HandleTemplateEditorFormClosed(object sender, EventArgs e)
		{
			LoadKeeThemeTemplates();
		}

		private void HandleTemplateEditorPreviewButtonClick(object sender, EventArgs e)
		{
			LoadKeeThemeTemplates();
		}
	}
}