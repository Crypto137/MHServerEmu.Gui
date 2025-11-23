using IniParser;
using IniParser.Model;

namespace MHServerEmu.ServerManagement
{
    /// <summary>
    /// Wrapper around IniParser library.
    /// </summary>
    public class ServerConfig
    {
        private static readonly FileIniDataParser IniParser = new();

        private readonly string _path;
        private IniData _iniData;

        public ServerConfig(string path)
        {
            _path = path;
        }

        public void Load()
        {
            if (File.Exists(_path) == false)
                return;

            _iniData = IniParser.ReadFile(_path);
        }

        public void Save()
        {
            IniParser.WriteFile(_path, _iniData);
        }

        public string GetValue(string section, string key)
        {
            KeyDataCollection sectionData = _iniData[section];

            if (sectionData != null)
                return sectionData[key];

            return null;
        }

        public void SetValue(string section, string key, string value)
        {
            KeyDataCollection sectionData = _iniData[section];

            if (value == null)
            {
                sectionData.RemoveKey(key);

                if (sectionData.Count == 0)
                    _iniData.Sections.RemoveSection(section);

                return;
            }
            else
            {
                sectionData[key] = value;
            }
        }

        public void GetData(Dictionary<string, Dictionary<string, string>> outputDict)
        {
            foreach (SectionData section in _iniData.Sections)
            {
                Dictionary<string, string> sectionDict = new();
                outputDict.Add(section.SectionName, sectionDict);

                foreach (var kvp in section.Keys)
                    sectionDict.Add(kvp.KeyName, kvp.Value);
            }
        }
    }
}
