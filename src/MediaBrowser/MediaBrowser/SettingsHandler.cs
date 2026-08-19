/*
 * MediaBrowser, A Modern version of Windows Media Center
 * Copyright (C) 2022 - 2026 - v38armageddon
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Markup;

namespace MediaBrowser;

public class SettingsHandler
{
    public Dictionary<string, object> LoadSettingsXML()
    {
        string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.xml");
        if (!File.Exists(filePath))
        {
            var doc = new XDocument(
                new XElement("Settings",
                    new XElement("Theme", 0)
                )
            );
            doc.Save(filePath);
        }
        Dictionary<string, object> settings = new Dictionary<string, object>();
        try
        {
            XDocument doc = XDocument.Load(filePath);
            if (doc.Root != null)
            {
                foreach (XElement element in doc.Root.Elements())
                {
                    settings[element.Name.LocalName] = element.Value;
                }
            }

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading settings: {ex}");
        }
        return settings;
    }

    public static void SetTheme(int themeValue)
    {
        // FIXME: Xbox storage is "not available"??
        string filePath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "settings.xml");
        if (File.Exists(filePath))
        {
            XDocument doc = XDocument.Load(filePath);
            if (doc.Root != null)
            {
                XElement theme = doc.Root.Element("Theme");
                if (theme == null)
                {
                    theme = new XElement("Theme");
                    doc.Root.Add(theme);
                }
                theme.Value = themeValue.ToString();
                doc.Save(filePath);
            }
        }
    }
}

public static class  ThemeManager
{
    public static void ApplyInternalTheme(string themeUri)
    {
        var newTheme = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };
        ApplyDictionnary(newTheme);
    }

    public static async void ApplyCustomTheme(string fileName)
    {
        string customThemeFolder = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "custom");
        string filePath = Path.Combine(customThemeFolder, fileName);
        if (!Directory.Exists(customThemeFolder)) {
            Directory.CreateDirectory(customThemeFolder);
        }
        try
        {
            string xamlContent = await File.ReadAllTextAsync(filePath);
            var newTheme = (ResourceDictionary)Microsoft.UI.Xaml.Markup.XamlReader.Load(xamlContent);
            ApplyDictionnary(newTheme);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error parsing custom theme XAML: {ex}");
        }
    }

    private static void ApplyDictionnary(ResourceDictionary newTheme)
    {
        // Theme dictionaries start at index 2, after XamlControlsResources [0] and ToolkitResources [1]
        const int ThemeStartIndex = 2;
        var dicts = Application.Current.Resources.MergedDictionaries;
        while (dicts.Count > ThemeStartIndex)
        {
            dicts.RemoveAt(ThemeStartIndex);
        }
        dicts.Add(newTheme);
    }
}
